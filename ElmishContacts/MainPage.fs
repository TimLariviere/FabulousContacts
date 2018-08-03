namespace ElmishContacts

open Models
open Repository
open Style
open Images
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open Xamarin.Forms.Maps

module MainPage =
    type Msg = | ContactsLoaded of Contact list
               | ContactSelected of Contact
               | AboutTapped
               | AddNewContactTapped
               | ContactAdded of Contact
               | ContactUpdated of Contact
               | ContactDeleted of Contact
               | PinsLoaded of ContactPin list

    type ExternalMsg = | NoOp
                       | Select of Contact
                       | About
                       | AddNewContact

    type Model =
        {
            Contacts: Contact list option
            Pins: ContactPin list option
        }

    let loadAsyncCmd dbPath = async {
        let! contacts = loadAllContacts dbPath
        return ContactsLoaded contacts
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
                   |> Array.map (fun (c, p) -> { Position = p.Value; Label = (c.FirstName + " " + c.LastName); PinType = PinType.Place; Address = c.Address})
                   |> Array.toList

        return PinsLoaded pins
    }

    let groupContacts contacts =
        contacts
        |> List.map (fun c -> (c, c.LastName.[0].ToString().ToUpper()))
        |> List.sortBy snd
        |> List.groupBy snd
        |> List.map (fun (k, l) -> (k, List.map fst l))

    let findContactIn (groupedContacts: (string * Contact list) list) (gIndex: int, iIndex: int) =
        groupedContacts.[gIndex]
        |> (fun (_, items) -> items.[iIndex])

    let init dbPath () =
        {
            Contacts = None
            Pins = None
        }, Cmd.ofAsyncMsg (loadAsyncCmd dbPath)

    let update msg model =
        match msg with
        | ContactsLoaded contacts ->
            { model with Contacts = Some contacts }, Cmd.ofAsyncMsg (loadPinsAsync contacts), ExternalMsg.NoOp
        | PinsLoaded pins ->
            { model with Pins = Some pins }, Cmd.none, ExternalMsg.NoOp
        | ContactSelected contact ->
            model, Cmd.none, (ExternalMsg.Select contact)
        | AboutTapped ->
            model, Cmd.none, ExternalMsg.About
        | AddNewContactTapped ->
            model, Cmd.none, ExternalMsg.AddNewContact
        | ContactAdded contact ->
            let newContacts = model.Contacts.Value @ [ contact ]
            { model with Contacts = Some newContacts }, Cmd.ofAsyncMsg (loadPinsAsync newContacts), ExternalMsg.NoOp
        | ContactUpdated contact ->
            let previousContact = model.Contacts.Value |> List.find (fun c -> c.Id = contact.Id)
            match previousContact.Picture, contact.Picture with
            | prevVal, currVal when prevVal = currVal && prevVal <> null -> releaseImageSource previousContact.Picture
            | _ -> ()

            let newContacts = model.Contacts.Value |> List.map (fun c -> if c.Id = contact.Id then contact else c)
            { model with Contacts = Some newContacts }, Cmd.ofAsyncMsg (loadPinsAsync newContacts), ExternalMsg.NoOp
        | ContactDeleted contact ->
            releaseImageSource contact.Picture
            let newContacts = model.Contacts.Value |> List.filter (fun c -> c <> contact)
            { model with Contacts = Some newContacts }, Cmd.ofAsyncMsg (loadPinsAsync newContacts), ExternalMsg.NoOp

    let mkListView contactsLength (groupedContacts: (string * Contact list) list) itemTapped =
        View.ListViewGrouped_XF31(
            verticalOptions=LayoutOptions.FillAndExpand,
            rowHeight=60,
            selectionMode=ListViewSelectionMode.None,
            showJumpList=(contactsLength > 10),
            itemTapped=itemTapped,
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

    let view model dispatch =
        let title = "ElmishContacts"
        let toolbarItems = 
            dependsOn () (fun model () ->
                [
                    mkToolbarButton "About" (fun() -> dispatch AboutTapped)
                    mkToolbarButton "Add" (fun() -> dispatch AddNewContactTapped)
                ]
            )

        match model.Contacts with
        | None ->
            dependsOn () (fun model () ->
                View.ContentPage(
                    title=title,
                    toolbarItems=toolbarItems,
                    content=View.StackLayout(
                        children=[ mkCentralLabel "Loading..." ]
                    )
                )
            )

        | Some [] ->
            dependsOn () (fun model () ->
                View.ContentPage(
                    title=title,
                    toolbarItems=toolbarItems,
                    content=View.StackLayout(
                        children=[ mkCentralLabel "No contact" ]
                    )
                )
            )

        | Some contacts ->
            let contactsTab =
                dependsOn contacts (fun contacts mContacts ->
                    let groupedContacts = groupContacts mContacts
                    View.ContentPage(
                        title="All",
                        toolbarItems=toolbarItems,
                        content=View.StackLayout(
                            children=[ mkListView mContacts.Length groupedContacts (findContactIn groupedContacts >> ContactSelected >> dispatch) ]
                        )
                    )
                )

            let favoriteTab =
                let favoriteContacts = contacts |> List.filter (fun c -> c.IsFavorite)

                dependsOn favoriteContacts (fun contacts mContacts ->
                    let groupedContacts = groupContacts mContacts
                    View.ContentPage(
                        title="Favorites",
                        toolbarItems=toolbarItems,
                        content=View.StackLayout(
                            children=[
                                yield
                                    match favoriteContacts with
                                    | [] -> mkCentralLabel "No favorite"
                                    | _ -> mkListView favoriteContacts.Length groupedContacts (findContactIn groupedContacts >> ContactSelected >> dispatch)
                            ]
                        )
                    )
                )

            let mapTab =
                dependsOn model.Pins (fun model pins ->
                    let paris = Position(48.8566, 2.3522)

                    View.ContentPage(
                        title="Map",
                        content=
                            match pins with
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

            dependsOn model (fun model _ ->
                View.TabbedPage(
                    title=title,
                    children=[ contactsTab; favoriteTab; mapTab ]
                )
            )