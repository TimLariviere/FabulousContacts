namespace FabulousContacts

open Fabulous
open Fabulous.XamarinForms
open FabulousContacts.Models
open FabulousContacts.Resources
open Xamarin.Forms

module App =
    type Msg =
        | AllTabMsg of ContactsListPage.Msg
        | FavoritesTabMsg of ContactsListPage.Msg
        | MapTabMsg of MapPage.Msg
    
    type CmdMsg = TestCmdMsg

    type Model = 
        { AllTabModel: ContactsListPage.Model
          FavoritesTabModel: ContactsListPage.Model
          MapTabModel: MapPage.Model }
        
    let mapToCmd cmdMsg =
        Cmd.none

    let init dbPath () = 
        { AllTabModel = ContactsListPage.init ()
          FavoritesTabModel = ContactsListPage.init ()
          MapTabModel = MapPage.init () }, []

    let update dbPath msg model =
        model, []

    let view (model: Model) dispatch =
        View.Shell([
            View.FlyoutItem(
                title = "Contacts",
                items = [
                    View.Tab(
                        title = "All",
                        icon = Path "alltab.png",
                        items = [
                            View.ShellContent(
                                ContactsListPage.view "All" model.AllTabModel (AllTabMsg >> dispatch)
                            )
                        ]
                    )
                    View.Tab(
                        title = "Favorites",
                        icon = Path "favoritetab.png",
                        items = [
                            View.ShellContent(
                                ContactsListPage.view "Favorites" model.FavoritesTabModel (FavoritesTabMsg >> dispatch)
                            )
                        ] 
                    )
                    View.Tab(
                        title = "Map",
                        icon = Path "maptab.png",
                        items = [
                            View.ShellContent(
                                MapPage.view model.MapTabModel (MapTabMsg >> dispatch)
                            )
                        ]
                    )
                ]
            )
            
            View.ShellContent(
                title = "About",
                content = AboutPage.view ()
            )
        ])

type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view
    let mapToCmd = App.mapToCmd
    
    let runner = 
        Program.mkProgramWithCmdMsg init update view mapToCmd
        |> Program.withConsoleTrace
        |> XamarinFormsProgram.run app