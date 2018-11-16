namespace ElmishContacts

open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open ElmishContacts.Models

module App =
    type Msg =
        | MainPageMsg of MainPage.Msg
        | DetailPageMsg of DetailPage.Msg
        | EditPageMsg of EditPage.Msg
        | GoToDetail of Contact
        | GoToEdit of Contact option
        | GoToAbout
        | UpdateWhenContactAdded of Contact
        | UpdateWhenContactUpdated of Contact
        | UpdateWhenContactDeleted of Contact
        | NavigationPopped

    type Model = 
        { MainPageModel: MainPage.Model
          DetailPageModel: DetailPage.Model option
          EditPageModel: EditPage.Model option
          AboutPageModel: bool option

          // Workaround Cmd limitation -- Can not pop a page in page stack and send Cmd at the same time
          // Otherwise it would pop pages 2 times in NavigationPage
          WorkaroundNavPageBug: bool
          WorkaroundNavPageBugPendingCmd: Cmd<Msg> }

    let init dbPath () = 
        let mainModel, mainMsg = MainPage.init dbPath ()

        { MainPageModel = mainModel
          DetailPageModel = None
          EditPageModel = None
          AboutPageModel = None
          WorkaroundNavPageBug = false
          WorkaroundNavPageBugPendingCmd = Cmd.none
        }, (Cmd.map MainPageMsg mainMsg)

    let update dbPath msg model =
        match msg with
        | MainPageMsg msg ->
            let m, cmd, externalMsg = MainPage.update msg model.MainPageModel

            let cmd2 =
                match externalMsg with
                | MainPage.ExternalMsg.NoOp ->
                    Cmd.none
                | MainPage.ExternalMsg.NavigateToAbout ->
                    Cmd.ofMsg GoToAbout
                | MainPage.ExternalMsg.NavigateToNewContact ->
                    Cmd.ofMsg (GoToEdit None)
                | MainPage.ExternalMsg.NavigateToDetail contact ->
                    Cmd.ofMsg (GoToDetail contact)

            { model with MainPageModel = m }, Cmd.batch [ (Cmd.map MainPageMsg cmd); cmd2 ]

        | DetailPageMsg msg ->
            let m, cmd, externalMsg = DetailPage.update msg model.DetailPageModel.Value

            let cmd2 =
                match externalMsg with
                | DetailPage.ExternalMsg.NoOp ->
                    Cmd.none
                | DetailPage.ExternalMsg.EditContact contact ->
                    Cmd.ofMsg (GoToEdit (Some contact))

            { model with DetailPageModel = Some m }, Cmd.batch [ (Cmd.map DetailPageMsg cmd); cmd2 ]

        | EditPageMsg msg ->
            let m, cmd, externalMsg = EditPage.update dbPath msg model.EditPageModel.Value

            let cmd2 =
                match externalMsg with
                | EditPage.ExternalMsg.NoOp ->
                    Cmd.none
                | EditPage.ExternalMsg.GoBackAfterContactAdded contact ->
                    Cmd.ofMsg (UpdateWhenContactAdded contact)
                | EditPage.ExternalMsg.GoBackAfterContactUpdated contact ->
                    Cmd.ofMsg (UpdateWhenContactUpdated contact)
                | EditPage.ExternalMsg.GoBackAfterContactDeleted contact ->
                    Cmd.ofMsg (UpdateWhenContactDeleted contact)

            { model with EditPageModel = Some m }, Cmd.batch [ (Cmd.map EditPageMsg cmd); cmd2 ]

        | NavigationPopped ->
            match model.WorkaroundNavPageBug, (model.AboutPageModel, model.DetailPageModel, model.EditPageModel) with
            | true, _ -> { model with WorkaroundNavPageBug = false; WorkaroundNavPageBugPendingCmd = Cmd.none }, model.WorkaroundNavPageBugPendingCmd // Do not pop pages if already done manually
            | false, (None, None, None) -> model, Cmd.none
            | false, (Some _, None, None) -> { model with AboutPageModel = None }, Cmd.none
            | false, (_, Some _, None) -> { model with DetailPageModel = None }, Cmd.none
            | false, (_, _, Some _) -> { model with EditPageModel = None }, Cmd.none

        | GoToAbout ->
            { model with AboutPageModel = Some true }, Cmd.none

        | GoToDetail contact ->
            let m, cmd = DetailPage.init contact
            { model with DetailPageModel = Some m }, (Cmd.map DetailPageMsg cmd)

        | GoToEdit contact ->
            let m, cmd = EditPage.init contact
            { model with EditPageModel = Some m }, (Cmd.map EditPageMsg cmd)

        | UpdateWhenContactAdded contact ->
            let mainMsg = Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactAdded contact))
            { model with EditPageModel = None }, mainMsg

        | UpdateWhenContactUpdated contact ->
            let pendingCmds =
                Cmd.batch
                    [
                        Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactUpdated contact))
                        Cmd.ofMsg (DetailPageMsg (DetailPage.Msg.ContactUpdated contact))
                    ]
            { model with EditPageModel = None; WorkaroundNavPageBug = true; WorkaroundNavPageBugPendingCmd = pendingCmds }, Cmd.none

        | UpdateWhenContactDeleted contact ->
            let mainMsg = Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactDeleted contact))
            { model with DetailPageModel = None; EditPageModel = None }, mainMsg


    let view (model: Model) dispatch =
        // Workaround iOS bug: https://github.com/xamarin/Xamarin.Forms/issues/3509
        let dispatchNavPopped =
            let mutable lastRemovedPageIdentifier: int = -1
            let apply dispatch (e: Xamarin.Forms.NavigationEventArgs) =
                let removedPageIdentifier = e.Page.GetHashCode()
                match lastRemovedPageIdentifier = removedPageIdentifier with
                | false ->
                    lastRemovedPageIdentifier <- removedPageIdentifier
                    dispatch NavigationPopped
                | true ->
                    ()
            apply


        let mainPage = MainPage.view model.MainPageModel (MainPageMsg >> dispatch)

        let detailPage =
            match model.DetailPageModel with
            | None -> None
            | Some dModel -> Some (DetailPage.view dModel (DetailPageMsg >> dispatch))

        let editPage =
            match model.EditPageModel with
            | None -> None
            | Some eModel -> Some (EditPage.view eModel (EditPageMsg >> dispatch))

        let aboutPage = 
            match model.AboutPageModel with
            | None -> None
            | Some _ -> Some (AboutPage.view ())

        View.NavigationPage(
            barTextColor=Style.accentTextColor,
            barBackgroundColor=Style.accentColor,
            popped=(dispatchNavPopped dispatch),
            pages=
                match aboutPage, detailPage, editPage with
                | None, None, None -> [ mainPage ]
                | Some about, None, None -> [ mainPage; about ]
                | _, Some detail, None -> [ mainPage; detail ]
                | _, Some detail, Some edit -> [ mainPage; detail; edit ]
                | _, None, Some edit -> [ mainPage; edit ]
        )

type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view

    let runner = 
        Program.mkProgram init update view
        //|> Program.withConsoleTrace
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