namespace FabulousContacts.Controls

open Fabulous.DynamicViews
open Xamarin.Forms

type UnderlinedLabel() =
    inherit Label()

[<AutoOpen>]
module DynamicViewsUnderlinedLabel =
    type Fabulous.DynamicViews.View with
        static member UnderlinedLabel(?text, ?gestureRecognizers, ?verticalOptions) =
            let attribs =
                ViewBuilders.BuildLabel(0, ?text=text, ?gestureRecognizers=gestureRecognizers, ?verticalOptions=verticalOptions)

            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: UnderlinedLabel) =
                ViewBuilders.UpdateLabel(prevOpt, source, target)

            ViewElement.Create(UnderlinedLabel, update, attribs)