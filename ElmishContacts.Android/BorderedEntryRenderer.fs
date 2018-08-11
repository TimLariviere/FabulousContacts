namespace ElmishContacts.Droid

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.Android
open System.ComponentModel

type BorderedEntryRenderer(context) =
    inherit EntryRenderer(context)

    member this.BorderedEntry with get() = this.Element :?> ElmishContacts.Controls.BorderedEntry

    override this.OnElementChanged(e: ElementChangedEventArgs<Entry>) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            if this.BorderedEntry.BorderColor = Xamarin.Forms.Color.FromHex("#FF0000") then
                this.Control.Error <- "Field required"
            else
                this.Control.Error <- null
        else
            ()

    override this.OnElementPropertyChanged(sender: obj, e: PropertyChangedEventArgs) =
        if e.PropertyName = "BorderColor" then
            if this.BorderedEntry.BorderColor = Xamarin.Forms.Color.FromHex("#FF0000") then
                this.Control.Error <- "Field required"
            else
                this.Control.Error <- null
        else
            ()

module Dummy_BorderedEntryRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<ElmishContacts.Controls.BorderedEntry>, typeof<BorderedEntryRenderer>)>]
    do ()