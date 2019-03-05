namespace ElmishContacts

open Microsoft.AppCenter
open Microsoft.AppCenter.Analytics
open Microsoft.AppCenter.Crashes
open Fabulous.Core

module AppCenter =
    type TraceParameter =
        { Key: string
          Value: string }

    type TraceData =
        { EventName: string
          AdditionalParameters: TraceParameter list }

    type UpdateTracer<'msg, 'model> = 'msg -> 'model -> TraceData option

    /// Initialize AppCenter Analytics and Crashes modules
    let start() =
        AppCenter.Start("ios=REPLACE_WITH_APPCENTER_IOS_SECRET;android=REPLACE_WITH_APPCENTER_ANDROID_SECRET", typeof<Analytics>, typeof<Crashes>)

    /// Trace all the updates to AppCenter
    let withAppCenterTrace (shouldTraceUpdate: UpdateTracer<_, _>) (program: Program<_, _, _>) =
        let traceUpdate msg model =
            match shouldTraceUpdate msg model with
            | None -> ()
            | Some data ->
                let dictionary =
                    data.AdditionalParameters
                    |> List.map (fun p -> p.Key, p.Value)
                    |> dict

                Analytics.TrackEvent (data.EventName, dictionary)

            program.update msg model

        let traceError (message, exn) =
            Crashes.TrackError(exn, dict [ ("Message", message) ])

        { program with
            update = traceUpdate 
            onError = traceError }