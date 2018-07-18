namespace ElmishTodoList

open System.Diagnostics
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

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

module Data = 
    type Contact =
        {
            Name: string
            IsFavorite: bool
        }

module App =
    open Data
    open Style

    type Model = 
        {
            Contacts: Contact list
            SelectedContact: Contact option
            Name: string
            IsFavorite: bool
        }

    type Msg = | Select of Contact
               | UpdateName of string | UpdateIsFavorite of bool
               | SaveContact of Contact * name: string * isFavorite: bool | DeleteContact of Contact

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
        | UpdateName name -> { model with Name = name }, Cmd.none
        | UpdateIsFavorite isFavorite -> { model with IsFavorite = isFavorite }, Cmd.none
        | SaveContact (contact, name, isFavorite) ->
            let newContact = { contact with Name = name; IsFavorite = isFavorite }
            let newContacts = model.Contacts |> List.map (fun c -> if c = model.SelectedContact.Value then newContact else c)
            { model with Contacts = newContacts; SelectedContact = None; Name = ""; IsFavorite = false }, Cmd.none
        | DeleteContact contact ->
            let newContacts = model.Contacts |> List.filter (fun c -> c <> contact)
            { model with Contacts = newContacts; SelectedContact = None; Name = ""; IsFavorite = false }, Cmd.none

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

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
        |> Program.runWithDynamicView app
