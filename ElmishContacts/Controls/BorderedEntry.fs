namespace ElmishContacts.Controls

open Fabulous.DynamicViews
open Xamarin.Forms

type BorderedEntry() =
    inherit Entry()

    static let BorderColorProperty = BindableProperty.Create("BorderColor", typeof<Color>, typeof<BorderedEntry>, Color.Default)

    member this.BorderColor
        with get () = this.GetValue(BorderColorProperty) :?> Color
        and set (value) = this.SetValue(BorderColorProperty, value)

[<AutoOpen>]
module DynamicViewsBorderedEntry =
    let BorderedEntryBorderColorAttributeKey = AttributeKey<_> "BorderedEntry_BorderColor"    

    type Fabulous.DynamicViews.View with
        static member BorderedEntry(?borderColor: Color,
                                    ?placeholder, ?text, ?textChanged, ?keyboard) =
            let attribCount = match borderColor with None -> 0 | Some _ -> 1
            let attribs =
                View.BuildEntry(attribCount, ?placeholder=placeholder, ?text=text, ?textChanged=textChanged, ?keyboard=keyboard)

            match borderColor with None -> () | Some v -> attribs.Add(BorderedEntryBorderColorAttributeKey, v)

            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: BorderedEntry) =
                View.UpdateEntry(prevOpt, source, target)
                source.UpdatePrimitive(prevOpt, target, BorderedEntryBorderColorAttributeKey, (fun target v -> target.BorderColor <- v))

            ViewElement.Create(BorderedEntry, update, attribs)