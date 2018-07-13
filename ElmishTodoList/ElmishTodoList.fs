namespace ElmishTodoList

open System.Diagnostics
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module App = 
    type Contact =
        {
            Name: string
        }

    type Model = 
        {
            Contacts: Contact list
        }

    type Msg = NoMsg

    let initModel = 
        {
            Contacts =
                [
                    { Name = "John" }
                    { Name = "James Montemagno" }
                    { Name = "Jim Bennett" }
                    { Name = "Frank A. Krueger" }
                ]
        }

    let init () = initModel, Cmd.none


    let update (msg: Model) model = model, Cmd.none

    let view (model: Model) dispatch =
        View.ContentPage(
            content=View.StackLayout(
                children=
                    [
                        View.ListView(
                            verticalOptions=LayoutOptions.FillAndExpand,
                            items=
                                [
                                    for contact in model.Contacts do
                                        yield View.Label(text=contact.Name)
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
