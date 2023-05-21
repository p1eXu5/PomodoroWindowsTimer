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
    