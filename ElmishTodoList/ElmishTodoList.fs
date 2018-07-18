namespace ElmishTodoList

open System.Diagnostics
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open SQLite

module Style =
    let mkFormLabel text =
        View.Label(text=text, margin=new Thickness(20., 40., 20., 20.))

    let mkFormEntry text textChanged =
        View.Entry(text=text, textChanged=textChanged, margin=new Thickness(20., 0., 20., 0.))

    let mkFormSwitch isToggled toggled =
        View.Switch(isToggled=isToggled, toggled=toggled, margin=new Thickness(20., 0., 20., 0.))

    let mkDestroyButton text command =
        View.Button(text=text, command=command, backgroundColor=Color.Red, textColor=Color.White, margin=new Thickness(20., 40., 20., 20.))

    let mkCellView name isFavorite =
        View.StackLayout(
            orientation=StackOrientation.Horizontal,
            children=[
                View.Label(text=name, horizontalOptions=LayoutOptions.StartAndExpand, verticalTextAlignment=TextAlignment.Center, margin=new Thickness(20., 0.))
                View.Image(source="star.png", isVisible=isFavorite, verticalOptions=LayoutOptions.Center, margin=new Thickness(0., 0., 20., 0.), heightRequest=25., widthRequest=25.)
            ]
        )

module Models =
    type Contact =
        {
            Id: int
            Name: string
            IsFavorite: bool
        }

module Data =
    open Models

    type ContactObject() =
        [<PrimaryKey>][<AutoIncrement>]
        member val Id = 0 with get, set
        member val Name = "" with get, set
        member val IsFavorite = false with get, set

    let convertToObject (item: Contact) =
        let obj = ContactObject()
        obj.Id <- item.Id
        obj.Name <- item.Name
        obj.IsFavorite <- item.IsFavorite
        obj

    let convertToModel (obj: ContactObject) : Contact =
        {
            Id = obj.Id
            Name = obj.Name
            IsFavorite = obj.IsFavorite
        }

    let connect getDbPathAsync = async {
        let! dbPath = getDbPathAsync()
        let db = new SQLiteAsyncConnection(dbPath)
        do! db.CreateTableAsync<ContactObject>() |> Async.AwaitTask |> Async.Ignore
        return db
    }

    let loadAllContacts dbPath = async {
        let! database = connect dbPath
        let! objs = database.Table<ContactObject>().ToListAsync() |> Async.AwaitTask
        return objs |> Seq.toList |> List.map convertToModel
    }

module App =
    open Data
    open Style
    open Models

    type Model = 
        {
            Contacts: Contact list option
            SelectedContact: Contact option
            Name: string
            IsFavorite: bool
        }

    type Msg = | ContactsLoaded of Contact list | Select of Contact
               | UpdateName of string | UpdateIsFavorite of bool
               | SaveContact of Contact * name: string * isFavorite: bool | DeleteContact of Contact

    let initModel = 
        {
            Contacts = None;
            SelectedContact = None;
            Name = ""
            IsFavorite = false
        }

    let loadAsync database = async {
        let! contacts = loadAllContacts database
        return ContactsLoaded contacts
    }

    let init getDbPathAsync () = initModel, Cmd.ofAsyncMsg (loadAsync getDbPathAsync)

    let update getDbPathAsync msg model =
        match msg with
        | ContactsLoaded contacts -> { model with Contacts = Some contacts }, Cmd.none
        | Select contact -> { model with SelectedContact = Some contact; Name = contact.Name; IsFavorite = contact.IsFavorite }, Cmd.none
        | UpdateName name -> { model with Name = name }, Cmd.none
        | UpdateIsFavorite isFavorite -> { model with IsFavorite = isFavorite }, Cmd.none
        | SaveContact (contact, name, isFavorite) ->
            let newContact = { contact with Name = name; IsFavorite = isFavorite }
            let newContacts = model.Contacts.Value |> List.map (fun c -> if c = model.SelectedContact.Value then newContact else c)
            { model with Contacts = Some newContacts; SelectedContact = None; Name = ""; IsFavorite = false }, Cmd.none
        | DeleteContact contact ->
            let newContacts = model.Contacts.Value |> List.filter (fun c -> c <> contact)
            { model with Contacts = Some newContacts; SelectedContact = None; Name = ""; IsFavorite = false }, Cmd.none

    let view getDbPathAsync (model: Model) dispatch =
        let mainPage =
            View.ContentPage(
                title="My Contacts",
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
            View.ContentPage(
                title="Item",
                toolbarItems=[
                    View.ToolbarItem(order=ToolbarItemOrder.Primary, text="Save", command=(fun() -> (model.SelectedContact.Value, model.Name, model.IsFavorite) |> SaveContact |> dispatch))
                ],
                content=View.StackLayout(
                    children=[
                        mkFormLabel "Name"
                        mkFormEntry model.Name (fun e -> e.NewTextValue |> UpdateName |> dispatch)
                        mkFormLabel "Is Favorite"
                        mkFormSwitch model.IsFavorite (fun e -> e.Value |> UpdateIsFavorite |> dispatch)
                        mkDestroyButton "Delete" (fun () -> model.SelectedContact.Value |> DeleteContact |> dispatch)
                    ]
                )
            )

        View.NavigationPage(
            pages=
                match model.SelectedContact with
                | None -> [ mainPage ]
                | Some _ -> [ mainPage; itemPage ]
        )

type App (getDbPathAsync: unit -> Async<string>) as app = 
    inherit Application ()

    let init = App.init getDbPathAsync
    let update = App.update getDbPathAsync
    let view = App.view getDbPathAsync

    let runner = 
        Program.mkProgram init update view
        |> Program.runWithDynamicView app
