namespace ElmishContacts.Controls

open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

type UnderlinedLabel() =
    inherit Label()

[<AutoOpen>]
module DynamicViewsUnderlinedLabel =
    type Elmish.XamarinForms.DynamicViews.View with
        static member UnderlinedLabel(?text, ?gestureRecognizers, ?verticalOptions) =
            let attribs =
                View.BuildLabel(0, ?text=text, ?gestureRecognizers=gestureRecognizers, ?verticalOptions=verticalOptions)

            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: UnderlinedLabel) =
                View.UpdateLabel(prevOpt, source, target)

            ViewElement.Create(UnderlinedLabel, update, attribs)