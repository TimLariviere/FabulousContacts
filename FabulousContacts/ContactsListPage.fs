namespace FabulousContacts

open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms
open Models
open Style

module ContactsListPage =
    // Declarations
    type Msg =
        | AboutTapped
        | AddNewContactTapped
        | UpdateFilterText of string
        | ContactsLoaded of Contact list
        | ContactSelected of Contact

    type ExternalMsg =
        | NoOp
        | NavigateToAbout
        | NavigateToNewContact
        | NavigateToDetail of Contact

    type Model =
        { Contacts: Contact list
          FilterText: string
          FilteredContacts: Contact list }

    // Functions
    let filterContacts filterText (contacts: Contact list) =
        match filterText with
        | null | "" ->
            contacts
        | s ->
            contacts
            |> List.filter (fun c -> c.FirstName.Contains s || c.LastName.Contains s)

    let groupContacts contacts =
        contacts
        |> List.groupBy (fun c -> c.LastName.[0].ToString().ToUpper())
        |> List.map (fun (k, cs) -> (k, cs |> List.sortBy (fun c -> c.FirstName)))
        |> List.sortBy (fun (k, _) -> k)

    let findContactIn (groupedContacts: (string * Contact list) list) (gIndex: int, iIndex: int) =
        groupedContacts.[gIndex]
        |> (fun (_, items) -> items.[iIndex])

    // Lifecycle
    let init () =
        let m =
            { Contacts = []
              FilterText = ""
              FilteredContacts = [] }
        m, Cmd.none

    let update msg model =
        match msg with
        | AboutTapped ->
            model, Cmd.none, ExternalMsg.NavigateToAbout
        | AddNewContactTapped ->
            model, Cmd.none, ExternalMsg.NavigateToNewContact
        | UpdateFilterText filterText ->
            let filteredContacts = filterContacts filterText model.Contacts
            let m = { model with FilterText = filterText; FilteredContacts = filteredContacts }
            m, Cmd.none, ExternalMsg.NoOp
        | ContactsLoaded contacts ->
            let filteredContacts = filterContacts model.FilterText contacts
            let m = { model with Contacts = contacts; FilteredContacts = filteredContacts }
            m, Cmd.none, ExternalMsg.NoOp
        | ContactSelected contact ->
            model, Cmd.none, ExternalMsg.NavigateToDetail contact


    let mkToolBarItems dispatch = [
        View.ToolbarItem(text = "About",
                         command = fun () -> dispatch AboutTapped)
        View.ToolbarItem(text = "+",
                         command = fun () -> dispatch AddNewContactTapped)
    ]

    let mkStackLayoutChildren filterText contacts dispatch =
        let groupedContacts = groupContacts contacts
        let textChanged =
            debounce 250 (fun (e: TextChangedEventArgs) -> e.NewTextValue |> UpdateFilterText |> dispatch)
        let searchBar =
            View.SearchBar(text = filterText,
                           textChanged = textChanged,
                           backgroundColor = accentColor,
                           cancelButtonColor = accentTextColor)
        let findContact = findContactIn groupedContacts >> ContactSelected >> dispatch
        let mkCachedCell c a = mkCachedCellView c.Picture (sprintf "%s %s" c.FirstName c.LastName) a c.IsFavorite
        let mkGroupedItems = [
            for (groupName, items) in groupedContacts do
                yield groupName, mkGroupView groupName, [
                    for contact in items do
                        let address = contact.Address.Replace("\n", " ")
                        yield mkCachedCell contact address
                    ]
            ]
        let groupedView =
            View.ListViewGrouped(verticalOptions = LayoutOptions.FillAndExpand,
                                 rowHeight = 60,
                                 selectionMode = ListViewSelectionMode.None,
                                 showJumpList = (contacts.Length > 10),
                                 itemTapped = findContact,
                                 items = mkGroupedItems)
        [ searchBar; groupedView]

    let view title model dispatch =
        dependsOn (title, model.FilterText, model.FilteredContacts) (fun model (mTitle, mFilterText, mContacts) ->
            View.ContentPage(
                title = mTitle,
                toolbarItems = mkToolBarItems dispatch,
                content = View.StackLayout(
                    spacing = 0.,
                    children = mkStackLayoutChildren mFilterText mContacts dispatch
                )
            )
        )