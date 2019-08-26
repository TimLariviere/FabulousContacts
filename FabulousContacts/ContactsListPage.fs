namespace FabulousContacts

open Fabulous
open Fabulous.XamarinForms
open FabulousContacts.Components
open FabulousContacts.Models
open FabulousContacts.Style
open Xamarin.Forms

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
    let initModel =
        { Contacts = []
          FilterText = ""
          FilteredContacts = [] }
    
    let init () =
        initModel, Cmd.none

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

    let view title model dispatch =
        let cachedCell contact address =
            cachedCellView contact.Picture (sprintf "%s %s" contact.FirstName contact.LastName) address contact.IsFavorite
        
        dependsOn (title, model.FilterText, model.FilteredContacts) (fun model (mTitle, mFilterText, mContacts) ->
            let groupedContacts = groupContacts mContacts
            
            // Actions
            let goToAbout = fun () -> dispatch AboutTapped
            let addNewContact = fun () -> dispatch AddNewContactTapped
            let selectContact = findContactIn groupedContacts >> ContactSelected >> dispatch
            let updateFilter = debounce 250 (fun (e: TextChangedEventArgs) -> e.NewTextValue |> UpdateFilterText |> dispatch)
            
            // View
            View.ContentPage(
                title = mTitle,
                toolbarItems = [
                    View.ToolbarItem(text = Strings.Common_About,
                                     command = goToAbout)
                    View.ToolbarItem(text = "+",
                                     command = addNewContact)
                ],
                content = View.StackLayout(
                    spacing = 0.,
                    children = [
                        View.SearchBar(text = mFilterText,
                                       textChanged = updateFilter,
                                       backgroundColor = accentColor,
                                       cancelButtonColor = accentTextColor)
                        
                        View.ListViewGrouped(verticalOptions = LayoutOptions.FillAndExpand,
                                             rowHeight = 60,
                                             selectionMode = ListViewSelectionMode.None,
                                             showJumpList = (mContacts.Length > 10),
                                             itemTapped = selectContact,
                                             items = [
                            for (groupName, items) in groupedContacts do
                                yield groupName, groupView groupName, [
                                    for contact in items do
                                        let address = contact.Address.Replace("\n", " ")
                                        yield cachedCell contact address
                                    ]
                            ]
                        )
                    ]
                )
            )
        )