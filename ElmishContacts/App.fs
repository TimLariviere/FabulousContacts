namespace ElmishContacts

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open ElmishContacts.Style
open ElmishContacts.Models
open ElmishContacts.Repository
open Xamarin.Forms.Maps

module App =
    type Model = 
        {
            Contacts: Contact list option
            SelectedContact: Contact option
            Name: string
            Address: string
            IsFavorite: bool
            IsMapShowing: bool
            Pins: ContactPin list option
        }

    type Msg = | ContactsLoaded of Contact list | NavigationPopped
               | Select of Contact | AddNewContact | ShowMap
               | UpdateName of string | UpdateAddress of string | UpdateIsFavorite of bool
               | SaveContact of Contact * name: string * address: string * isFavorite: bool | DeleteContact of Contact
               | ContactAdded of Contact | ContactUpdated of Contact | ContactDeleted of Contact
               | PinsLoaded of ContactPin list

    let initModel = 
        {
            Contacts = None
            SelectedContact = None
            Name = ""
            Address = ""
            IsFavorite = false
            IsMapShowing = false
            Pins = None
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

    let loadPinsAsync (contacts: Contact list) = async {
        let geocoder = Geocoder()

        let gettingPositions =
            contacts
            |> List.map (fun c -> async {
                let! positions = geocoder.GetPositionsForAddressAsync(c.Address) |> Async.AwaitTask
                let position = positions |> Seq.tryHead
                return (c, position)
            })
            |> Async.Parallel

        let! contactsAndPositions = gettingPositions

        let pins = contactsAndPositions
                   |> Array.filter (fun (_, p) -> Option.isSome p)
                   |> Array.map (fun (c, p) -> { Position = p.Value; Label = c.Name; PinType = PinType.Place; Address = c.Address})
                   |> Array.toList

        return PinsLoaded pins
    }

    let updateModelAndNavBack model newContacts =
        { model with Contacts = Some newContacts; SelectedContact = None; Name = ""; Address = ""; IsFavorite = false }, Cmd.ofMsg NavigationPopped

    let updateModelAfterNavPopped model =
            match (model.SelectedContact, model.IsMapShowing) with
            | (None, false) -> model, Cmd.none
            | (None, true) -> { model with IsMapShowing = false }, Cmd.none
            | (Some _, false) -> { model with SelectedContact = None; Name = ""; Address = ""; IsFavorite = false }, Cmd.none
            | (Some _, true) -> { model with SelectedContact = None; Name = ""; Address = ""; IsFavorite = false }, Cmd.ofAsyncMsg (loadPinsAsync model.Contacts.Value)

    let init dbPath () = initModel, Cmd.ofAsyncMsg (loadAsync dbPath)

    let update dbPath msg model =
        match msg with
        | NavigationPopped ->
            updateModelAfterNavPopped model
        | ContactsLoaded contacts ->
            { model with Contacts = Some contacts }, Cmd.none
        | Select contact ->
            { model with SelectedContact = Some contact; Name = contact.Name; Address = contact.Address; IsFavorite = contact.IsFavorite }, Cmd.none
        | AddNewContact ->
            { model with SelectedContact = Some Contact.NewContact; Name = ""; Address = ""; IsFavorite = false }, Cmd.none
        | ShowMap ->
            { model with IsMapShowing = true }, Cmd.ofAsyncMsg (loadPinsAsync model.Contacts.Value)
        | UpdateName name ->
            { model with Name = name }, Cmd.none
        | UpdateAddress address ->
            { model with Address = address }, Cmd.none
        | UpdateIsFavorite isFavorite ->
            { model with IsFavorite = isFavorite }, Cmd.none
        | SaveContact (contact, name, address, isFavorite) ->
            let newContact = { contact with Name = name; Address = address; IsFavorite = isFavorite }
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
        | PinsLoaded pins ->
            { model with Pins = Some pins }, Cmd.none

    let test (groupedContacts: (string * Contact list) list) (gIndex: int, iIndex: int) =
        groupedContacts.[gIndex]
        |> (fun (gName, items) -> items.[iIndex])

    let view dbPath (model: Model) dispatch =
        let mkCachedCellView name address isFavorite =
            dependsOn (name, address, isFavorite) (fun _ (cName, cAddress, cIsFavorite) -> mkCellView cName cAddress cIsFavorite)

        let mainPage =
            dependsOn model.Contacts (fun model mContacts ->
                View.ContentPage(
                    title="ElmContact",
                    toolbarItems=[
                        mkToolbarButton "Add" (fun() -> AddNewContact |> dispatch)
                    ],
                    content=View.StackLayout(
                        children=
                            match mContacts with
                            | None ->
                                [ mkCentralLabel "Loading..." ]
                            | Some [] ->
                                [ mkCentralLabel "No contact" ]
                            | Some contacts ->
                                let groupedContacts =
                                    contacts
                                    |> List.groupBy (fun c -> c.Name.[0].ToString())
                                
                                [
                                    View.ListViewGrouped(
                                        verticalOptions=LayoutOptions.FillAndExpand,
                                        itemTapped=(test groupedContacts >> Select >> dispatch),
                                        items=
                                            [
                                                for (groupName, items) in groupedContacts do
                                                    yield View.Label groupName, [
                                                        for contact in items do
                                                            yield mkCachedCellView contact.Name contact.Address contact.IsFavorite
                                                    ]

                                            ]
                                    )
                                    View.Button(
                                        text="Show contacts on map",
                                        command=(fun () -> ShowMap |> dispatch)
                                    )
                                ]
                    ) 
                )
            )

        let itemPage =
            dependsOn (model.SelectedContact, model.Name, model.Address, model.IsFavorite) (fun model (mSelectedContact, mName, mAddress, mIsFavorite) ->
                let isDeleteButtonVisible =
                    match mSelectedContact with
                    | None -> false
                    | Some x when x.Id = 0 -> false
                    | Some x -> true

                View.ContentPage(
                    title=(if mName = "" then "New Contact" else mName),
                    toolbarItems=[
                        mkToolbarButton "Save" (fun() -> (mSelectedContact.Value, mName, mAddress, mIsFavorite) |> SaveContact |> dispatch)
                    ],
                    content=View.StackLayout(
                        children=[
                            mkFormLabel "Name"
                            mkFormEntry mName (fun e -> e.NewTextValue |> UpdateName |> dispatch)
                            mkFormLabel "Address"
                            mkFormEntry mAddress (fun e -> e.NewTextValue |> UpdateAddress |> dispatch)
                            mkFormLabel "Is Favorite"
                            mkFormSwitch mIsFavorite (fun e -> e.Value |> UpdateIsFavorite |> dispatch)
                            mkDestroyButton "Delete" (fun () -> mSelectedContact.Value |> DeleteContact |> dispatch) isDeleteButtonVisible
                        ]
                    )
                )
            )

        let mapPage =
            dependsOn model.Pins (fun model (mPins) ->
                let paris = Position(48.8566, 2.3522)

                View.ContentPage(
                    content=
                        match mPins with
                        | None ->
                            mkCentralLabel "Loading..."
                        | Some pins ->
                            View.Map(
                                hasZoomEnabled=true,
                                hasScrollEnabled=true,
                                requestedRegion=MapSpan.FromCenterAndRadius(paris, Distance.FromKilometers(25.)),
                                pins=[
                                    for pin in pins do
                                        yield View.Pin(position=pin.Position, label=pin.Label, pinType=pin.PinType, address=pin.Address)
                                ]
                            )
                )
            )



        View.NavigationPage(
            barTextColor=Color.White,
            barBackgroundColor=Color.FromHex("#3080b1"),
            popped=(fun e -> NavigationPopped |> dispatch),
            pages=
                match (model.SelectedContact, model.IsMapShowing) with
                | (None, false) -> [ mainPage ]
                | (None, true) -> [ mainPage; mapPage ]
                | (Some _, false) -> [ mainPage; itemPage ]
                | (Some _, true) -> [ mainPage; mapPage; itemPage ]
        )

type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view dbPath

    let runner = 
        Program.mkProgram init update view
        |> Program.runWithDynamicView app
