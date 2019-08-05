namespace FabulousContacts

open System
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open Plugin.Permissions.Abstractions
open Plugin.Media
open Models
open Helpers
open Repository
open Style

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
            let deleteMsg = sprintf "Delete %s %s" contact.FirstName contact.LastName
            let confirmationMsg = "This action is definitive. Are you sure?"
            displayAlertWithConfirm(deleteMsg, confirmationMsg, "Yes", "No")

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
                               previousValue |> Option.map (fun _ -> remove),
                               Some [|
                                   if canTakePicture then yield takePicture
                                   if canPickPicture then yield chooseFromGallery
                               |])

        let convertToMsg = SetPicture >> Some

        match action with
        | s when s = remove ->
            return convertToMsg None
        | s when s = takePicture ->
            let! bytes = doAsync takePictureAsync Permission.Camera
            return convertToMsg bytes
        | s when s = chooseFromGallery ->
            let! bytes = doAsync pickPictureAsync Permission.Photos
            return convertToMsg bytes
        | _ ->
            return None
    }

    let sayContactNotValid() =
        displayAlert("Invalid contact", "Please fill all mandatory fields", "OK")

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

    let saveCmd model dbPath =
        if not model.IsFirstNameValid || not model.IsLastNameValid then
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
            let msg = saveAsync dbPath newContact
            Cmd.ofAsyncMsg msg

    let update dbPath msg (model: Model) =
        match msg with
        | UpdateFirstName v ->
            let m = { model with FirstName = v; IsFirstNameValid = (validateFirstName v) }
            m, Cmd.none, ExternalMsg.NoOp
        | UpdateLastName v ->
            let m = { model with LastName = v; IsLastNameValid = (validateLastName v) }
            m, Cmd.none, ExternalMsg.NoOp
        | UpdateEmail email ->
            { model with Email = email }, Cmd.none, ExternalMsg.NoOp
        | UpdatePhone phone ->
            { model with Phone = phone }, Cmd.none, ExternalMsg.NoOp
        | UpdateAddress address ->
            { model with Address = address }, Cmd.none, ExternalMsg.NoOp
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none, ExternalMsg.NoOp
        | UpdatePicture ->
            let msg = updatePictureAsync model.Picture
            model, Cmd.ofAsyncMsgOption msg, ExternalMsg.NoOp
        | SetPicture picture ->
            { model with Picture = picture}, Cmd.none, ExternalMsg.NoOp
        | SaveContact ->
            let cmd = saveCmd model dbPath
            model, cmd, ExternalMsg.NoOp
        | DeleteContact contact ->
            let msg = deleteAsync dbPath contact
            model, Cmd.ofAsyncMsgOption msg, ExternalMsg.NoOp
        | ContactAdded contact -> 
            model, Cmd.none, ExternalMsg.GoBackAfterContactAdded contact
        | ContactUpdated contact -> 
            model, Cmd.none, ExternalMsg.GoBackAfterContactUpdated contact
        | ContactDeleted contact ->
            model, Cmd.none, ExternalMsg.GoBackAfterContactDeleted contact

    let mkTitle contact (fullName: string) =
        match contact, (fullName.Trim()) with
        | None, "" -> "New Contact"
        | _, "" -> "Add a name"
        | _, _ -> fullName

    let mkToolBarItems dispatch = [
        mkToolbarButton "Save" (fun() -> dispatch SaveContact)
    ]

    let mkStackLayoutChildren mModel dispatch =
        let isDeleteButtonVisible =
                match mModel.Contact with
                | None -> false
                | Some x when x.Id = 0 -> false
                | Some _ -> true
        let mkButtonOrPicture =
            match mModel.Picture with
            | None ->
                View.Button(image = "addphoto.png",
                            backgroundColor = Color.White,
                            command = fun () -> dispatch UpdatePicture)
                            .GridRowSpan(2)
            | Some picture ->
                View.Image(source = picture,
                           aspect = Aspect.AspectFill,
                           gestureRecognizers = [
                               View.TapGestureRecognizer(
                                   command = fun() -> dispatch UpdatePicture)
                           ])
                           .GridRowSpan(2)
        let firstNameTxtView =
            let txtView =
                mkFormEntry "First name*" mModel.FirstName Keyboard.Text mModel.IsFirstNameValid (UpdateFirstName >> dispatch)
            txtView.VerticalOptions(LayoutOptions.Center).GridColumn(1)
        let lastNameTxtView =
            let textView =
                mkFormEntry "Last name*" mModel.LastName Keyboard.Text mModel.IsLastNameValid (UpdateLastName >> dispatch)
            textView.VerticalOptions(LayoutOptions.Center).GridColumn(1).GridRow(1)
        let gridView =
            View.Grid(coldefs = [ 100.; GridLength.Star ],
                      rowdefs = [ 50.; 50. ],
                      columnSpacing = 10.,
                      rowSpacing = 0.,
                      children = [
                          mkButtonOrPicture
                          firstNameTxtView
                          lastNameTxtView
                      ])
        let favoriteView =
            View.StackLayout(orientation = StackOrientation.Horizontal,
                             margin = Thickness(0., 20., 0., 0.),
                             children = [
                                 View.Label(text = "Mark as Favorite",
                                            verticalOptions = LayoutOptions.Center)
                                 View.Switch(isToggled = mModel.IsFavorite,
                                             toggled = (fun e -> e.Value |> UpdateIsFavorite |> dispatch),
                                             horizontalOptions = LayoutOptions.EndAndExpand,
                                             verticalOptions = LayoutOptions.Center)
                             ])
        [ gridView
          favoriteView
          mkFormLabel "Email"
          mkFormEntry "Email" mModel.Email Keyboard.Email true (UpdateEmail >> dispatch)
          mkFormLabel "Phone"
          mkFormEntry "Phone" mModel.Phone Keyboard.Telephone true (UpdatePhone >> dispatch)
          mkFormLabel "Address"
          mkFormEditor mModel.Address (UpdateAddress >> dispatch)
          mkDestroyButton "Delete" (fun () -> mModel.Contact.Value |> DeleteContact |> dispatch) isDeleteButtonVisible ]

    let view model dispatch =
        dependsOn model (fun _ mModel ->
            View.ContentPage(
                title = mkTitle mModel.Contact (sprintf "%s %s" mModel.FirstName mModel.LastName),
                toolbarItems = mkToolBarItems dispatch,
                content = View.ScrollView(
                    content = View.StackLayout(
                        padding = Thickness(20.),
                        children = mkStackLayoutChildren mModel dispatch
                    )
                )
            )
        )