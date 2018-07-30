namespace ElmishContacts

open Helpers
open Models
open Repository
open Style
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module ItemPage =
    type Msg = | UpdatePicture
               | UpdateFirstName of string
               | UpdateLastName of string
               | UpdateAddress of string
               | UpdateIsFavorite of bool
               | SetPicture of string
               | SaveContact of Contact option * picture: string option * firstName: string * lastName: string * address: string * isFavorite: bool
               | DeleteContact of Contact
               | ContactAdded of Contact
               | ContactUpdated of Contact
               | ContactDeleted of Contact

    type ExternalMsg = | NoOp
                       | GoBackAfterContactAdded of Contact
                       | GoBackAfterContactUpdated of Contact
                       | GoBackAfterContactDeleted of Contact

    type Model =
        {
            Contact: Contact option
            Picture: string option
            FirstName: string
            LastName: string
            Address: string
            IsFavorite: bool
        }

    let saveAsync dbPath contact = async {
        match contact.Id with
        | 0 ->
            let! insertedContact = insertContact dbPath contact
            return ContactAdded insertedContact
        | _ ->
            let! updatedContact = updateContact dbPath contact
            return ContactUpdated updatedContact
    }

    let deleteAsync dbPath (contact: Contact) = async {
        let! shouldDelete = 
            displayAlertWithConfirm("Delete " + contact.FirstName + " " + contact.LastName, "This action is definitive. Are you sure?", "Yes", "No")

        if shouldDelete then
            do! deleteContact dbPath contact
            return Some (ContactDeleted contact)
        else
            return None
    }

    let addPhotoAsync () = async {
        let! photo = pickOrTakePictureAsync()

        match photo with
        | null ->
            return None
        | file ->
            let! base64 = readFileAsBase64 file
            return Some (SetPicture base64)
    }

    let init contact =
        let model =
            match contact with
            | Some c ->
                {
                    Contact = Some c
                    Picture = if c.Picture <> "" then Some c.Picture else None
                    FirstName = c.FirstName
                    LastName = c.LastName
                    Address = c.Address
                    IsFavorite = c.IsFavorite
                }
            | None ->
                {
                    Contact = None
                    Picture = None
                    FirstName = ""
                    LastName = ""
                    Address = ""
                    IsFavorite = false
                }

        model, Cmd.none

    let update dbPath msg (model: Model) =
        match msg with
        | UpdatePicture ->
            model, Cmd.ofAsyncMsgOption (addPhotoAsync ()), ExternalMsg.NoOp
        | UpdateFirstName name ->
            { model with FirstName = name }, Cmd.none, ExternalMsg.NoOp
        | UpdateLastName name ->
            { model with LastName = name }, Cmd.none, ExternalMsg.NoOp
        | UpdateAddress address ->
            { model with Address = address }, Cmd.none, ExternalMsg.NoOp
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none, ExternalMsg.NoOp
        | SetPicture base64 ->
            { model with Picture = Some base64}, Cmd.none, ExternalMsg.NoOp
        | SaveContact (contact, picture, firstName, lastName, address, isFavorite) ->
            let newContact =
                match contact with
                | None -> { Id = 0; Picture = ""; FirstName = firstName; LastName = lastName; Address = address; IsFavorite = isFavorite }
                | Some c -> { c with Picture = getBase64 picture; FirstName = firstName; LastName = lastName; Address = address; IsFavorite = isFavorite }
            model, Cmd.ofAsyncMsg (saveAsync dbPath newContact), ExternalMsg.NoOp
        | DeleteContact contact ->
            model, Cmd.ofAsyncMsgOption (deleteAsync dbPath contact), ExternalMsg.NoOp
        | ContactAdded contact -> 
            model, Cmd.none, (ExternalMsg.GoBackAfterContactAdded contact)
        | ContactUpdated contact -> 
            model, Cmd.none, (ExternalMsg.GoBackAfterContactUpdated contact)
        | ContactDeleted contact ->
            model, Cmd.none, (ExternalMsg.GoBackAfterContactDeleted contact)

    let view model dispatch =
        let imageSource =
            dependsOn model.Picture (fun _ picture ->
                match picture with
                | None -> null
                | Some base64 -> getImageSourceFromBase64 base64
            )

        dependsOn (model.Contact, model.Picture, model.FirstName, model.LastName, model.Address, model.IsFavorite) (fun model (mContact, mPicture, mFirstName, mLastName, mAddress, mIsFavorite) ->
            let isDeleteButtonVisible =
                match mContact with
                | None -> false
                | Some x when x.Id = 0 -> false
                | Some _ -> true

            View.ContentPage(
                title=(if (mFirstName = "" && mLastName = "") then "New Contact" else mFirstName + " " + mLastName),
                toolbarItems=[
                    mkToolbarButton "Save" (fun() -> (mContact, mPicture, mFirstName, mLastName, mAddress, mIsFavorite) |> SaveContact |> dispatch)
                ],
                content=View.StackLayout(
                    children=[
                        View.Grid(
                            margin=Thickness(20., 20., 20., 0.),
                            coldefs=[ 100.; GridLength.Star ],
                            rowdefs=[ 50.; 50. ],
                            columnSpacing=10.,
                            rowSpacing=0.,
                            children=[
                                if mPicture.IsNone then
                                    yield View.Button(image="addphoto.png", backgroundColor=Color.White, command=(fun () -> dispatch UpdatePicture)).GridRowSpan(2)
                                else
                                    yield View.Image(
                                            source=imageSource,
                                            aspect=Aspect.AspectFill,
                                            gestureRecognizers=[ View.TapGestureRecognizer(command=(fun () -> dispatch UpdatePicture)) ]
                                          ).GridRowSpan(2)

                                yield View.Entry(placeholder="First name", text=mFirstName, textChanged=(fun e -> e.NewTextValue |> UpdateFirstName |> dispatch), verticalOptions=LayoutOptions.Center).GridColumn(1)
                                yield View.Entry(placeholder="Last name", text=mLastName, textChanged=(fun e -> e.NewTextValue |> UpdateLastName |> dispatch), verticalOptions=LayoutOptions.Center).GridColumn(1).GridRow(1)
                            ]
                        )

                        mkFormLabel "Address"
                        mkFormEditor mAddress (fun e -> e.NewTextValue |> UpdateAddress |> dispatch)
                        mkFormLabel "Is Favorite"
                        mkFormSwitch mIsFavorite (fun e -> e.Value |> UpdateIsFavorite |> dispatch)
                        mkDestroyButton "Delete" (fun () -> mContact.Value |> DeleteContact |> dispatch) isDeleteButtonVisible
                    ]
                )
            )
        )