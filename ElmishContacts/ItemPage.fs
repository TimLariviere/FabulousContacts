namespace ElmishContacts

open Extensions
open Models
open Repository
open Style
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open Plugin.Media
open Plugin.Media.Abstractions

module ItemPage =
    type Msg = | UpdateFirstName of string
               | UpdateLastName of string
               | UpdateAddress of string
               | UpdateIsFavorite of bool
               | AddPhoto
               | SaveContact of Contact option * firstName: string * lastName: string * address: string * isFavorite: bool
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
        let canPickPicture = CrossMedia.Current.IsPickPhotoSupported
        let canTakePicture = CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported

        let pickPicture = "Choose from gallery"
        let takePicture = "Take picture"
        let cancel = "Cancel"

        let choices = [|
            if canPickPicture then yield pickPicture
            if canTakePicture then yield takePicture
        |]

        let! source =
            displayActionSheet(None, Some cancel, None, Some choices)

        let! photo =
            match source with
            | choice when choice = pickPicture -> CrossMedia.Current.PickPhotoAsync() |> Async.AwaitTask
            | choice when choice = takePicture -> CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()) |> Async.AwaitTask
            | _ -> System.Threading.Tasks.Task.FromResult<Abstractions.MediaFile>(null) |> Async.AwaitTask

        do!
            match photo with
            | null -> displayAlert("No photo", "No photo selected", "OK")
            | file -> displayAlert("Photo", "Photo selected: " + file.AlbumPath, "OK")

        return None
    }

    let init contact =
        let model =
            match contact with
            | Some c ->
                {
                    Contact = Some c
                    FirstName = c.FirstName
                    LastName = c.LastName
                    Address = c.Address
                    IsFavorite = c.IsFavorite
                }
            | None ->
                {
                    Contact = None
                    FirstName = ""
                    LastName = ""
                    Address = ""
                    IsFavorite = false
                }

        model, Cmd.none

    let update dbPath msg (model: Model) =
        match msg with
        | UpdateFirstName name ->
            { model with FirstName = name }, Cmd.none, ExternalMsg.NoOp
        | UpdateLastName name ->
            { model with LastName = name }, Cmd.none, ExternalMsg.NoOp
        | UpdateAddress address ->
            { model with Address = address }, Cmd.none, ExternalMsg.NoOp
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none, ExternalMsg.NoOp
        | AddPhoto ->
            model, Cmd.ofAsyncMsgOption (addPhotoAsync ()), ExternalMsg.NoOp
        | SaveContact (contact, firstName, lastName, address, isFavorite) ->
            let newContact =
                match contact with
                | None -> { Id = 0; FirstName = firstName; LastName = lastName; Address = address; IsFavorite = isFavorite }
                | Some c -> { c with FirstName = firstName; LastName = lastName; Address = address; IsFavorite = isFavorite }
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
        dependsOn (model.Contact, model.FirstName, model.LastName, model.Address, model.IsFavorite) (fun model (mContact, mFirstName, mLastName, mAddress, mIsFavorite) ->
            let isDeleteButtonVisible =
                match mContact with
                | None -> false
                | Some x when x.Id = 0 -> false
                | Some _ -> true

            View.ContentPage(
                title=(if (mFirstName = "" && mLastName = "") then "New Contact" else mFirstName + " " + mLastName),
                toolbarItems=[
                    mkToolbarButton "Save" (fun() -> (mContact, mFirstName, mLastName, mAddress, mIsFavorite) |> SaveContact |> dispatch)
                ],
                content=View.StackLayout(
                    children=[

                        View.Grid(
                            margin=Thickness(20., 20., 20., 0.),
                            coldefs=[ 65.; GridLength.Star ],
                            rowdefs=[ GridLength.Star; GridLength.Star ],
                            children=[
                                View.Button(text="Photo", command=(fun () -> dispatch AddPhoto), heightRequest=65., margin=Thickness(0., 0., 20., 0.), verticalOptions=LayoutOptions.Center, horizontalOptions=LayoutOptions.Center).GridRowSpan(2)
                                View.Entry(placeholder="First name", text=mFirstName, textChanged=(fun e -> e.NewTextValue |> UpdateFirstName |> dispatch)).GridColumn(1)
                                View.Entry(placeholder="Last name", text=mLastName, textChanged=(fun e -> e.NewTextValue |> UpdateLastName |> dispatch)).GridColumn(1).GridRow(1)
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