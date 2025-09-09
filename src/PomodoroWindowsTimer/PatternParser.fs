namespace PomodoroWindowsTimer

open System
open PomodoroWindowsTimer.Types



module PatternParser =

    open FParsec

    let ws = spaces
    let ws1 = spaces1

    let ptimeSpan : Parser<TimeSpan, unit> =
        let pmilliseconds = skipChar '.' >>. ws >>. puint16
        let pminOrSec = skipChar ':' >>. ws >>. puint8
        let phours = puint8

        choice [
            pmilliseconds |>> (fun ms -> TimeSpan(0, 0, 0, 0, int ms))
            pminOrSec .>> ws .>>. pmilliseconds |>> (fun (s, ms) -> TimeSpan(0, 0, 0, int s, int ms))
            pminOrSec .>> ws .>>. pminOrSec .>> ws .>>. pmilliseconds |>> (fun ((m, s), ms) -> TimeSpan(0, 0, int m, int s, int ms))
            phours .>> ws .>>. pminOrSec .>> ws .>>. pminOrSec .>> ws .>>. pmilliseconds |>> (fun (((h, m), s), ms) -> TimeSpan(0, int h, int m, int s, int ms))
        ]
        .>> ws .>> nextCharSatisfies (fun ch -> ch = ',' || ch = ']')

    let palias (timePointAliases: string seq) =
        timePointAliases |> Seq.map pstring |> choice

    let paliasItem (timePointAliases: string seq) =
        palias timePointAliases |>> (Alias.createOrThrow >> PatternParsedItem.Alias)

    let paliasTimeSpanItem (timePointAliases: string seq) =
        palias timePointAliases
        .>> ws
        .>> skipChar '['
        .>> ws
        .>>. ptimeSpan
        .>> ws
        .>> skipChar ']'
        |>> (fun (a, ts) -> (Alias.createOrThrow a, ts) |> PatternParsedItem.AliasTimeSpan)

    let paliasTimeSpanNameItem (timePointAliases: string seq) =
        let isIdentifierChar c = isLetter c || isDigit c || c = '_' || c = ' '
        palias timePointAliases
        .>> ws
        .>> skipChar '['
        .>> ws
        .>>. ptimeSpan
        .>> ws
        .>> skipChar ','
        .>> ws
        .>>. many1SatisfyL isIdentifierChar "time point Name"
        .>> skipChar ']'
        |>> (fun ((a, ts), n) -> (Alias.createOrThrow a, ts, n.Trim()) |> PatternParsedItem.AliasTimeSpanName)

    let pitem (timePointAliases: string seq) =
        choice [
            paliasItem timePointAliases
            paliasTimeSpanItem timePointAliases
            paliasTimeSpanNameItem timePointAliases
        ]

    let ptimePointSeq (timePointAliases: string seq) =
        sepBy (pitem timePointAliases) (ws >>? skipChar '-' >>? ws >>? nextCharSatisfiesNot ((=) '('))

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


    let parse (timePointAliases: Alias seq) input =
        run (ptimePointProgram (timePointAliases |> Seq.map Alias.value)) input
        |> function
            | Success (ok,_,_) -> Result.Ok (ok)
            | Failure (err,_,_) -> Result.Error err