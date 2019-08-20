namespace FabulousContacts.Droid

open Xamarin.Forms
open Xamarin.Forms.Platform.Android

type BorderedEntryRenderer(context) =
    inherit EntryRenderer(context)

    member this.BorderedEntry
        with get() =
            this.Element :?> FabulousContacts.Controls.BorderedEntry

    override this.OnElementChanged(e) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            if this.BorderedEntry.BorderColor = Xamarin.Forms.Color.FromHex("#FF0000") then
                this.Control.Error <- "Field required"
            else
                this.Control.Error <- null

    override this.OnElementPropertyChanged(_, e) =
        if e.PropertyName = "BorderColor" then
            if this.BorderedEntry.BorderColor = Xamarin.Forms.Color.FromHex("#FF0000") then
                this.Control.Error <- "Field required"
            else
                this.Control.Error <- null

module Dummy_BorderedEntryRenderer =
    [<assembly: ExportRenderer(typeof<FabulousContacts.Controls.BorderedEntry>, typeof<BorderedEntryRenderer>)>]
    do ()