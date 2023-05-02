module PomodoroWindowsTimer.Parser

open FParsec

let ws = spaces
let ws1 = spaces1

let ptimePoint (timePointAliases: string seq) =
    ws >>. (timePointAliases |> Seq.map pstring |> choice)

let ptimePointSeq (timePointAliases: string seq) =
    sepBy (ptimePoint timePointAliases) (ws >>? skipChar '-' >>? ws >>? nextCharSatisfiesNot ((=) '('))

let ptimePointGroup (timePointAliases: string seq) =
    ws
    >>. between (skipChar '(') (ws >>? skipChar ')') (ptimePointSeq timePointAliases)

let ptimePointGroupMany (timePointAliases: string seq) =
    (ptimePointGroup timePointAliases)
    .>> ws
    .>>. pint32
    |>> (fun (l, n) ->
        l |> List.replicate n |> List.concat
    )

let ptimePointProgram (timePointAliases: string seq) =
    ws
    >>. sepEndBy
        (choice [
            (ptimePointGroupMany timePointAliases) |> attempt
            (ptimePointGroup timePointAliases) |> attempt
            (ptimePointSeq timePointAliases) |> attempt
        ])
        (ws >>? skipChar '-')
    .>> ws
    .>> eof
    |>> List.concat


let parse (timePointAliases: string seq) input =
    run (ptimePointProgram timePointAliases) input
    |> function
        | Success (ok,_,_) -> Result.Ok ok
        | Failure (err,_,_) -> Result.Error err