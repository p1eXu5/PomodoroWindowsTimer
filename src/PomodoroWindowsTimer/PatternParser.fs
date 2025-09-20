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
        let pend = ws .>> nextCharSatisfies (fun ch -> ch = ',' || ch = ']')

        choice [
            pmilliseconds .>> pend |>> (fun ms -> TimeSpan(0, 0, 0, 0, int ms)) |> attempt
            pminOrSec .>> pend |>> (fun m -> TimeSpan(0, int m, 0)) |> attempt
            pminOrSec .>> ws .>> skipChar '.' .>> pend |>> (fun s -> TimeSpan(0, 0, int s)) |> attempt
            phours .>> ws .>> skipChar '.' .>> pend |>> (fun s -> TimeSpan(0, 0, int s)) |> attempt
            pminOrSec .>> ws .>>. pmilliseconds .>> pend |>> (fun (s, ms) -> TimeSpan(0, 0, 0, int s, int ms)) |> attempt
            pminOrSec .>> ws .>>. pminOrSec .>> pend |>> (fun (m, s) -> TimeSpan(0, int m, int s)) |> attempt
            pminOrSec .>> ws .>>. pminOrSec .>> ws .>>. pmilliseconds .>> pend |>> (fun ((m, s), ms) -> TimeSpan(0, 0, int m, int s, int ms)) |> attempt
            phours .>> ws .>>. pminOrSec .>> ws .>>. pminOrSec .>>  pend |>> (fun ((h, m), s) -> TimeSpan(int h, int m, int s)) |> attempt
            phours .>> ws .>>. pminOrSec .>> ws .>>. pminOrSec .>> ws .>>. pmilliseconds .>> pend |>> (fun (((h, m), s), ms) -> TimeSpan(0, int h, int m, int s, int ms))  |> attempt
        ]

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
            paliasTimeSpanNameItem timePointAliases |> attempt
            paliasTimeSpanItem timePointAliases |> attempt
            paliasItem timePointAliases |> attempt
        ]

    let ptimePointSeq (timePointAliases: string seq) =
        sepBy (pitem timePointAliases) (ws >>? skipChar '-' >>? ws >>? nextCharSatisfiesNot ((=) '('))

    let parse (timePointAliases: Alias seq) input =

        let ppatternItemList, ppatternItemListR = createParserForwardedToRef ()

        let ptimePointGroup =
            between (skipChar '(' >>? ws ) (ws >>? skipChar ')') ppatternItemList
            |>> List.concat

        let ptimePointGroupMany =
            ptimePointGroup
            .>> ws
            .>>. pint32
            |>> (fun (l, n) ->
                l |> List.replicate n |> List.concat
            )

        let ptimePointProgram =
            ppatternItemList
            .>> eof
            |>> List.concat

        ppatternItemListR.Value <-
            ws
            >>. sepEndBy
                (choice [
                    ptimePointGroupMany |> attempt
                    ptimePointGroup |> attempt
                    (ptimePointSeq (timePointAliases |> Seq.map Alias.value)) |> attempt
                ])
                (ws >>? skipChar '-' >>? ws)
            .>> ws

        run ptimePointProgram input
        |> function
            | Success (ok,_,_) -> Result.Ok (ok)
            | Failure (err,_,_) -> Result.Error err