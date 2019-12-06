(*** hide ***)
#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

open System
open Deedle
open System.Globalization
open XPlot.GoogleCharts
open FSharp.Data
(**

Dota 2 Data
========================

Games per week
-----------------

We'll need to conver timestamps to dates and dates to week numbers
*)
let toDateTime (timestamp: int) =
    let start = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    start.AddSeconds(float timestamp).ToLocalTime()

let toWeekNumber (d: DateTime) =
    CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetWeekOfYear
        (d, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)

type OpenDotaPlayerData = JsonProvider<"https://api.opendota.com/api/players/71050296/matches">

let dateToYearWeek (m: OpenDotaPlayerData.Root) =
    let d = m.StartTime |> toDateTime
    (d.Year, toWeekNumber d)
(**
Make a series out of the data
*)

let matchesToData (data: OpenDotaPlayerData.Root []) =
    data
    |> Array.map dateToYearWeek
    |> Array.countBy id
    |> Array.where (fun ((y, _), _) -> (y = 2019))
    |> Array.map (fun ((y, w), c) -> (sprintf "%i/%i" y w, c))
    |> Array.sortBy fst
    |> Array.toList
    |> (fun lst ->
    { 1 .. 52 }
    |> Seq.map (sprintf "2019/%0i")
    |> Seq.toList
    |> List.map (fun k ->
        (k,
         lst
         |> List.tryFind (fst >> ((=) k))
         |> Option.map snd
         |> Option.defaultValue 0)))

(**
Load the data, and transform it to have just number of games per "year/week"
*)

let j2ghz = OpenDotaPlayerData.Load "https://api.opendota.com/api/players/71050296/matches" |> matchesToData
let artic = OpenDotaPlayerData.Load "https://api.opendota.com/api/players/101483335/matches" |> matchesToData
let simik = OpenDotaPlayerData.Load "https://api.opendota.com/api/players/94313396/matches" |> matchesToData
let data = [ artic; j2ghz; simik ]


(**
And in a chart:
*)
(*** define-output:chart ***)
data
|> Chart.Line
|> Chart.WithLabels [ "ArticCz"; "J2ghz"; "ssimik" ]
|> Chart.WithLegend true
(*** include-it:chart ***)
(**
Win rate compared to match length
--------------------------------
*)
(*** define-output:w ***)
OpenDotaPlayerData.Load "https://api.opendota.com/api/players/71050296/matches"
|> Array.map (fun m ->
    m.Duration,
    (if m.Deaths > 0 then (m.Kills |> double) / (m.Deaths |> double)
     else m.Kills |> double))
|> Chart.Scatter
