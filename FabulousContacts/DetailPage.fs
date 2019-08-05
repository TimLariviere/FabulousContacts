namespace FabulousContacts

open System
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open Xamarin.Essentials
open Models
open Helpers
open Style

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

    let notSupportedMsg = sprintf "%s is not supported on this device"

    let errorMsg = "An error has occurred"

    let ok = "OK"

    let dialNumber phoneNumber = async {
        let msg = "Can't dial number"
        try
            PhoneDialer.Open(phoneNumber)
        with
        | :? FeatureNotSupportedException ->
            do! displayAlert(msg, notSupportedMsg "Phone Dialer", ok)
        | _ ->
            do! displayAlert(msg, errorMsg, ok)

        return None
    }

    let composeSms (phoneNumber: string) = async {
        let msg = "Can't send SMS"
        try
            let message = SmsMessage("", phoneNumber)
            do! Sms.ComposeAsync(message) |> Async.AwaitTask
        with
        | :? FeatureNotSupportedException ->
            do! displayAlert(msg, notSupportedMsg "SMS", ok)
        | _ ->
            do! displayAlert(msg, errorMsg, ok)

        return None
    }

    let composeEmail emailAddress = async {
        let msg = "Can't send email"
        try
            let message = EmailMessage("", "", [| emailAddress |])
            do! Email.ComposeAsync(message) |> Async.AwaitTask
        with
        | :? FeatureNotSupportedException ->
            do! displayAlert(msg, notSupportedMsg "Email", ok)
        | _ ->
            do! displayAlert(msg, errorMsg, ok)

        return None
    }

    let init (contact: Contact) =
        { Contact = contact }, Cmd.none

    let update msg model =
        match msg with
        | EditTapped ->
            model, Cmd.none, ExternalMsg.EditContact model.Contact
        | CallTapped ->
            let dialMsg = dialNumber model.Contact.Phone
            model, Cmd.ofAsyncMsgOption dialMsg, ExternalMsg.NoOp
        | SmsTapped ->
            let smsMsg = composeSms model.Contact.Phone
            model, Cmd.ofAsyncMsgOption smsMsg, ExternalMsg.NoOp
        | EmailTapped ->
            let emailMsg = composeEmail model.Contact.Email
            model, Cmd.ofAsyncMsgOption emailMsg, ExternalMsg.NoOp
        | ContactUpdated contact ->
            { model with Contact = contact }, Cmd.none, ExternalMsg.NoOp

    let mkToolBarItems dispatch = [
        View.ToolbarItem(order = ToolbarItemOrder.Primary,
                         text = "Edit",
                         command= fun () -> dispatch EditTapped)
    ]

    let mkStackLayoutChildren model dispatch =
        let bottomChildren =
            View.StackLayout(padding = Thickness(20., 10., 20., 20.),
                             spacing = 10.,
                             children = [
                                mkDetailFieldTitle "Email"
                                mkOptionalLabel model.Contact.Email
                                mkDetailFieldTitle "Phone"
                                mkOptionalLabel model.Contact.Phone
                                mkDetailFieldTitle "Address"
                                mkOptionalLabel model.Contact.Address
                            ])
        let headerChildren =
            let headerLabel =
                View.Label(text = model.Contact.FirstName + " " + model.Contact.LastName,
                           fontSize = 20,
                           fontAttributes = FontAttributes.Bold,
                           textColor = accentTextColor,
                           horizontalOptions = LayoutOptions.Center)
            let contactPicture =
                model.Contact.Picture
                |> Option.ofObj
                |> Option.map box
                |> Option.defaultValue (box "addphoto.png")
            let gridView =
                View.Grid(
                    widthRequest = 125.,
                    heightRequest = 125.,
                    backgroundColor = Color.White,
                    horizontalOptions = LayoutOptions.Center,
                    children = [
                        View.Image(source = contactPicture,
                                   aspect = Aspect.AspectFill)
                        View.Image(source = "star.png",
                                   isVisible = model.Contact.IsFavorite,
                                   heightRequest = 35.,
                                   widthRequest = 35.,
                                   horizontalOptions = LayoutOptions.Start,
                                   verticalOptions = LayoutOptions.Start)
                    ])
            let detailView =
                View.StackLayout(horizontalOptions = LayoutOptions.Center,
                                 orientation = StackOrientation.Horizontal,
                                 margin = Thickness(0., 10., 0., 10.),
                                 spacing = 20.,
                                 children = [
                                    if hasSetField model.Contact.Phone then
                                        yield mkDetailActionButton "call.png" (fun() -> dispatch CallTapped)
                                        yield mkDetailActionButton "sms.png" (fun() -> dispatch SmsTapped)
                                    if hasSetField model.Contact.Email then
                                        yield mkDetailActionButton "email.png" (fun() -> dispatch EmailTapped)
                                ])
            View.StackLayout(backgroundColor = Color.FromHex("#448cb8"),
                             padding = Thickness(20., 10., 20., 10.),
                             spacing = 10.,
                             children = [
                                headerLabel
                                gridView
                                detailView
                             ])
        [ headerChildren; bottomChildren ]

    let view model dispatch =
        View.ContentPage(
            toolbarItems = mkToolBarItems dispatch,
            content = View.ScrollView(
                content = View.StackLayout(
                    spacing = 0.,
                    children = mkStackLayoutChildren model dispatch
                )
            )
        )    

