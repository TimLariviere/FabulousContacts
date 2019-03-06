namespace ElmishContacts

open Models
open Repository
open Style
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms.PlatformConfiguration.AndroidSpecific
open Xamarin.Forms.PlatformConfiguration

module MainPage =
    // Declarations
    type Msg =
        | TabAllContactsMsg of ContactsListPage.Msg
        | TabFavContactsMsg of ContactsListPage.Msg
        | TabMapMsg of MapPage.Msg
        | ContactsLoaded of Contact list
        | ContactAdded of Contact
        | ContactUpdated of Contact
        | ContactDeleted of Contact
        | NoContactAboutTapped
        | NoContactAddNewContactTapped

    type ExternalMsg =
        | NoOp
        | NavigateToAbout
        | NavigateToNewContact
        | NavigateToDetail of Contact

    type Model =
        { Contacts: Contact list option
          TabAllContactsModel: ContactsListPage.Model
          TabFavContactsModel: ContactsListPage.Model
          TabMapModel: MapPage.Model }

    // Functions
    let loadAsync dbPath = async {
        let! contacts = loadAllContacts dbPath
        return ContactsLoaded contacts
    }

    // Lifecycle
    let init dbPath () =
        let (modelAllContacts, msgAllContacts) = ContactsListPage.init()
        let (modelFavContacts, msgFavContacts) = ContactsListPage.init()
        let (modelMap, msgMap) = MapPage.init()

        let model =
            { Contacts = None
              TabAllContactsModel = modelAllContacts
              TabFavContactsModel = modelFavContacts
              TabMapModel = modelMap }

        let cmd = Cmd.batch [
            Cmd.ofAsyncMsg (loadAsync dbPath)
            Cmd.map TabAllContactsMsg msgAllContacts
            Cmd.map TabFavContactsMsg msgFavContacts
            Cmd.map TabMapMsg msgMap
        ]

        model, cmd

    let updateContactsList msg model =
        let pageModel, pageCmd, pageExternalMsg = ContactsListPage.update msg model

        let externalMsg =
            match pageExternalMsg with
            | ContactsListPage.ExternalMsg.NoOp -> ExternalMsg.NoOp
            | ContactsListPage.ExternalMsg.NavigateToAbout -> ExternalMsg.NavigateToAbout
            | ContactsListPage.ExternalMsg.NavigateToNewContact -> ExternalMsg.NavigateToNewContact
            | ContactsListPage.ExternalMsg.NavigateToDetail contact -> (ExternalMsg.NavigateToDetail contact)

        pageModel, pageCmd, externalMsg

    let updateContacts model contacts =
        let allMsg = (ContactsListPage.Msg.ContactsLoaded contacts)
        let favMsg = (ContactsListPage.Msg.ContactsLoaded (contacts |> List.filter (fun c -> c.IsFavorite)))
        let mapMsg = (MapPage.Msg.LoadPins contacts)

        let model = { model with Contacts = Some contacts }
        let cmd = Cmd.batch [
            Cmd.ofMsg (TabAllContactsMsg allMsg)
            Cmd.ofMsg (TabFavContactsMsg favMsg)
            Cmd.ofMsg (TabMapMsg mapMsg)
        ]

        model, cmd, ExternalMsg.NoOp

    let update msg model =
        match msg with
        | TabAllContactsMsg msg ->
            let m, cmd, externalMsg = updateContactsList msg model.TabAllContactsModel
            { model with TabAllContactsModel = m }, cmd, externalMsg
        | TabFavContactsMsg msg ->
            let m, cmd, externalMsg = updateContactsList msg model.TabFavContactsModel
            { model with TabFavContactsModel = m }, cmd, externalMsg
        | TabMapMsg msg ->
            let m, cmd = MapPage.update msg model.TabMapModel
            { model with TabMapModel = m }, (Cmd.map TabMapMsg cmd), ExternalMsg.NoOp
        | ContactsLoaded contacts ->
            updateContacts model contacts
        | ContactAdded contact ->
            let newContacts = contact :: model.Contacts.Value
            updateContacts model newContacts
        | ContactUpdated contact ->
            let newContacts = model.Contacts.Value |> List.map (fun c -> if c.Id = contact.Id then contact else c)
            updateContacts model newContacts
        | ContactDeleted contact ->
            let newContacts = model.Contacts.Value |> List.filter (fun c -> c <> contact)
            updateContacts model newContacts
        | NoContactAboutTapped ->
            model, Cmd.none, ExternalMsg.NavigateToAbout
        | NoContactAddNewContactTapped ->
            model, Cmd.none, ExternalMsg.NavigateToNewContact

    let view model dispatch =
        let title = "ElmishContacts"

        match model.Contacts with
        | None ->
            dependsOn () (fun model () ->
                View.ContentPage(
                    title=title,
                    content=View.StackLayout(
                        children=[ mkCentralLabel "Loading..." ]
                    )
                )
            )

        | Some [] ->
            dependsOn () (fun model () ->
                View.ContentPage(
                    title=title,
                    toolbarItems=[
                        View.ToolbarItem(text="About", command=(fun() -> dispatch NoContactAboutTapped))
                        View.ToolbarItem(text="+", command=(fun() -> dispatch NoContactAddNewContactTapped))
                    ],
                    content=View.StackLayout(
                        children=[ mkCentralLabel "No contact" ]
                    )
                )
            )

        | Some _ ->
            dependsOn (model.TabAllContactsModel, model.TabFavContactsModel, model.TabMapModel) (fun _ (contacts, favorites, map) ->

                let tabAllContacts = (ContactsListPage.view "All" contacts (TabAllContactsMsg >> dispatch)).Icon("alltab.png")
                let tabFavContacts = (ContactsListPage.view "Favorites" favorites (TabFavContactsMsg >> dispatch)).Icon("favoritetab.png")
                let tabMap = MapPage.view map (TabMapMsg >> dispatch)

                View.TabbedPage(
                    created=(fun target -> target.On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom) |> ignore),
                    title=title,
                    children=[ tabAllContacts; tabFavContacts; tabMap ]
                )
            )