namespace PomodoroWindowsTimer

open System.Text.Json
open System.Text.Json.Serialization
open System.Runtime.CompilerServices
open System.Text.Encodings.Web
open System.Text.Unicode

type JsonString = JsonString of string


[<Extension>]
type JsonHelpers() =
    static let mutable _jsonSerializerOptions = Unchecked.defaultof<_>
    static let mutable _jsonNoIndentSerializerOptions = Unchecked.defaultof<_>

    static member val JsonSerializerOptions =
        match _jsonSerializerOptions with
        | null ->
            let options = JsonSerializerOptions(JsonSerializerDefaults.General)
            _jsonSerializerOptions <- options.SetJsonFSharpOptions()
            _jsonSerializerOptions
        | _ -> _jsonSerializerOptions
        with get

    static member val JsonNoIndentSerializerOptions =
        match _jsonNoIndentSerializerOptions with
        | null ->
            let options = JsonSerializerOptions(JsonSerializerDefaults.General)
            _jsonNoIndentSerializerOptions <- options.SetJsonNoIndentFSharpOptions()
            _jsonNoIndentSerializerOptions
        | _ -> _jsonNoIndentSerializerOptions
        with get

    ///
    static member GetJavaScriptEncoder() =
        JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.Specials)
    
    [<Extension>]
    static member SetJsonFSharpOptions(options: JsonSerializerOptions) =
        options.PropertyNameCaseInsensitive <- true
        options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        options.Converters.Add(JsonStringEnumConverter())
        options.Encoder <- JsonHelpers.GetJavaScriptEncoder()
#if DEBUG
        options.WriteIndented <- true
#endif
        JsonFSharpOptions.Default()
            .WithSkippableOptionFields(SkippableOptionFields.FromJsonSerializerOptions)
            .AddToJsonSerializerOptions(options)
        options

    [<Extension>]
    static member SetJsonNoIndentFSharpOptions(options: JsonSerializerOptions) =
        options.PropertyNameCaseInsensitive <- true
        options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        options.Converters.Add(JsonStringEnumConverter())
        options.Encoder <- JsonHelpers.GetJavaScriptEncoder()

        JsonFSharpOptions.Default()
            .WithSkippableOptionFields(SkippableOptionFields.FromJsonSerializerOptions)
            .AddToJsonSerializerOptions(options)
        options

    /// 
    static member SerializeToJsonString(o: #obj) =
        JsonSerializer.Serialize(o, JsonHelpers.JsonSerializerOptions)
        |> JsonString

    static member Serialize(o: #obj) =
        JsonSerializer.Serialize(o, JsonHelpers.JsonSerializerOptions)

    static member SerializeNoIndent(o: #obj) =
        JsonSerializer.Serialize(o, JsonHelpers.JsonNoIndentSerializerOptions)

    static member Deserialize<'T>(s: string) : 'T =
        match JsonSerializer.Deserialize<'T>(s, JsonHelpers.JsonSerializerOptions) with
        | null -> Unchecked.defaultof<'T>
        | v -> v

