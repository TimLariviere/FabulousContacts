namespace ElmishContacts

open Elmish.XamarinForms
open Xamarin.Forms

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

module Extensions =
    let displayAlertWithConfirm(title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel) |> Async.AwaitTask

    let displayActionSheet(title, cancel, destruction, buttons) =
        let cancelButton =
            match cancel with
            | None -> null
            | Some label -> label

        let destructionButton =
            match destruction with
            | None -> null
            | Some label -> label

        Application.Current.MainPage.DisplayActionSheet(title, cancelButton, destructionButton, buttons) |> Async.AwaitTask