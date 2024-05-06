(*



*)

namespace Elmish.Extensions

open System
open System.Threading
open FsToolkit.ErrorHandling
open System.Text.Json.Serialization

[<ReferenceEquality>]
type Cts =
    {
        [<JsonIgnore>]
        CancellationTokenSource: CancellationTokenSource
    }
    member this.Token = this.CancellationTokenSource.Token
    member this.Cancel() = this.CancellationTokenSource.Cancel()
    member this.Dispose() = this.CancellationTokenSource.Dispose()
    interface IDisposable with
         member this.Dispose() = this.Dispose()


/// <summary>
/// see <see href="https://zaid-ajaj.github.io/the-elmish-book/#/chapters/commands/async-state"/>
/// </summary>
type Operation<'TArg, 'TRes> =
    | Start of 'TArg
    | Finish of 'TRes


type AsyncOperation<'Arg, 'Res> =
    | Start of 'Arg
    | Finish of 'Res * operationId: Cts

[<RequireQualifiedAccess>]
type PreparingDeferred<'Preparing,'Retrieved> =
    | Preparing of 'Preparing
    | InProgress of 'Preparing
    | Retrieved of 'Retrieved

[<RequireQualifiedAccess>]
type AsyncPreparingDeferred<'Preparing, 'Retrieved> =
    | Preparing of 'Preparing
    | InProgress of 'Preparing * Cts
    | Retrieved of 'Retrieved

[<RequireQualifiedAccess>]
type Deferred<'Retrieved> =
    | NotRequested
    | InProgress
    | Retrieved of 'Retrieved

[<RequireQualifiedAccess>]
type AsyncDeferred<'Retrieved> =
    | NotRequested
    | InProgress of Cts
    | Retrieved of 'Retrieved

[<RequireQualifiedAccess>]
type Intent<'TIntent> =
    | None
    | Request of 'TIntent

// ----------------------- modules
[<AutoOpen>]
module Helpers =

    open Elmish

    let withCmdNone = fun m -> m, Cmd.none
    
    let flip f b a = f a b


[<AutoOpen>]
module Model =

    let map get set f model =
        model |> get |> f |> flip set model


[<AutoOpen>]
module List =

    open Elmish

    let mapFirst predicate updatef modelList =
        let rec mapFirstRec reverseFront back =
            match back with
            | [] ->
                (*
                    * Conceptually, the correct value to return is
                    * reverseFront |> List.rev
                    * but this is the same as
                    * input
                    * so returning that instead.
                    *)
                modelList
            | a :: ma ->
                if predicate a then
                    (reverseFront |> List.rev) @ (updatef a :: ma)
                else
                    mapFirstRec (a :: reverseFront) ma
        mapFirstRec [] modelList

    let mapFirstCmd predicate updatef modelList =
        let rec mapFirstRec reverseFront cmd back =
            match back with
            | [] ->
                (*
                 * Conceptually, the correct value to return is
                 * reverseFront |> List.rev
                 * but this is the same as
                 * input
                 * so returning that instead.
                 *)
                modelList, cmd
            | model :: tailModels ->
                if predicate model then
                    let (model, cmd) = updatef model
                    (reverseFront |> List.rev) @ (model :: tailModels), cmd
                else
                    mapFirstRec (model :: reverseFront) cmd tailModels
        mapFirstRec [] Cmd.none modelList

    let mapFirstCmdIntent predicate updatef defaultIntent modelList =
        let rec mapFirstRec reverseFront cmd back =
            match back with
            | [] ->
                (*
                 * Conceptually, the correct value to return is
                 * reverseFront |> List.rev
                 * but this is the same as
                 * input
                 * so returning that instead.
                 *)
                modelList, cmd, defaultIntent
            | model :: tailModels ->
                if predicate model then
                    let (model, cmd, intent) = updatef model
                    (reverseFront |> List.rev) @ (model :: tailModels), cmd, intent
                else
                    mapFirstRec (model :: reverseFront) cmd tailModels
        mapFirstRec [] Cmd.none modelList


module Utils =
    open System.Reflection

    let bindingProperties (bindingsType: System.Type) =
        bindingsType.GetProperties(BindingFlags.GetProperty ||| BindingFlags.Instance ||| BindingFlags.Public)
        |> Array.filter (fun pi -> pi.ReflectedType = bindingsType)

    let bindings<'Binding> (bindingsTypeInstance: obj) (bindingsTypeInstanceProperties: System.Reflection.PropertyInfo array) =
        bindingsTypeInstanceProperties
        |> Array.map (fun pi ->
            pi.GetValue(bindingsTypeInstance) :?> 'Binding
        )
        |> Array.toList


[<RequireQualifiedAccess>]
module Cts =

    let init () =
        {
            CancellationTokenSource = new CancellationTokenSource()
        }


[<RequireQualifiedAccess>]
module Intent =
    let none = Intent.None


[<RequireQualifiedAccess>]
module AsyncOperation =

    let start opMsgCtor args = AsyncOperation.Start args |> opMsgCtor

    let startWith args opMsgCtor = AsyncOperation.Start args |> opMsgCtor

    let startUnit opMsgCtor = AsyncOperation.Start () |> opMsgCtor

    let finish cts =
        fun res -> AsyncOperation.Finish (res, cts)

    let finishWithin msgCtor cts =
        fun res -> AsyncOperation.Finish (res, cts) |> msgCtor

[<RequireQualifiedAccess>]
module AsyncPreparingDeferred =

    /// If asyncDeferred is InProgress then cancels it. If asyncDeferred is Retrieved then
    /// returns None
    let tryInProgressWithCancellation<'Preparing, 'Retrieved>
        (asyncDeferred: AsyncPreparingDeferred<'Preparing, 'Retrieved>)
        : (AsyncPreparingDeferred<'Preparing, 'Retrieved> * 'Preparing * Cts) option
        =
        match asyncDeferred with
        | AsyncPreparingDeferred.InProgress (p, cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            let newCts = Cts.init ()
            (AsyncPreparingDeferred.InProgress (p, newCts), p, newCts) |> Some
        | AsyncPreparingDeferred.Preparing p ->
            let newCts = Cts.init ()
            (AsyncPreparingDeferred.InProgress (p, newCts), p, newCts) |> Some
        | _ ->
            None

    /// If asyncDeferred is InProgress then cancels it.
    let forceInProgressWithCancellation<'Preparing, 'Retrieved>
        (preparingf: 'Retrieved -> 'Preparing)
        (asyncDeferred: AsyncPreparingDeferred<'Preparing, 'Retrieved>)
        : (AsyncPreparingDeferred<'Preparing, 'Retrieved> * 'Preparing * Cts)
        =
        let newCts = Cts.init ()
        match asyncDeferred with
        | AsyncPreparingDeferred.InProgress (p, cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            (AsyncPreparingDeferred.InProgress (p, newCts), p, newCts)
        | AsyncPreparingDeferred.Preparing p ->
            (AsyncPreparingDeferred.InProgress (p, newCts), p, newCts)
        | AsyncPreparingDeferred.Retrieved r ->
            let p = r |> preparingf
            (AsyncPreparingDeferred.InProgress (p, newCts), p, newCts)


    /// Is operation cts is equal to in progress deferred cts then return Some with cts disposing
    /// or returns None with disposing operation cts.
    let chooseRetrievedWithin<'Preparing,'Args,'Retrieved>
        (retrievedValue: 'Retrieved)
        (asyncOperationCts: Cts)
        (asyncDeferred: AsyncPreparingDeferred<'Preparing, 'Retrieved>)
        : (AsyncPreparingDeferred<'Preparing, 'Retrieved> * 'Retrieved) option
        =
        match asyncDeferred with
        | AsyncPreparingDeferred.InProgress (_, cts) when obj.ReferenceEquals(cts, asyncOperationCts) ->
            cts.Dispose()
            (AsyncPreparingDeferred.Retrieved retrievedValue, retrievedValue) |> Some
        | _ -> // finished operation that we do not expect
            asyncOperationCts.Dispose() // just dispose operation cts
            None

    /// Is operation cts is equal to in progress deferred cts then return Some with cts disposing.
    /// Or returns None with disposing operation cts.
    let chooseRetrievedResultWithin<'Preparing,'Args,'Retrieved,'Error>
        (asyncOperationResult: Result<'Retrieved,'Error>)
        (asyncOperationCts: Cts)
        (asyncDeferred: AsyncPreparingDeferred<'Preparing, 'Retrieved>)
        : (Result<AsyncPreparingDeferred<'Preparing, 'Retrieved> * 'Retrieved * 'Preparing, 'Error>) option
        =
        match asyncDeferred with
        | AsyncPreparingDeferred.InProgress (preparingModel, cts) when obj.ReferenceEquals(cts, asyncOperationCts) ->
            cts.Dispose()
            asyncOperationResult
            |> Result.map (fun retrievedValue -> (AsyncPreparingDeferred.Retrieved retrievedValue, retrievedValue, preparingModel))
            |> Some
        | _ -> // finished operation that we do not expect
            asyncOperationCts.Dispose() // just dispose operation cts
            None

    /// If asyncDeferred is InProgress then cancels it and returns NotRequested.
    let notRequestedWithCancellation<'Preparing, 'Retrieved>
        (preparingf: 'Retrieved -> 'Preparing)
        (asyncDeferred: AsyncPreparingDeferred<'Preparing, 'Retrieved>)
        =
        match asyncDeferred with
        | AsyncPreparingDeferred.InProgress (p, cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            (AsyncPreparingDeferred.Preparing p)
        | AsyncPreparingDeferred.Retrieved r ->
            (AsyncPreparingDeferred.Preparing (r |> preparingf))
        | _ -> asyncDeferred

    /// Invokes opCts.Dispose(), returns Some () if opCts is equal to AsyncDeferred.InProgress cts.
    let (|InProgressWithCts|_|) (opCts: Cts) (asyncDeferred: AsyncPreparingDeferred<_,_>) =
        opCts.Dispose()
        match asyncDeferred with
        | AsyncPreparingDeferred.InProgress (_, defCts) when obj.ReferenceEquals(opCts, defCts) ->
            Some ()
        | _ -> None

    /// <summary>
    /// Tries to extract AsyncDeferred.Retrieved case value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Value has not been retrieved.</exception>
    let retrievedValue = function
        | AsyncPreparingDeferred.Retrieved v -> v
        | _ -> raise (InvalidOperationException("Value has not been retrieved."))

    /// <summary>
    /// Tries to extract AsyncDeferred.Retrieved case value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Value has not been retrieved.</exception>
    let preparingValue = function
        | AsyncPreparingDeferred.Preparing v
        | AsyncPreparingDeferred.InProgress (v, _) -> v
        | _ -> raise (InvalidOperationException("Value has been retrieved."))

    let disableRetrieved _ =
        raise (InvalidOperationException("Could not transpose from retrieved state."))

    let apply f (asyncPreparingDeffered: AsyncPreparingDeferred<'a,'a>) : AsyncPreparingDeferred<'b,'b> =
        match asyncPreparingDeffered with
        | AsyncPreparingDeferred.Preparing v -> v |> f |> AsyncPreparingDeferred.Preparing
        | AsyncPreparingDeferred.InProgress (v, cts) -> (v |> f, cts) |> AsyncPreparingDeferred.InProgress
        | AsyncPreparingDeferred.Retrieved v -> v |> f |> AsyncPreparingDeferred.Retrieved

    module List =

        let tryFind predicate (deff: AsyncPreparingDeferred<'a list, 'a list>) =
            match deff with
            | AsyncPreparingDeferred.Preparing l
            | AsyncPreparingDeferred.InProgress (l,_)
            | AsyncPreparingDeferred.Retrieved l -> l |> List.tryFind predicate

        let unwrap (deff: AsyncPreparingDeferred<'a list, 'a list>) =
            match deff with
            | AsyncPreparingDeferred.Preparing l
            | AsyncPreparingDeferred.InProgress (l,_)
            | AsyncPreparingDeferred.Retrieved l -> l

[<RequireQualifiedAccess>]
module AsyncDeferred =

    /// If asyncDeferred is InProgress then cancels it.
    let tryInProgressWithCancellation<'Retrieved>
        (asyncDeferred: AsyncDeferred<'Retrieved>)
        : (AsyncDeferred<'Retrieved> * Cts) option
        =
        match asyncDeferred with
        | AsyncDeferred.InProgress (cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            let newCts = Cts.init ()
            (AsyncDeferred.InProgress (newCts), newCts) |> Some
        | AsyncDeferred.NotRequested ->
            let newCts = Cts.init ()
            (AsyncDeferred.InProgress (newCts), newCts) |> Some
        | _ ->
            None

    /// If asyncDeferred is InProgress then cancels it.
    let forceInProgressWithCancellation<'Retrieved>
        (asyncDeferred: AsyncDeferred<'Retrieved>)
        : (AsyncDeferred<'Retrieved> * Cts)
        =
        let newCts = Cts.init ()
        match asyncDeferred with
        | AsyncDeferred.InProgress (cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            (AsyncDeferred.InProgress (newCts), newCts)
        | AsyncDeferred.NotRequested ->
            (AsyncDeferred.InProgress (newCts), newCts)
        | AsyncDeferred.Retrieved r ->
            (AsyncDeferred.InProgress (newCts), newCts)

    /// Is operation cts is equal to in progress deferred cts then return Some with cts disposing
    /// or returns None with disposing operation cts.
    let chooseRetrievedWithin<'Preparing,'Args,'Retrieved>
        (retrievedValue: 'Retrieved)
        (asyncOperationCts: Cts)
        (asyncDeferred: AsyncDeferred<'Retrieved>)
        : (AsyncDeferred<'Retrieved> * 'Retrieved) option
        =
        match asyncDeferred with
        | AsyncDeferred.InProgress (cts) when obj.ReferenceEquals(cts, asyncOperationCts) ->
            cts.Dispose()
            (AsyncDeferred.Retrieved retrievedValue, retrievedValue) |> Some
        | _ -> // finished operation that we do not expect
            asyncOperationCts.Dispose() // just dispose operation cts
            None

    /// Is operation cts is equal to in progress deferred cts then return Some with cts disposing
    /// or returns None with disposing operation cts.
    let chooseRetrievedResultWithin<'Args,'Res,'Retrieved,'Error>
        (asyncOperationResult: Result<'Res,'Error>)
        (asyncOperationCts: Cts)
        (asyncDeferred: AsyncDeferred<'Retrieved>)
        : (Result<AsyncDeferred<'Res> * 'Res,'Error>) option
        =
        match asyncDeferred with
        | AsyncDeferred.InProgress (cts) when obj.ReferenceEquals(cts, asyncOperationCts) ->
            cts.Dispose()
            asyncOperationResult
            |> Result.map (fun retrievedValue -> (AsyncDeferred.Retrieved retrievedValue, retrievedValue))
            |> Some
        | _ -> // finished operation that we do not expect
            asyncOperationCts.Dispose() // just dispose operation cts
            None

    let chooseRetrieved<'Preparing,'Args,'Retrieved> (asyncDeferred: AsyncDeferred<'Retrieved>) : 'Retrieved option =
        match asyncDeferred with
        | AsyncDeferred.Retrieved v -> v |> Some
        | _ -> None

    /// If asyncDeferred is InProgress then cancels it and returns NotRequested.
    let notRequestedWithCancellation<'Retrieved>
        (asyncDeferred: AsyncDeferred<'Retrieved>) =
        match asyncDeferred with
        | AsyncDeferred.InProgress (cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            (AsyncDeferred.NotRequested)
        | AsyncDeferred.Retrieved _ ->
            (AsyncDeferred.NotRequested)
        | _ -> asyncDeferred

    /// If asyncDeferred is InProgress then cancels it.
    let forceRetrievedWithCancellation<'Retrieved>
        (retrievedValue: 'Retrieved)
        (asyncDeferred: AsyncDeferred<'Retrieved>)
        : AsyncDeferred<'Retrieved>
        =
        match asyncDeferred with
        | AsyncDeferred.InProgress (cts) ->
            // CancellationTokenSource is disposed in a Program module,
            // using LastInProgressWithCancellation active pattern below
            // last cts reference contains AsyncOperation Finish message
            cts.Cancel()
            (AsyncDeferred.Retrieved retrievedValue)
        | _ ->
            (AsyncDeferred.Retrieved retrievedValue)


    /// Invokes opCts.Dispose(), returns Some () if opCts is equal to AsyncDeferred.InProgress cts.
    let (|InProgressWithCts|_|) (opCts: Cts) (asyncDeferred: AsyncDeferred<_>) =
        opCts.Dispose()
        match asyncDeferred with
        | AsyncDeferred.InProgress defCts when obj.ReferenceEquals(opCts, defCts) ->
            Some ()
        | _ -> None

    /// <summary>
    /// Tries to extract AsyncDeferred.Retrieved case value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Value has not been retrieved.</exception>
    let retrievedValue = function
        | AsyncDeferred.Retrieved v -> v
        | _ -> raise (InvalidOperationException("Value has not been retrieved."))

    let toOption<'Retrieved> (asyncDeferred: AsyncDeferred<'Retrieved>) : 'Retrieved option =
        asyncDeferred
        |> function
            | AsyncDeferred.Retrieved v -> v |> Some
            | _ -> None

    let map<'a, 'b> (f: 'a -> 'b) = function
        | AsyncDeferred.Retrieved v -> v |> f |> AsyncDeferred.Retrieved
        | AsyncDeferred.NotRequested -> AsyncDeferred.NotRequested
        | AsyncDeferred.InProgress cts -> AsyncDeferred.InProgress cts

    let mapUpdate f = function
        | AsyncDeferred.Retrieved retrieved ->
            let (retrieved', cmd) = retrieved |> f
            (retrieved' |> AsyncDeferred.Retrieved), cmd
        | deff -> deff, Elmish.Cmd.none

