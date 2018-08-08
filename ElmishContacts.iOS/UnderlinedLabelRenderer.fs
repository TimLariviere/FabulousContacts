namespace ElmishContacts.iOS

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open System.ComponentModel
open Foundation
open UIKit

type UnderlinedLabelRenderer() =
    inherit LabelRenderer()

    override this.OnElementChanged(e: ElementChangedEventArgs<Label>) =
        base.OnElementChanged(e)

        if (e.NewElement <> null) then
            this.Control.AttributedText <- new NSAttributedString(str=this.Element.Text, underlineStyle=NSUnderlineStyle.Single)
        else
            ()

    override this.OnElementPropertyChanged(sender: obj, e: PropertyChangedEventArgs) =
        if e.PropertyName = "Text" then
            this.Control.AttributedText <- new NSAttributedString(str=this.Element.Text, underlineStyle=NSUnderlineStyle.Single)
        else
            ()

module Dummy_UnderlinedLabelRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<ElmishContacts.Controls.UnderlinedLabel>, typeof<UnderlinedLabelRenderer>)>]
    do ()