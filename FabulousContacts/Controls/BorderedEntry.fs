namespace FabulousContacts.Controls

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms

type BorderedEntry() =
    inherit Entry()

    static let borderColorProperty =
        BindableProperty.Create("BorderColor", typeof<Color>, typeof<BorderedEntry>, Color.Default)

    member this.BorderColor
        with get () =
            this.GetValue(borderColorProperty) :?> Color
        and set (value) =
            this.SetValue(borderColorProperty, value)

[<AutoOpen>]
module FabulousBorderedEntry =
    let BorderedEntryBorderColorAttributeKey =
        AttributeKey<_> "BorderedEntry_BorderColor"

    type Fabulous.XamarinForms.View with
        static member inline BorderedEntry(?borderColor: Color, ?placeholder, ?text, ?textChanged, ?keyboard) =
            let attribCount = match borderColor with None -> 0 | Some _ -> 1
            let attribs =
                ViewBuilders.BuildEntry(attribCount,
                                        ?placeholder = placeholder,
                                        ?text = text,
                                        ?textChanged = textChanged,
                                        ?keyboard = keyboard)

            match borderColor with None -> () | Some v -> attribs.Add(BorderedEntryBorderColorAttributeKey, v)

            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: BorderedEntry) =
                ViewBuilders.UpdateEntry(prevOpt, source, target)
                source.UpdatePrimitive(prevOpt, target, BorderedEntryBorderColorAttributeKey, (fun target v -> target.BorderColor <- v))

            ViewElement.Create(BorderedEntry, update, attribs)