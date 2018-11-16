namespace ElmishContacts

open Models
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open Style
open Xamarin.Essentials
open Helpers
open System

module DetailPage =
    type Msg =
        | EditTapped
        | CallTapped
        | SmsTapped
        | EmailTapped
        | ContactUpdated of Contact

    type ExternalMsg =
        | NoOp
        | EditContact of Contact

    type Model =
        { Contact: Contact }

    let hasSetField = not << String.IsNullOrWhiteSpace

    let dialNumber phoneNumber = async {
        try
            PhoneDialer.Open(phoneNumber)
        with
        | :? FeatureNotSupportedException -> do! displayAlert("Can't dial number", "Phone Dialer is not supported on this device", "OK")
        | _ -> do! displayAlert("Can't dial number", "An error has occurred", "OK")

        return None
    }

    let composeSms phoneNumber = async {
        try
            let message = SmsMessage("", phoneNumber)
            do! Sms.ComposeAsync(message) |> Async.AwaitTask
        with
        | :? FeatureNotSupportedException -> do! displayAlert("Can't send SMS", "Sms is not supported on this device", "OK")
        | _ -> do! displayAlert("Can't send SMS", "An error has occurred", "OK")

        return None
    }

    let composeEmail emailAddress = async {
        try
            let message = EmailMessage("", "", [| emailAddress |])
            do! Email.ComposeAsync(message) |> Async.AwaitTask
        with
        | :? FeatureNotSupportedException -> do! displayAlert("Can't send email", "Email is not supported on this device", "OK")
        | _ -> do! displayAlert("Can't send email", "An error has occurred", "OK")

        return None
    }

    let init (contact: Contact) =
        { Contact = contact }, Cmd.none

    let update msg model =
        match msg with
        | EditTapped ->
            model, Cmd.none, (ExternalMsg.EditContact model.Contact)
        | CallTapped ->
            model, Cmd.ofAsyncMsgOption (dialNumber model.Contact.Phone), ExternalMsg.NoOp
        | SmsTapped ->
            model, Cmd.ofAsyncMsgOption (composeSms model.Contact.Phone), ExternalMsg.NoOp
        | EmailTapped ->
            model, Cmd.ofAsyncMsgOption (composeEmail model.Contact.Email), ExternalMsg.NoOp
        | ContactUpdated contact ->
            { model with Contact = contact }, Cmd.none, ExternalMsg.NoOp

    let view model dispatch =
        View.ContentPage(
            toolbarItems=[
                View.ToolbarItem(order=ToolbarItemOrder.Primary, text="Edit", command=(fun() -> dispatch EditTapped))
            ],
            content=View.ScrollView(
                content=View.StackLayout(
                    spacing=0.,
                    children=[
                        View.StackLayout(
                            backgroundColor=Color.FromHex("#448cb8"),
                            padding=Thickness(20., 10., 20., 10.),
                            spacing=10.,
                            children=[
                                View.Label(text=model.Contact.FirstName + " " + model.Contact.LastName, fontSize=20, fontAttributes=FontAttributes.Bold, textColor=accentTextColor, horizontalOptions=LayoutOptions.Center)
                                View.Grid(
                                    widthRequest=125.,
                                    heightRequest=125.,
                                    backgroundColor=Color.White,
                                    horizontalOptions=LayoutOptions.Center,
                                    children=[
                                        View.Image(automationId="Picture", source=(match model.Contact.Picture with null -> box "addphoto.png" | picture -> box picture), aspect=Aspect.AspectFill)
                                        View.Image(source="star.png", isVisible=model.Contact.IsFavorite, heightRequest=35., widthRequest=35., horizontalOptions=LayoutOptions.Start, verticalOptions=LayoutOptions.Start)
                                    ]
                                )
                                View.StackLayout(
                                    horizontalOptions=LayoutOptions.Center,
                                    orientation=StackOrientation.Horizontal,
                                    margin=Thickness(0., 10., 0., 10.),
                                    spacing=20.,
                                    children=[
                                        if hasSetField model.Contact.Phone then
                                            yield mkDetailActionButton "call.png" (fun() -> dispatch CallTapped)
                                            yield mkDetailActionButton "sms.png" (fun() -> dispatch SmsTapped)
                                        if hasSetField model.Contact.Email then
                                            yield mkDetailActionButton "email.png" (fun() -> dispatch EmailTapped)
                                    ]
                                )
                            ]
                        )
                        View.StackLayout(
                            padding=Thickness(20., 10., 20., 20.),
                            spacing=10.,
                            children=[
                                mkDetailFieldTitle "Email"
                                mkOptionalLabel model.Contact.Email
                                mkDetailFieldTitle "Phone"
                                mkOptionalLabel model.Contact.Phone
                                mkDetailFieldTitle "Address"
                                mkOptionalLabel model.Contact.Address
                            ]
                        )
                    ]
                )
            )
        )    

