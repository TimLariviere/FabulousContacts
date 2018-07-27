namespace ElmishContacts.iOS

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS

type BorderedEditorRenderer() =
    inherit EditorRenderer()

    override this.OnElementChanged(e: ElementChangedEventArgs<Editor>) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            this.Control.Layer.CornerRadius <- nfloat 3.
            this.Control.Layer.BorderColor <- Color.FromHex("F0F0F0").ToCGColor()
            this.Control.Layer.BorderWidth <- nfloat 1.
        else
            ()

module Dummy_BorderedEditorRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<Xamarin.Forms.Editor>, typeof<BorderedEditorRenderer>)>]
    do ()