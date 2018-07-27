// Extensions that would be great to include in Elmish.XamarinForms directly
namespace ElmishContacts

open Elmish.XamarinForms
open Xamarin.Forms

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

module View =
    let displayAlert title message cancel =
        Application.Current.MainPage.DisplayAlert(title, message, cancel) |> Async.AwaitTask

    let displayAlertWithConfirm title message accept cancel =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel) |> Async.AwaitTask

    let displayActionSheet title cancel destruction buttons =
        Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons) |> Async.AwaitTask