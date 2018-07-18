namespace ElmishContacts

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open ElmishContacts.Style
open ElmishContacts.Models
open ElmishContacts.Repository

module App =

    type Model = 
        {
            Contacts: Contact list option
            SelectedContact: Contact option
            Name: string
            IsFavorite: bool
        }

    type Msg = | ContactsLoaded of Contact list | Select of Contact | AddNewContact
               | UpdateName of string | UpdateIsFavorite of bool
               | SaveContact of Contact * name: string * isFavorite: bool | DeleteContact of Contact
               | ContactAdded of Contact | ContactUpdated of Contact | ContactDeleted of Contact

    let initModel = 
        {
            Contacts = None;
            SelectedContact = None;
            Name = ""
            IsFavorite = false
        }

    let loadAsync dbPath = async {
        let! contacts = loadAllContacts dbPath
        return ContactsLoaded contacts
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

    let deleteAsync dbPath contact = async {
        do! deleteContact dbPath contact
        return ContactDeleted contact
    }

    let updateModelAndNavBack model newContacts =
        { model with Contacts = Some newContacts; SelectedContact = None; Name = ""; IsFavorite = false }, Cmd.none

    let init dbPath () = initModel, Cmd.ofAsyncMsg (loadAsync dbPath)

    let update dbPath msg model =
        match msg with
        | ContactsLoaded contacts ->
            { model with Contacts = Some contacts }, Cmd.none
        | Select contact ->
            { model with SelectedContact = Some contact; Name = contact.Name; IsFavorite = contact.IsFavorite }, Cmd.none
        | AddNewContact ->
            { model with SelectedContact = Some Contact.NewContact; Name = ""; IsFavorite = false }, Cmd.none
        | UpdateName name ->
            { model with Name = name }, Cmd.none
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none
        | SaveContact (contact, name, isFavorite) ->
            let newContact = { contact with Name = name; IsFavorite = isFavorite }
            model, Cmd.ofAsyncMsg (saveAsync dbPath newContact)
        | DeleteContact contact ->
            model, Cmd.ofAsyncMsg (deleteAsync dbPath contact)
        | ContactAdded contact -> 
            let newContacts = model.Contacts.Value @ [ contact ]
            updateModelAndNavBack model newContacts
        | ContactUpdated contact -> 
            let newContacts = model.Contacts.Value |> List.map (fun c -> if c.Id = contact.Id then contact else c)
            updateModelAndNavBack model newContacts
        | ContactDeleted contact ->
            let newContacts = model.Contacts.Value |> List.filter (fun c -> c <> contact)
            updateModelAndNavBack model newContacts

    let view dbPath (model: Model) dispatch =
        let mainPage =
            View.ContentPage(
                title="My Contacts",
                toolbarItems=[
                    mkToolbarButton "Add" (fun() -> AddNewContact |> dispatch)
                ],
                content=View.StackLayout(
                    children=
                        match model.Contacts with
                        | None | Some [] ->
                            [ View.Label(text="Aucun contact", horizontalOptions=LayoutOptions.Center, verticalOptions=LayoutOptions.CenterAndExpand) ]
                        | Some contacts ->
                            [
                                View.ListView(
                                    verticalOptions=LayoutOptions.FillAndExpand,
                                    itemTapped=(fun i -> contacts.[i] |> Select |> dispatch),
                                    items=
                                        [
                                            for contact in contacts do
                                                yield mkCellView contact.Name contact.IsFavorite
                                        ]
                                )
                            ]
                ) 
            )

        let itemPage =
            let isDeleteButtonVisible =
                match model.SelectedContact with
                | None -> false
                | Some x when x.Id = 0 -> false
                | Some x -> true

            View.ContentPage(
                title=(if model.Name = "" then "New Contact" else model.Name),
                toolbarItems=[
                    mkToolbarButton "Save" (fun() -> (model.SelectedContact.Value, model.Name, model.IsFavorite) |> SaveContact |> dispatch)
                ],
                content=View.StackLayout(
                    children=[
                        mkFormLabel "Name"
                        mkFormEntry model.Name (fun e -> e.NewTextValue |> UpdateName |> dispatch)
                        mkFormLabel "Is Favorite"
                        mkFormSwitch model.IsFavorite (fun e -> e.Value |> UpdateIsFavorite |> dispatch)
                        mkDestroyButton "Delete" (fun () -> model.SelectedContact.Value |> DeleteContact |> dispatch) isDeleteButtonVisible
                    ]
                )
            )

        View.NavigationPage(
            pages=
                match model.SelectedContact with
                | None -> [ mainPage ]
                | Some _ -> [ mainPage; itemPage ]
        )

type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view dbPath

    let runner = 
        Program.mkProgram init update view
        |> Program.runWithDynamicView app
