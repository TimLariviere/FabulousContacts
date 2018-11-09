namespace ElmishContacts

open Fabulous.Core
open System.Collections.Generic
open System.Threading

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

module Extensions =
    let debounce<'T> =
        let memoizations = Dictionary<obj, CancellationTokenSource>(HashIdentity.Structural)

        fun (timeout: int) (fn: 'T -> unit) value ->
            let key = fn.GetType()

            // Cancel previous debouncer
            match memoizations.TryGetValue(key) with
            | true, cts -> cts.Cancel()
            | _ -> ()

            // Create a new cancellation token and memoize it
            let cts = new CancellationTokenSource()
            memoizations.[key] <- cts

            // Start a new debouncer
            (async {
                try
                    // Wait timeout to see if another event will cancel this one
                    do! Async.Sleep timeout

                    // If still not cancelled, then proceed to invoke the callback and discard the unused token
                    memoizations.Remove(key) |> ignore
                    fn value
                with
                | _ -> ()
            })
            |> (fun task -> Async.StartImmediate(task, cts.Token))