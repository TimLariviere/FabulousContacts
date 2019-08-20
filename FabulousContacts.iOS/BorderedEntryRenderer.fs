namespace FabulousContacts.iOS

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open System.ComponentModel

type BorderedEntryRenderer() =
    inherit EntryRenderer()

    member this.BorderedEntry
        with get() =
            this.Element :?> FabulousContacts.Controls.BorderedEntry

    override this.OnElementChanged(e: ElementChangedEventArgs<Entry>) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            this.Control.Layer.BorderColor <- this.BorderedEntry.BorderColor.ToCGColor()
            this.Control.Layer.BorderWidth <- nfloat 1.
            this.Control.Layer.CornerRadius <- nfloat 5.
        else
            ()

    override this.OnElementPropertyChanged(sender: obj, e: PropertyChangedEventArgs) =
        if e.PropertyName = "BorderColor" then
            this.Control.Layer.BorderColor <- this.BorderedEntry.BorderColor.ToCGColor()
            this.Control.Layer.BorderWidth <- nfloat 1.
            this.Control.Layer.CornerRadius <- nfloat 5.
        else
            ()

module Dummy_BorderedEntryRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<FabulousContacts.Controls.BorderedEntry>, typeof<BorderedEntryRenderer>)>]
    do ()