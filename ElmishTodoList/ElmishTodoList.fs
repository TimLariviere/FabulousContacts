namespace ElmishTodoList

open System.Diagnostics
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module App = 
    type Contact =
        {
            Name: string
            IsFavorite: bool
        }

    type Model = 
        {
            Contacts: Contact list
            SelectedContact: Contact option
            Name: string
            IsFavorite: bool
        }

    type Msg = Select of Contact | SaveContact | ChangeName of string | ChangeIsFavorite of bool

    let initModel = 
        {
            Contacts =
                [
                    { Name = "James"; IsFavorite = true }
                    { Name = "Jim"; IsFavorite = true }
                    { Name = "John"; IsFavorite = false }
                    { Name = "Frank"; IsFavorite = true }
                ];
            SelectedContact = None;
            Name = ""
            IsFavorite = false
        }

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | Select contact -> { model with SelectedContact = Some contact; Name = contact.Name; IsFavorite = contact.IsFavorite }, Cmd.none
        | ChangeName name -> { model with Name = name }, Cmd.none
        | ChangeIsFavorite isFavorite -> { model with IsFavorite = isFavorite }, Cmd.none
        | SaveContact ->
            let newContact = { model.SelectedContact.Value with Name = model.Name; IsFavorite = model.IsFavorite }
            let newContacts = model.Contacts |> List.map (fun c -> if c = model.SelectedContact.Value then newContact else c)
            { model with Contacts = newContacts; SelectedContact = None; Name = ""; IsFavorite = false }, Cmd.none

    let cellView name isFavorite =
        View.StackLayout(
            orientation=StackOrientation.Horizontal,
            children=[
                View.Label(text=name, horizontalOptions=LayoutOptions.StartAndExpand, verticalTextAlignment=TextAlignment.Center, margin=new Thickness(20., 0.))
                View.Image(source="star.png", isVisible=isFavorite, verticalOptions=LayoutOptions.Center, margin=new Thickness(0., 0., 20., 0.), heightRequest=25., widthRequest=25.)
            ]
        )

    let view (model: Model) dispatch =
        let mainPage =
            View.ContentPage(
                title="My Contacts",
                content=View.StackLayout(
                    children=
                        [
                            View.ListView(
                                verticalOptions=LayoutOptions.FillAndExpand,
                                itemTapped=(fun i -> model.Contacts.[i] |> Select |> dispatch),
                                items=
                                    [
                                        for contact in model.Contacts do
                                            yield cellView contact.Name contact.IsFavorite
                                    ]
                            )
                        ]
                ) 
            )

        let itemPage =
            View.ContentPage(
                title="Item",
                toolbarItems=[
                    View.ToolbarItem(order=ToolbarItemOrder.Primary, text="Save", command=(fun() -> SaveContact |> dispatch))
                ],
                content=View.StackLayout(
                    children=[
                        View.Label(text="Name")
                        View.Entry(text=model.Name, textChanged=(fun e -> e.NewTextValue |> ChangeName |> dispatch))
                        View.Label(text="Is Favorite")
                        View.Switch(isToggled=model.IsFavorite, toggled=(fun e -> e.Value |> ChangeIsFavorite |> dispatch))
                    ]
                )
            )

        View.NavigationPage(
            pages=
                match model.SelectedContact with
                | None -> [ mainPage ]
                | Some _ -> [ mainPage; itemPage ]
        )

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
        |> Program.runWithDynamicView app
