namespace ElmishContacts

open Models
open Style
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module ContactsListPage =
    // Declarations
    type Msg = | AboutTapped
               | AddNewContactTapped
               | UpdateFilterText of string
               | ContactsLoaded of Contact list
               | ContactSelected of Contact

    type ExternalMsg = | NoOp
                       | NavigateToAbout
                       | NavigateToNewContact
                       | NavigateToDetail of Contact

    type Model =
        {
            Contacts: Contact list
            FilterText: string
            FilteredContacts: Contact list
        }

    // Functions
    let filterContacts filterText (contacts: Contact list) =
        match filterText with
        | null | "" -> contacts
        | _ -> contacts |> List.filter (fun c ->(c.FirstName.Contains(filterText) || c.LastName.Contains(filterText)))

    let groupContacts contacts =
        contacts
        |> List.sortBy (fun c -> c.FirstName)
        |> List.groupBy (fun c -> c.LastName.[0].ToString().ToUpper())

    let findContactIn (groupedContacts: (string * Contact list) list) (gIndex: int, iIndex: int) =
        groupedContacts.[gIndex]
        |> (fun (_, items) -> items.[iIndex])

    // Lifecycle
    let init () =
        {
            Contacts = []
            FilterText = ""
            FilteredContacts = []
        }, Cmd.none

    let update msg model =
        match msg with
        | AboutTapped ->
            model, Cmd.none, ExternalMsg.NavigateToAbout
        | AddNewContactTapped ->
            model, Cmd.none, ExternalMsg.NavigateToNewContact
        | UpdateFilterText filterText ->
            { model with FilterText = filterText; FilteredContacts = (filterContacts filterText model.Contacts) }, Cmd.none, ExternalMsg.NoOp
        | ContactsLoaded contacts ->
            { model with Contacts = contacts; FilteredContacts = (filterContacts model.FilterText contacts) }, Cmd.none, ExternalMsg.NoOp
        | ContactSelected contact ->
            model, Cmd.none, (ExternalMsg.NavigateToDetail contact)


    let view title model dispatch =
        dependsOn (title, model.FilterText, model.FilteredContacts) (fun model (mTitle, mFilterText, mContacts) ->
            let groupedContacts = groupContacts mContacts
            View.ContentPage(
                title=mTitle,
                toolbarItems=[
                    View.ToolbarItem(text="About", command=(fun() -> dispatch AboutTapped))
                    View.ToolbarItem(text="+", command=(fun() -> dispatch AddNewContactTapped))
                ],
                content=View.StackLayout(
                    children=[
                        View.StackLayout(
                            spacing=0.,
                            children=[
                                View.SearchBar(text=mFilterText, textChanged=(fun e -> e.NewTextValue |> UpdateFilterText |> dispatch), backgroundColor=accentColor, cancelButtonColor=accentTextColor)
                                View.ListViewGrouped_XF31(
                                    verticalOptions=LayoutOptions.FillAndExpand,
                                    rowHeight=60,
                                    selectionMode=ListViewSelectionMode.None,
                                    showJumpList=(mContacts.Length > 10),
                                    itemTapped=(findContactIn groupedContacts >> ContactSelected >> dispatch),
                                    items=
                                        [
                                            for (groupName, items) in groupedContacts do
                                                yield groupName, mkGroupView groupName,
                                                        [
                                                            for contact in items do
                                                                let address = contact.Address.Replace("\n", " ")
                                                                yield mkCachedCellView contact.Picture (contact.FirstName + " " + contact.LastName) address contact.IsFavorite
                                                        ]
                                        ]
                                )
                            ]
                        )
                    ]
                )
            )
        )