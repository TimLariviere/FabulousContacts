namespace ElmishContacts

open Models
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

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

    let init (contact: Contact) =
        {
            Contact = contact
        }, Cmd.none

    let update msg model =
        match msg with
        | EditTapped -> model, Cmd.none, (ExternalMsg.EditContact model.Contact)
        | CallTapped -> model, Cmd.none, ExternalMsg.NoOp
        | SmsTapped -> model, Cmd.none, ExternalMsg.NoOp
        | EmailTapped -> model, Cmd.none, ExternalMsg.NoOp

    let view model dispatch =
        View.ContentPage(
            toolbarItems=[
                View.ToolbarItem(order=ToolbarItemOrder.Primary, text="Edit", command=(fun () -> dispatch EditTapped))
            ],
            content=View.ScrollView(
                content=View.StackLayout(
                    children=[
                        View.ImageEx(source=match model.Contact.Picture with null -> box "addphoto.png" | picture -> box picture)
                        View.StackLayout(
                            orientation=StackOrientation.Horizontal,
                            children=[
                                View.ImageEx(source="star.png", isVisible=model.Contact.IsFavorite, heightRequest=25., widthRequest=25.)
                                View.Label(text= model.Contact.FirstName + " " + model.Contact.LastName)
                            ]
                        )
                        View.StackLayout(
                            orientation=StackOrientation.Horizontal,
                            children=[
                                if hasSetField model.Contact.Phone then
                                    yield View.Button(text="call", command=(fun () -> dispatch CallTapped))
                                    yield View.Button(text="sms", command=(fun () -> dispatch SmsTapped))
                                if hasSetField model.Contact.Email then
                                    yield View.Button(text="email", command=(fun () -> dispatch EmailTapped))
                            ]
                        )
                        View.Label(text="Address")
                        View.Label(text=model.Contact.Address)
                    ]
                )
            )
        )    

