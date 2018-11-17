namespace ElmishContacts

open Helpers
open Models
open Repository
open Style
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open Plugin.Permissions.Abstractions
open Plugin.Media
open System

module EditPage =
    /// Declarations
    type Msg = 
        // Fields update
        | UpdateFirstName of string
        | UpdateLastName of string
        | UpdateEmail of string
        | UpdatePhone of string
        | UpdateAddress of string
        | UpdateIsFavorite of bool
        | UpdatePicture
        | SetPicture of byte array option

        // Actions
        | SaveContact
        | DeleteContact of Contact

        // Events
        | ContactAdded of Contact
        | ContactUpdated of Contact
        | ContactDeleted of Contact

    type ExternalMsg =
        | NoOp
        | GoBackAfterContactAdded of Contact
        | GoBackAfterContactUpdated of Contact
        | GoBackAfterContactDeleted of Contact

    type Model =
        { Contact: Contact option
          FirstName: string
          LastName: string
          Email: string
          Phone: string
          Address: string
          IsFavorite: bool
          Picture: byte array option
          IsFirstNameValid: bool
          IsLastNameValid: bool }

    /// Functions
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

    let doAsync action permission = async {
        let! permissionGranted = askPermissionAsync permission
        if permissionGranted then
            let! picture = action()
            return! readBytesAsync picture
        else
            return None
    }

    let updatePictureAsync previousValue = async {
        let cancel = "Cancel"
        let remove = "Remove"
        let takePicture = "Take a picture"
        let chooseFromGallery = "Choose from the gallery"

        let canTakePicture = CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported
        let canPickPicture = CrossMedia.Current.IsPickPhotoSupported

        let! action =
            displayActionSheet(None,
                               Some cancel,
                               (match previousValue with None -> None | Some _ -> Some remove), 
                               Some [| 
                                   if canTakePicture then yield takePicture
                                   if canPickPicture then yield chooseFromGallery
                               |])

        let convertToMsg bytes =
            match bytes with
            | None -> None
            | Some bytes -> Some (SetPicture (Some bytes))

        match action with
        | s when s = remove ->
            return Some (SetPicture None)

        | s when s = takePicture ->
            let! bytes = doAsync takePictureAsync Permission.Camera
            return convertToMsg bytes

        | s when s = chooseFromGallery ->
            let! bytes = doAsync pickPictureAsync Permission.Photos
            return convertToMsg bytes

        | _ -> return None
    }

    let sayContactNotValid() =
        displayAlert("Invalid contact", "Please fill all mandatory fields", "OK")

    let getSaveContactCmd dbPath model =
        if model.IsFirstNameValid = false || model.IsLastNameValid = false then
            do sayContactNotValid() |> ignore
            Cmd.none
        else
            let id = (match model.Contact with None -> 0 | Some c -> c.Id)
            let bytes = (match model.Picture with None -> null | Some arr -> arr)
            let newContact =
                { Id = id
                  FirstName = model.FirstName
                  LastName = model.LastName
                  Email = model.Email
                  Phone = model.Phone
                  Address = model.Address
                  IsFavorite = model.IsFavorite
                  Picture = bytes }
            Cmd.ofAsyncMsg (saveAsync dbPath newContact)


    /// Validations
    let validateFirstName = not << String.IsNullOrWhiteSpace
    let validateLastName = not << String.IsNullOrWhiteSpace

    /// Lifecycle
    let init (contact: Contact option) =
        let model =
            match contact with
            | Some c ->
                { Contact = Some c
                  FirstName = c.FirstName
                  LastName = c.LastName
                  Email = c.Email
                  Phone = c.Phone
                  Address = c.Address
                  IsFavorite = c.IsFavorite
                  Picture = if c.Picture <> null then Some c.Picture else None
                  IsFirstNameValid = true
                  IsLastNameValid = true }
            | None ->
                { Contact = None
                  FirstName = ""
                  LastName = ""
                  Email = ""
                  Phone = ""
                  Address = ""
                  IsFavorite = false
                  Picture = None
                  IsFirstNameValid = false
                  IsLastNameValid = false }

        model, Cmd.none

    let update dbPath msg (model: Model) =
        match msg with
        | UpdateFirstName v ->
            { model with FirstName = v; IsFirstNameValid = (validateFirstName v) }, Cmd.none, ExternalMsg.NoOp
        | UpdateLastName v ->
            { model with LastName = v; IsLastNameValid = (validateLastName v) }, Cmd.none, ExternalMsg.NoOp
        | UpdateEmail email ->
            { model with Email = email }, Cmd.none, ExternalMsg.NoOp
        | UpdatePhone phone ->
            { model with Phone = phone }, Cmd.none, ExternalMsg.NoOp
        | UpdateAddress address ->
            { model with Address = address }, Cmd.none, ExternalMsg.NoOp
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none, ExternalMsg.NoOp
        | UpdatePicture ->
            model, Cmd.ofAsyncMsgOption (updatePictureAsync model.Picture), ExternalMsg.NoOp
        | SetPicture picture ->
            { model with Picture = picture}, Cmd.none, ExternalMsg.NoOp
        | SaveContact ->
            model, (getSaveContactCmd dbPath model), ExternalMsg.NoOp
        | DeleteContact contact ->
            model, Cmd.ofAsyncMsgOption (deleteAsync dbPath contact), ExternalMsg.NoOp
        | ContactAdded contact -> 
            model, Cmd.none, (ExternalMsg.GoBackAfterContactAdded contact)
        | ContactUpdated contact -> 
            model, Cmd.none, (ExternalMsg.GoBackAfterContactUpdated contact)
        | ContactDeleted contact ->
            model, Cmd.none, (ExternalMsg.GoBackAfterContactDeleted contact)

    let view model dispatch =
        dependsOn model (fun model mModel ->

            let isDeleteButtonVisible =
                match mModel.Contact with
                | None -> false
                | Some x when x.Id = 0 -> false
                | Some _ -> true

            View.ContentPage(
                title=(
                    match mModel.Contact, mModel.FirstName, mModel.LastName with
                    | None, "", "" -> "New Contact"
                    | Some _, "", "" -> "Add a name"
                    | _, firstName, lastName -> firstName + " " + lastName
                ),
                toolbarItems=[
                    mkToolbarButton "Save" (fun() -> dispatch SaveContact)
                ],
                content=View.ScrollView(
                    automationId="ScrollView",
                    content=View.StackLayout(
                        padding=Thickness(20.),
                        children=[
                            View.Grid(
                                coldefs=[ 100.; GridLength.Star ],
                                rowdefs=[ 50.; 50. ],
                                columnSpacing=10.,
                                rowSpacing=0.,
                                children=[
                                    match mModel.Picture with
                                    | None -> 
                                        yield View.Button(
                                                image="addphoto.png",
                                                backgroundColor=Color.White,
                                                command=(fun() -> dispatch UpdatePicture)
                                              ).GridRowSpan(2)
                                    | Some picture ->
                                        yield View.Image(
                                                source=picture,
                                                aspect=Aspect.AspectFill,
                                                gestureRecognizers=[ View.TapGestureRecognizer(command=(fun() -> dispatch UpdatePicture)) ]
                                              ).GridRowSpan(2)

                                    yield (mkFormEntry "FirstName" "First name*" mModel.FirstName Keyboard.Text mModel.IsFirstNameValid (UpdateFirstName >> dispatch)).VerticalOptions(LayoutOptions.Center).GridColumn(1)
                                    yield (mkFormEntry "LastName" "Last name*" mModel.LastName Keyboard.Text mModel.IsLastNameValid (UpdateLastName >> dispatch)).VerticalOptions(LayoutOptions.Center).GridColumn(1).GridRow(1)
                                ]
                            )

                            View.StackLayout(
                                orientation=StackOrientation.Horizontal,
                                margin=new Thickness(0., 20., 0., 0.),
                                children=[
                                    View.Label(text="Mark as Favorite", verticalOptions=LayoutOptions.Center)
                                    View.Switch(automationId="MarkAsFavorite", isToggled=mModel.IsFavorite, toggled=(fun e -> e.Value |> UpdateIsFavorite |> dispatch), horizontalOptions=LayoutOptions.EndAndExpand, verticalOptions=LayoutOptions.Center)
                                ]
                            )

                            mkFormLabel "Email"
                            mkFormEntry "EmailField" "Email" mModel.Email Keyboard.Email true (UpdateEmail >> dispatch)

                            mkFormLabel "Phone"
                            mkFormEntry "PhoneField" "Phone" mModel.Phone Keyboard.Telephone true (UpdatePhone >> dispatch)

                            mkFormLabel "Address"
                            mkFormEditor "AddressField" mModel.Address (UpdateAddress >> dispatch)

                            mkDestroyButton "Delete" (fun () -> mModel.Contact.Value |> DeleteContact |> dispatch) isDeleteButtonVisible
                        ]
                    )
                )
            )
        )