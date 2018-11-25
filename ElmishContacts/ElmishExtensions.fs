namespace ElmishContacts

open System
open System.Collections.Generic
open System.Threading
open Xamarin.Forms

module Extensions =
    /// Debounce multiple calls to a single function
    let debounce<'T> (timeout: int) =
        let memoizations = Dictionary<obj, CancellationTokenSource>(HashIdentity.Structural)
        fun (fn: 'T -> unit) value ->
            let key = fn.GetType()
            match memoizations.TryGetValue(key) with
            | true, previousCts -> previousCts.Cancel()
            | _ -> ()

            let cts = new CancellationTokenSource()
            memoizations.[key] <- cts

            Device.StartTimer(TimeSpan.FromMilliseconds(float timeout), (fun () ->
                match cts.IsCancellationRequested with
                | false ->
                    memoizations.Remove(key) |> ignore
                    fn value
                | _ -> ()
                false // Do not let the timer trigger a second time
            ))

    let debounce250<'T> = debounce<'T> 250