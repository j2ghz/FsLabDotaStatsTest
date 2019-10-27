open System.Globalization
(*** hide ***)
#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

(**

Welcome to FsLab journal
========================

Sample experiment
-----------------

We start by referencing `Deedle` and `XPlot.GoogleCharts` libraries and then we
load the contents of *this* file:
*)

open System
open Deedle
open System.IO
open XPlot.GoogleCharts
open FSharp.Data

let toDateTime (timestamp:int) =
  let start = DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)
  start.AddSeconds(float timestamp).ToLocalTime()

type OpenDotaPlayerData = JsonProvider<"https://api.opendota.com/api/players/71050296/matches">

let data = OpenDotaPlayerData.Load "https://api.opendota.com/api/players/71050296/matches"
          |> Array.map (fun m -> m.StartTime |> toDateTime) 
          |> Array.map (fun d -> d.Year,CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetWeekOfYear(d,CalendarWeekRule.FirstFourDayWeek,DayOfWeek.Monday))
          |> Array.countBy id
          |> Array.where (fun ((y,_),_) -> (y = 2019))
          |> Array.map (fun ((y,w),c) -> (sprintf "%i/%i" y w , c))
          |> Array.toList

(**
Now, we split the contents of the file into words, count the frequency of
words longer than 3 letters and turn the result into a Deedle series:
*)
(*** define-output:series ***)
let dataSeries =
  data
  |> series
dataSeries
(*** include-it:series ***)

(**
Finally, we can take the same 6 words and call `Chart.Column` to see them in a chart:
*)
(*** define-output:chart ***)
dataSeries
|> Chart.Column
(*** include-it:chart ***)
