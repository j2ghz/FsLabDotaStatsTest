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
let toDateTime (timestamp:int) =
  let start = DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)
  start.AddSeconds(float timestamp).ToLocalTime()

let toWeekNumber (d:DateTime) = 
  CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetWeekOfYear(
    d,
    CalendarWeekRule.FirstFourDayWeek,
    DayOfWeek.Monday)
(**
Load the data, and transform it to have just number of games per "year/week"
*)
type OpenDotaPlayerData = JsonProvider<"https://api.opendota.com/api/players/71050296/matches">

let data = OpenDotaPlayerData.Load "https://api.opendota.com/api/players/71050296/matches"
          |> Array.map (fun m -> 
            let d = m.StartTime |> toDateTime
            d.Year,toWeekNumber d)
          |> Array.countBy id
          |> Array.where (fun ((y,_),_) -> (y = 2019))
          |> Array.map (fun ((y,w),c) -> (sprintf "%i/%i" y w , c))
          |> Array.toList

(**
The data in the table:
*)
(*** define-output:series ***)
let dataSeries =
  data
  |> series
dataSeries
(*** include-it:series ***)

(**
And in a chart:
*)
(*** define-output:chart ***)
dataSeries
|> Chart.Column
(*** include-it:chart ***)
