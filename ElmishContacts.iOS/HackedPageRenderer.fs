namespace ElmishContacts.iOS

open System.Reflection
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS

// F# version of https://kent-boogaart.com/blog/hacking-xamarin.forms-page.appearing-for-ios
type HackedPageRenderer() =
    inherit PageRenderer()

    let appearedField = (typeof<PageRenderer>).GetField("_appeared", BindingFlags.NonPublic ||| BindingFlags.Instance)
    let disposedField = (typeof<PageRenderer>).GetField("_disposed", BindingFlags.NonPublic ||| BindingFlags.Instance)

    member this.PageController = this.Element :> obj :?> IPageController
    member this.Appeared with get() = appearedField.GetValue(this) :?> bool and set (value: bool) = appearedField.SetValue(this, value)
    member this.Disposed with get() = disposedField.GetValue(this) :?> bool and set (value: bool) = disposedField.SetValue(this, value)

    override this.ViewWillAppear(animated: bool) =
        base.ViewWillAppear(animated)

        if (this.Appeared = false && this.Disposed = false) then
            this.Appeared <- true
            this.PageController.SendAppearing()
        else
            ()

module Dummy_HackedPageRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<Xamarin.Forms.Page>, typeof<HackedPageRenderer>)>]
    do ()