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
        }

    type Msg = ToggleFavorite of Contact

    let initModel = 
        {
            Contacts =
                [
                    { Name = "James"; IsFavorite = true }
                    { Name = "Jim"; IsFavorite = true }
                    { Name = "John"; IsFavorite = false }
                    { Name = "Frank"; IsFavorite = true }
                ]
        }

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | ToggleFavorite contact ->
            let newContact = { contact with IsFavorite = not contact.IsFavorite }
            let newContacts = model.Contacts |> List.map (fun c -> if c = contact then newContact else c)
            { model with Contacts = newContacts }, Cmd.none

    let cellView name isFavorite =
        View.StackLayout(
            orientation=StackOrientation.Horizontal,
            children=[
                View.Label(text=name, horizontalOptions=LayoutOptions.StartAndExpand, verticalTextAlignment=TextAlignment.Center, margin=new Thickness(20., 0.))
                View.Image(source="star.png", isVisible=isFavorite, verticalOptions=LayoutOptions.Center, margin=new Thickness(0., 0., 20., 0.), heightRequest=25., widthRequest=25.)
            ]
        )

    let view (model: Model) dispatch =
        View.ContentPage(
            content=View.StackLayout(
                children=
                    [
                        View.ListView(
                            verticalOptions=LayoutOptions.FillAndExpand,
                            itemTapped=(fun i -> model.Contacts.[i] |> ToggleFavorite |> dispatch),
                            items=
                                [
                                    for contact in model.Contacts do
                                        yield cellView contact.Name contact.IsFavorite
                                ]
                        )
                    ]
            ) 
        )

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
        |> Program.runWithDynamicView app
