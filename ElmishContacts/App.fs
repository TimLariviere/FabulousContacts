namespace ElmishContacts

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open ElmishContacts.Models
open System

module App =
    type Model = 
        {
            MainPageModel: MainPage.Model
            ItemPageModel: ItemPage.Model option
            MapPageModel: MapPage.Model option
        }

    type Msg = | MainPageMsg of MainPage.Msg
               | ItemPageMsg of ItemPage.Msg
               | MapPageMsg of MapPage.Msg
               | GoToItem of Contact option
               | GoToMap
               | UpdateMainWithContactAdded of Contact
               | UpdateMainWithContactUpdated of Contact
               | UpdateMainWithContactDeleted of Contact
               | NavigationPopped

    let init dbPath () = 
        let mainModel, mainMsg = MainPage.init dbPath ()
        {
            MainPageModel = mainModel
            ItemPageModel = None
            MapPageModel = None
        }, Cmd.batch [ (Cmd.map MainPageMsg mainMsg) ]

    let update dbPath msg model =
        match msg with
        | MainPageMsg msg ->
            let m, cmd, externalMsg = MainPage.update msg model.MainPageModel

            let cmd2 =
                match externalMsg with
                | MainPage.ExternalMsg.NoOp ->
                    Cmd.none
                | MainPage.ExternalMsg.Select contact ->
                    Cmd.ofMsg (GoToItem (Some contact))
                | MainPage.ExternalMsg.AddNewContact ->
                    Cmd.ofMsg (GoToItem None)
                | MainPage.ExternalMsg.ShowMap ->
                    Cmd.ofMsg GoToMap

            { model with MainPageModel = m }, Cmd.batch [ (Cmd.map MainPageMsg cmd); cmd2 ]

        | ItemPageMsg msg ->
            let m, cmd, externalMsg = ItemPage.update dbPath msg model.ItemPageModel.Value

            let cmd2 =
                match externalMsg with
                | ItemPage.ExternalMsg.NoOp ->
                    Cmd.none
                | ItemPage.ExternalMsg.GoBackAfterContactAdded contact ->
                    Cmd.ofMsg (UpdateMainWithContactAdded contact)
                | ItemPage.ExternalMsg.GoBackAfterContactUpdated contact ->
                    Cmd.ofMsg (UpdateMainWithContactUpdated contact)
                | ItemPage.ExternalMsg.GoBackAfterContactDeleted contact ->
                    Cmd.ofMsg (UpdateMainWithContactDeleted contact)

            { model with ItemPageModel = Some m }, Cmd.batch [ (Cmd.map ItemPageMsg cmd); cmd2 ]

        | MapPageMsg msg ->
            let m, cmd, externalMsg = MapPage.update msg model.MapPageModel.Value

            let cmd2 =
                match externalMsg with
                | MapPage.ExternalMsg.NoOp ->
                    Cmd.none

            { model with MapPageModel = Some m }, Cmd.batch [ (Cmd.map MainPageMsg cmd); cmd2 ]

        | NavigationPopped ->
            match (model.ItemPageModel, model.MapPageModel) with
            | None, None -> model, Cmd.none
            | Some _, None -> { model with ItemPageModel = None }, Cmd.none
            | None, Some _ -> { model with MapPageModel = None }, Cmd.none
            | Some _, Some _ -> { model with ItemPageModel = None }, Cmd.none

        | GoToItem contact ->
            let m, cmd = ItemPage.init contact
            { model with ItemPageModel = Some m }, (Cmd.map ItemPageMsg cmd)

        | GoToMap ->
            let m, cmd = MapPage.init dbPath
            { model with MapPageModel = Some m }, (Cmd.map MapPageMsg cmd)

        | UpdateMainWithContactAdded contact ->
            { model with ItemPageModel = None }, Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactAdded contact))

        | UpdateMainWithContactUpdated contact ->
            { model with ItemPageModel = None }, Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactUpdated contact))

        | UpdateMainWithContactDeleted contact ->
            { model with ItemPageModel = None }, Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactDeleted contact))


    let view (model: Model) dispatch =
        let mainPage = MainPage.view model.MainPageModel (MainPageMsg >> dispatch)

        let itemPage =
            match model.ItemPageModel with
            | None -> None
            | Some iModel -> Some (ItemPage.view iModel (ItemPageMsg >> dispatch))

        let mapPage =
            match model.MapPageModel with
            | None -> None
            | Some mModel -> Some (MapPage.view mModel (MapPageMsg >> dispatch))
            
        View.NavigationPage(
            barTextColor=Style.accentTextColor,
            barBackgroundColor=Style.accentColor,
            popped=(fun _ -> NavigationPopped |> dispatch),
            pages=
                match (itemPage, mapPage) with
                | (None, None) -> [ mainPage ]
                | (None, Some map) -> [ mainPage; map ]
                | (Some item, None) -> [ mainPage; item ]
                | (Some item, Some map) -> [ mainPage; map; item ]
        )

type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view

    let runner = 
        Program.mkProgram init update view
        |> Program.runWithDynamicView app



#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            Console.WriteLine ("Error while restoring model found in app.Properties. " + ex.ToString())

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif