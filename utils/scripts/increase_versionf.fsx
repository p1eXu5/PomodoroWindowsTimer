type IncreaseVersionf =
    | IncreaseMajor of (int -> int -> int -> string)
    | IncreaseMinor of (int -> int -> int -> string)
    | IncreasePatch of (int -> int -> int -> string)
    | Current of (int -> int -> int -> string)

module IncreaseVersionf =

    let private increaseMajor major minor patch =
        sprintf "%i.%i.%i" (major+1) minor patch

    let private increaseMinor major minor patch =
        sprintf "%i.%i.%i" major (minor + 1) patch

    let private increasePatch major minor patch =
        sprintf "%i.%i.%i" major minor (patch + 1)

    let private currentVersionf major minor patch =
        sprintf "%i.%i.%i" major minor patch

    let ofString = function
    | "major" -> increaseMajor |> IncreaseVersionf.IncreaseMajor |> Ok
    | "minor" -> increaseMinor |> IncreaseVersionf.IncreaseMinor |> Ok
    | "patch" -> increasePatch |> IncreaseVersionf.IncreasePatch |> Ok
    | "current" -> currentVersionf  |> IncreaseVersionf.Current |> Ok
    | _ -> Result.Error "Increase function must be set properly. Possible values: major, minor, patch"

    let value = function
        | IncreaseMajor f
        | IncreaseMinor f
        | IncreasePatch f
        | Current f -> f
