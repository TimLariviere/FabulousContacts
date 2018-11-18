namespace ElmishContacts

open Microsoft.AppCenter
open Microsoft.AppCenter.Analytics
open Microsoft.AppCenter.Crashes
open Fabulous.Core

module AppCenter =
    type UpdateTracer<'msg, 'model> = 'msg -> 'model -> (string * (string * string) list) option

    /// Initialize AppCenter Analytics and Crashes modules
    let start() =
        AppCenter.Start("ios=REPLACE_WITH_APPCENTER_IOS_SECRET;android=REPLACE_WITH_APPCENTER_ANDROID_SECRET", typeof<Analytics>, typeof<Crashes>)

    /// Trace all the updates to AppCenter
    let withAppCenterTrace (shouldTraceUpdate: UpdateTracer<_, _>) (program: Program<_, _, _>) =
        let traceUpdate msg model =
            match shouldTraceUpdate msg model with
            | Some (key, value) -> Analytics.TrackEvent (key, dict value)
            | None -> ()
            program.update msg model

        let traceError (message, exn) =
            Crashes.TrackError(exn, dict [ ("Message", message) ])

        { program with
            update = traceUpdate 
            onError = traceError }