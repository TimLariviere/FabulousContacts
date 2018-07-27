namespace ElmishContacts

open Models
open Repository
open Style
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews

module ItemPage =
    type Msg = | UpdateName of string
               | UpdateAddress of string
               | UpdateIsFavorite of bool
               | SaveContact of Contact option * name: string * address: string * isFavorite: bool
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
            Name: string
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
            View.displayAlertWithConfirm ("Delete " + contact.Name) "This action is definitive. Are you sure?" "Yes" "No"

        if shouldDelete then
            do! deleteContact dbPath contact
            return Some (ContactDeleted contact)
        else
            return None
    }

    let init contact =
        let model =
            match contact with
            | Some c ->
                {
                    Contact = Some c
                    Name = c.Name
                    Address = c.Address
                    IsFavorite = c.IsFavorite
                }
            | None ->
                {
                    Contact = None
                    Name = ""
                    Address = ""
                    IsFavorite = false
                }

        model, Cmd.none

    let update dbPath msg (model: Model) =
        match msg with
        | UpdateName name ->
            { model with Name = name }, Cmd.none, ExternalMsg.NoOp
        | UpdateAddress address ->
            { model with Address = address }, Cmd.none, ExternalMsg.NoOp
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none, ExternalMsg.NoOp
        | SaveContact (contact, name, address, isFavorite) ->
            let newContact =
                match contact with
                | None -> { Id = 0; Name = name; Address = address; IsFavorite = isFavorite }
                | Some c -> { c with Name = name; Address = address; IsFavorite = isFavorite }
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
        dependsOn (model.Contact, model.Name, model.Address, model.IsFavorite) (fun model (mContact, mName, mAddress, mIsFavorite) ->
            let isDeleteButtonVisible =
                match mContact with
                | None -> false
                | Some x when x.Id = 0 -> false
                | Some _ -> true

            View.ContentPage(
                title=(if mName = "" then "New Contact" else mName),
                toolbarItems=[
                    mkToolbarButton "Save" (fun() -> (mContact, mName, mAddress, mIsFavorite) |> SaveContact |> dispatch)
                ],
                content=View.StackLayout(
                    children=[
                        mkFormLabel "Name"
                        mkFormEntry mName (fun e -> e.NewTextValue |> UpdateName |> dispatch)
                        mkFormLabel "Address"
                        mkFormEntry mAddress (fun e -> e.NewTextValue |> UpdateAddress |> dispatch)
                        mkFormLabel "Is Favorite"
                        mkFormSwitch mIsFavorite (fun e -> e.Value |> UpdateIsFavorite |> dispatch)
                        mkDestroyButton "Delete" (fun () -> mContact.Value |> DeleteContact |> dispatch) isDeleteButtonVisible
                    ]
                )
            )
        )