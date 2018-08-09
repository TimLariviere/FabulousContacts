namespace ElmishContacts

open Models
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open Style
open Xamarin.Essentials
open Helpers

module DetailPage =
    type Msg = | EditTapped
               | CallTapped
               | SmsTapped
               | EmailTapped

    type ExternalMsg = NoOp
                       | EditContact of Contact

    type Model =
        {
            Contact: Contact
        }

    let hasSetField field =
        (System.String.IsNullOrWhiteSpace(field) = false)

    let dialNumber phoneNumber = async {
        try
            PhoneDialer.Open(phoneNumber)
        with
        | :? FeatureNotSupportedException as fnse -> do! displayAlert("Can't dial number", "Phone Dialer is not supported on this device", "OK")
        | exn -> do! displayAlert("Can't dial number", "An error has occurred", "OK")

        return None
    }

    let composeSms phoneNumber = async {
        try
            let message = SmsMessage("", phoneNumber)
            do! Sms.ComposeAsync(message) |> Async.AwaitTask
        with
        | :? FeatureNotSupportedException as fnse -> do! displayAlert("Can't send SMS", "Sms is not supported on this device", "OK")
        | exn -> do! displayAlert("Can't send SMS", "An error has occurred", "OK")

        return None
    }

    let composeEmail emailAddress = async {
        try
            let message = EmailMessage("", "", [| emailAddress |])
            do! Email.ComposeAsync(message) |> Async.AwaitTask
        with
        | :? FeatureNotSupportedException as fnse -> do! displayAlert("Can't send email", "Email is not supported on this device", "OK")
        | exn -> do! displayAlert("Can't send email", "An error has occurred", "OK")

        return None
    }

    let init (contact: Contact) =
        {
            Contact = contact
        }, Cmd.none

    let update msg model =
        match msg with
        | EditTapped -> model, Cmd.none, (ExternalMsg.EditContact model.Contact)
        | CallTapped -> model, Cmd.ofAsyncMsgOption (dialNumber model.Contact.Phone), ExternalMsg.NoOp
        | SmsTapped -> model, Cmd.ofAsyncMsgOption (composeSms model.Contact.Phone), ExternalMsg.NoOp
        | EmailTapped -> model, Cmd.ofAsyncMsgOption (composeEmail model.Contact.Email), ExternalMsg.NoOp

    let view model dispatch =
        View.ContentPage(
            toolbarItems=[
                View.ToolbarItem(order=ToolbarItemOrder.Primary, text="Edit", command=(fun () -> dispatch EditTapped))
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
                                        View.ImageEx(source=(match model.Contact.Picture with null -> box "addphoto.png" | picture -> box picture), aspect=Aspect.AspectFill)
                                        View.ImageEx(source="star.png", isVisible=model.Contact.IsFavorite, heightRequest=35., widthRequest=35., horizontalOptions=LayoutOptions.Start, verticalOptions=LayoutOptions.Start)
                                    ]
                                )
                                View.StackLayout(
                                    horizontalOptions=LayoutOptions.Center,
                                    orientation=StackOrientation.Horizontal,
                                    margin=Thickness(0., 10., 0., 10.),
                                    spacing=30.,
                                    children=[
                                        if hasSetField model.Contact.Phone then
                                            yield mkDetailActionButton "call.png" (fun () -> dispatch CallTapped)
                                            yield mkDetailActionButton "sms.png" (fun () -> dispatch SmsTapped)
                                        if hasSetField model.Contact.Email then
                                            yield mkDetailActionButton "email.png" (fun () -> dispatch EmailTapped)
                                    ]
                                )
                            ]
                        )
                        View.StackLayout(
                            padding=Thickness(20., 10., 20., 20.),
                            spacing=10.,
                            children=[
                                yield mkDetailFieldTitle "Email"
                                match model.Contact.Email with
                                | "" -> yield View.Label(text="Not specified", fontAttributes=FontAttributes.Italic)
                                | _ -> yield View.Label(text=model.Contact.Email)

                                yield mkDetailFieldTitle "Phone"
                                match model.Contact.Phone with
                                | "" -> yield View.Label(text="Not specified", fontAttributes=FontAttributes.Italic)
                                | _ -> yield View.Label(text=model.Contact.Phone)

                                yield mkDetailFieldTitle "Address"
                                match model.Contact.Address with
                                | "" -> yield View.Label(text="Not specified", fontAttributes=FontAttributes.Italic)
                                | _ -> yield View.Label(text=model.Contact.Address)
                            ]
                        )
                    ]
                )
            )
        )    

