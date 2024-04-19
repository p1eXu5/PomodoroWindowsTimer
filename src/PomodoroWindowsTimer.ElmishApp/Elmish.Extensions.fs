module Elmish.Extensions

/// <summary>
/// see <see href="https://zaid-ajaj.github.io/the-elmish-book/#/chapters/commands/async-state"/>
/// </summary>
type Operation<'TArg, 'TRes> =
    | Start of 'TArg
    | Finish of 'TRes

[<RequireQualifiedAccess>]
type Intent<'TIntent> =
    | None
    | Request of 'TIntent

[<RequireQualifiedAccess>]
module Intent =
    let none = Intent.None


let flip f b a = f a b

let map get set f model =
    model |> get |> f |> flip set model

let mapFirst p f originList =
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
            originList
        | a :: ma ->
            if p a then
                (reverseFront |> List.rev) @ (f a :: ma)
            else
                mapFirstRec (a :: reverseFront) ma
    mapFirstRec [] originList
