namespace ElmishContacts

open Models
open Repository
open Style
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module MainPage =
    type Msg = | ContactsLoaded of Contact list
               | ContactSelected of Contact
               | AddNewContactTapped
               | ShowMapTapped

    type ExternalMsg = | NoOp
                       | Select of Contact
                       | AddNewContact
                       | ShowMap

    type Model =
        {
            Contacts: Contact list option
        }

    let loadAsyncCmd dbPath = async {
        let! contacts = loadAllContacts dbPath
        return ContactsLoaded contacts
    }

    let findContactIn (groupedContacts: (string * Contact list) list) (gIndex: int, iIndex: int) =
        groupedContacts.[gIndex]
        |> (fun (gName, items) -> items.[iIndex])

    let init dbPath () =
        {
            Contacts = None
        }, Cmd.ofAsyncMsg (loadAsyncCmd dbPath)

    let update dbPath msg model =
        match msg with
        | ContactsLoaded contacts -> { model with Contacts = Some contacts }, Cmd.none, ExternalMsg.NoOp
        | ContactSelected contact -> model, Cmd.none, (ExternalMsg.Select contact)
        | AddNewContactTapped -> model, Cmd.none, ExternalMsg.AddNewContact
        | ShowMapTapped -> model, Cmd.none, ExternalMsg.ShowMap

    let view model dispatch =
        dependsOn model.Contacts (fun model mContacts ->
            View.ContentPage(
                title="ElmContact",
                toolbarItems=[
                    mkToolbarButton "Add" (fun() -> dispatch AddNewContactTapped)
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
                                |> List.groupBy (fun c -> c.Name.[0].ToString().ToLower())
                            
                            [
                                View.ListViewGrouped(
                                    rowHeight=55,
                                    verticalOptions=LayoutOptions.FillAndExpand,
                                    itemTapped=(findContactIn groupedContacts >> ContactSelected >> dispatch),
                                    items=
                                        [
                                            for (groupName, items) in groupedContacts do
                                                yield mkGroupView groupName,
                                                        [
                                                            for contact in items do
                                                                yield mkCachedCellView contact.Name contact.Address contact.IsFavorite
                                                        ]
                                        ]
                                )
                                View.Button(
                                    text="Show contacts on map",
                                    command=(fun () -> dispatch ShowMapTapped)
                                )
                            ]
                ) 
            )
        )