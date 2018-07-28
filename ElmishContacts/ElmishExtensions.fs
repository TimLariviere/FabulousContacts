namespace ElmishContacts

open Elmish.XamarinForms
open Xamarin.Forms

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

module Extensions =
    let displayAlertWithConfirm(title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel) |> Async.AwaitTask