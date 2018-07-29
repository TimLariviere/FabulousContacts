namespace ElmishContacts

open Elmish.XamarinForms
open Xamarin.Forms

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

module Extensions =
    let displayAlert(title, message, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, cancel) |> Async.AwaitTask

    let displayAlertWithConfirm(title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel) |> Async.AwaitTask

    let displayActionSheet(title, cancel, destruction, buttons) =
        let title =
            match title with
            | None -> null
            | Some label -> label

        let cancel =
            match cancel with
            | None -> null
            | Some label -> label

        let destruction =
            match destruction with
            | None -> null
            | Some label -> label

        let buttons =
            match buttons with
            | None -> null
            | Some buttons -> buttons

        Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons) |> Async.AwaitTask