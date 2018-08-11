namespace ElmishContacts.Droid

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.Android
open System.ComponentModel

type UnderlinedLabelRenderer(context) =
    inherit LabelRenderer(context)

    override this.OnElementChanged(e: ElementChangedEventArgs<Label>) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            this.Control.PaintFlags <- this.Control.PaintFlags + Android.Graphics.PaintFlags.UnderlineText
        else
            ()

module Dummy_UnderlinedLabelRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<ElmishContacts.Controls.UnderlinedLabel>, typeof<UnderlinedLabelRenderer>)>]
    do ()