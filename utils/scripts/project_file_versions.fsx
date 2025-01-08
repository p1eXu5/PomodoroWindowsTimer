open System
open System.IO
open System.Xml.Linq
open System.Text.RegularExpressions

type Project = {
    Sdk: string
    TargetFramework: string option

    Version: string option
    AssemblyVersion: string option
    FileVersion: string option
}

let [<Literal>] private PropertyGroup = "PropertyGroup"
let [<Literal>] private TargetFramework = "TargetFramework"
let [<Literal>] private Version = "Version"
let [<Literal>] private AssemblyVersion = "AssemblyVersion"
let [<Literal>] private FileVersion = "FileVersion"

let private tryGetElementValue (element: XElement) (name: string) =
    match element.Element(XName.Get name) with
    | null -> None
    | el -> Some el.Value

let private tryGetBoolElementValue (element: XElement) (name: string) =
    tryGetElementValue element name |> Option.map (fun v -> v.ToLower() = "true")

let private parseProject (projectElement: XElement) =
    let propertyGroup = projectElement.Element(XName.Get PropertyGroup)
    {
        Sdk = projectElement.Attribute(XName.Get "Sdk").Value
        TargetFramework = tryGetElementValue propertyGroup TargetFramework
        Version = tryGetElementValue propertyGroup Version
        AssemblyVersion = tryGetElementValue propertyGroup AssemblyVersion
        FileVersion = tryGetElementValue propertyGroup FileVersion
    }

let private updateElementValue (element: XElement) (name: string) (value: string option) =
    match value with
    | Some v ->
        let child = element.Element(XName.Get name)
        if child = null then
            element.Add(XElement(XName.Get name, v))
        else
            child.Value <- v
    | None -> ()

let private updateProjectXml (projectElement: XElement) (project: Project) =
    let propertyGroup = projectElement.Element(XName.Get PropertyGroup)

    updateElementValue propertyGroup Version project.Version
    updateElementValue propertyGroup AssemblyVersion project.AssemblyVersion  
    updateElementValue propertyGroup FileVersion project.FileVersion

let private insertBreakSpaces (xml: string) =
    let endTagRegex = Regex(@"^\s*<\/")
    xml.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
    |> Array.map(fun s ->
        if
            s.Contains("<PackageId>")
            || s.Contains("<NeutralLanguage>")
            || s.Contains("<PublishRepositoryUrl>")
            || s.Contains("<Description>")
        then
            Environment.NewLine + s
        elif s.Contains("</Description>") then
            s
        elif endTagRegex.IsMatch(s) then
            s + Environment.NewLine
        else s
    )
    |> fun s -> String.Join(Environment.NewLine, s)

let updateProjectVersions (path: string) (nextVersion: string) =
    let versionValues = nextVersion.Split('.')
    let xml = File.ReadAllText(path)
    let doc = XDocument.Parse(xml)
    let projectElement = doc.Element(XName.Get "Project")
    let project =
        {
            parseProject(projectElement) with
                Version = nextVersion |> Some
                AssemblyVersion = $"{versionValues[0]}.{versionValues[1]}" |> Some
                FileVersion = nextVersion + ".0" |> Some
        }
    updateProjectXml projectElement project
    File.WriteAllText(path, doc.ToString() |> insertBreakSpaces)

let getCurrentVersion (path: string) =
    printfn $"Reading %s{path}"
    let xml = File.ReadAllText(path)
    let doc = XDocument.Parse(xml)
    let projectElement = doc.Element(XName.Get "Project")
    parseProject(projectElement)
    |> _.Version
