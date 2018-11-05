namespace ElmishContacts

open Models
open Repository
open Style
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms.PlatformConfiguration.AndroidSpecific

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

        { Contacts = None
          TabAllContactsModel = modelAllContacts
          TabFavContactsModel = modelFavContacts
          TabMapModel = modelMap
        }, Cmd.batch [
            Cmd.ofAsyncMsg (loadAsync dbPath)
            Cmd.map TabAllContactsMsg msgAllContacts
            Cmd.map TabFavContactsMsg msgFavContacts
            Cmd.map TabMapMsg msgMap
        ]

    let updateContactsList msg mapMsgFunc model =
        let m, cmd, externalMsg = ContactsListPage.update msg model

        let cmd2, externalMsg2 =
            match externalMsg with
            | ContactsListPage.ExternalMsg.NoOp -> Cmd.none, ExternalMsg.NoOp
            | ContactsListPage.ExternalMsg.NavigateToAbout -> Cmd.none, ExternalMsg.NavigateToAbout
            | ContactsListPage.ExternalMsg.NavigateToNewContact -> Cmd.none, ExternalMsg.NavigateToNewContact
            | ContactsListPage.ExternalMsg.NavigateToDetail contact -> Cmd.none, (ExternalMsg.NavigateToDetail contact)

        m, Cmd.batch [ Cmd.map mapMsgFunc cmd; cmd2 ], externalMsg2

    let updateContacts model contacts =
        let allMsg = (ContactsListPage.Msg.ContactsLoaded contacts)
        let favMsg = (ContactsListPage.Msg.ContactsLoaded (contacts |> List.filter (fun c -> c.IsFavorite)))
        let mapMsg = (MapPage.Msg.LoadPins contacts)

        { model with Contacts = Some contacts }, Cmd.batch [
            Cmd.ofMsg (TabAllContactsMsg allMsg)
            Cmd.ofMsg (TabFavContactsMsg favMsg)
            Cmd.ofMsg (TabMapMsg mapMsg)
        ], ExternalMsg.NoOp

    let update msg model =
        match msg with
        | TabAllContactsMsg msg ->
            let m, cmd, externalMsg = updateContactsList msg TabAllContactsMsg model.TabAllContactsModel
            { model with TabAllContactsModel = m }, cmd, externalMsg
        | TabFavContactsMsg msg ->
            let m, cmd, externalMsg = updateContactsList msg TabFavContactsMsg model.TabFavContactsModel
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
            let tabAllContacts = (ContactsListPage.view "All" model.TabAllContactsModel (TabAllContactsMsg >> dispatch)).Icon("alltab.png")
            let tabFavContacts = (ContactsListPage.view "Favorites" model.TabFavContactsModel (TabFavContactsMsg >> dispatch)).Icon("favoritetab.png")
            let tabMap = MapPage.view model.TabMapModel (TabMapMsg >> dispatch)

            dependsOn (tabAllContacts, tabFavContacts, tabMap) (fun _ (contacts, favorites, map) ->
                View.TabbedPage(
                    created=(fun target -> target.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom) |> ignore),
                    title=title,
                    children=[ contacts; favorites; map ]
                )
            )