namespace FabulousContacts.Controls

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms

type UnderlinedLabel() =
    inherit Label()

[<AutoOpen>]
module FabulousUnderlinedLabel =
    type Fabulous.XamarinForms.View with
        static member inline UnderlinedLabel(?text, ?gestureRecognizers, ?verticalOptions) =
            let attribs =
                ViewBuilders.BuildLabel(0,
                                        ?text = text,
                                        ?gestureRecognizers = gestureRecognizers,
                                        ?verticalOptions = verticalOptions)

            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: UnderlinedLabel) =
                ViewBuilders.UpdateLabel(prevOpt, source, target)

            ViewElement.Create(UnderlinedLabel, update, attribs)