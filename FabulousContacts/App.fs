namespace FabulousContacts

open Xamarin.Forms
open Fabulous
open Fabulous.XamarinForms
open FabulousContacts.Models

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

    type Pages =
        { MainPage: ViewElement
          DetailPage: ViewElement option
          EditPage: ViewElement option
          AboutPage: ViewElement option }

    let init dbPath () = 
        let mainModel, mainMsg = MainPage.init dbPath ()
        let initialModel =
            { MainPageModel = mainModel
              DetailPageModel = None
              EditPageModel = None
              AboutPageModel = None
              WorkaroundNavPageBug = false
              WorkaroundNavPageBugPendingCmd = Cmd.none }
        initialModel, (Cmd.map MainPageMsg mainMsg)

    let handleMainExternalMsg externalMsg =
        match externalMsg with
        | MainPage.ExternalMsg.NoOp                     -> Cmd.none
        | MainPage.ExternalMsg.NavigateToAbout          -> Cmd.ofMsg GoToAbout
        | MainPage.ExternalMsg.NavigateToNewContact     -> Cmd.ofMsg (GoToEdit None)
        | MainPage.ExternalMsg.NavigateToDetail contact -> Cmd.ofMsg (GoToDetail contact)

    let handleDetailPageExternalMsg externalMsg =
        match externalMsg with
        | DetailPage.ExternalMsg.NoOp                   -> Cmd.none
        | DetailPage.ExternalMsg.EditContact contact    -> Cmd.ofMsg (GoToEdit (Some contact))

    let handleEditPageExternalMsg externalMsg =
        match externalMsg with
        | EditPage.ExternalMsg.NoOp                              -> Cmd.none
        | EditPage.ExternalMsg.GoBackAfterContactAdded contact   -> Cmd.ofMsg (UpdateWhenContactAdded contact)
        | EditPage.ExternalMsg.GoBackAfterContactUpdated contact -> Cmd.ofMsg (UpdateWhenContactUpdated contact)
        | EditPage.ExternalMsg.GoBackAfterContactDeleted contact -> Cmd.ofMsg (UpdateWhenContactDeleted contact)

    let navigationMapper (model : Model) =
        let aboutModel = model.AboutPageModel
        let detailModel = model.DetailPageModel
        let editModel = model.EditPageModel
        match aboutModel, detailModel, editModel with
        | None, None, None -> model
        | Some _, None, None -> { model with AboutPageModel = None }
        | _, Some _, None -> { model with DetailPageModel = None }
        | _, _, Some _ -> { model with EditPageModel = None }

    let update dbPath msg model =
        match msg with
        | MainPageMsg msg ->
            let m, cmd, externalMsg = MainPage.update msg model.MainPageModel
            let cmd2 = handleMainExternalMsg externalMsg
            let batchCmd = Cmd.batch [ (Cmd.map MainPageMsg cmd); cmd2 ]
            { model with MainPageModel = m }, batchCmd

        | DetailPageMsg msg ->
            let m, cmd, externalMsg = DetailPage.update msg model.DetailPageModel.Value
            let cmd2 = handleDetailPageExternalMsg externalMsg
            let batchCmd = Cmd.batch [ (Cmd.map DetailPageMsg cmd); cmd2 ]
            { model with DetailPageModel = Some m }, batchCmd

        | EditPageMsg msg ->
            let m, cmd, externalMsg = EditPage.update dbPath msg model.EditPageModel.Value
            let cmd2 = handleEditPageExternalMsg externalMsg
            let batchCmd = Cmd.batch [ (Cmd.map EditPageMsg cmd); cmd2 ]
            { model with EditPageModel = Some m }, batchCmd

        | NavigationPopped ->
            match model.WorkaroundNavPageBug with
            | true ->
                // Do not pop pages if already done manually
                let newModel =
                    { model with
                        WorkaroundNavPageBug = false
                        WorkaroundNavPageBugPendingCmd = Cmd.none }
                newModel, model.WorkaroundNavPageBugPendingCmd
            | false ->
                navigationMapper model, Cmd.none

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
                    [ Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactUpdated contact))
                      Cmd.ofMsg (DetailPageMsg (DetailPage.Msg.ContactUpdated contact)) ]
            let m =
                { model with
                    EditPageModel = None
                    WorkaroundNavPageBug = true
                    WorkaroundNavPageBugPendingCmd = pendingCmds }
            m, Cmd.none

        | UpdateWhenContactDeleted contact ->
            let mainMsg = Cmd.ofMsg (MainPageMsg (MainPage.Msg.ContactDeleted contact))
            let m = { model with DetailPageModel = None; EditPageModel = None }
            m, mainMsg

    let getPages allPages =
        let mainPage = allPages.MainPage
        let aboutPage = allPages.AboutPage
        let detailPage = allPages.DetailPage
        let editPage = allPages.EditPage
        
        match aboutPage, detailPage, editPage with
        | None, None, None          -> [ mainPage ]
        | Some about, None, None    -> [ mainPage; about ]
        | _, Some detail, None      -> [ mainPage; detail ]
        | _, Some detail, Some edit -> [ mainPage; detail; edit ]
        | _, None, Some edit        -> [ mainPage; edit ]

    let view (model: Model) dispatch =
        let mainPage = MainPage.view model.MainPageModel (MainPageMsg >> dispatch)

        let detailPage =
            model.DetailPageModel
            |> Option.map (fun dModel -> DetailPage.view dModel (DetailPageMsg >> dispatch))

        let editPage =
            model.EditPageModel
            |> Option.map (fun eModel -> EditPage.view eModel (EditPageMsg >> dispatch))

        let aboutPage =
            model.AboutPageModel
            |> Option.map (fun _ -> AboutPage.view ())

        let allPages =
            { MainPage = mainPage
              DetailPage = detailPage
              EditPage = editPage
              AboutPage = aboutPage }

        View.NavigationPage(
            barTextColor = Style.accentTextColor,
            barBackgroundColor = Style.accentColor,
            popped = (fun _ -> dispatch NavigationPopped),
            pages = getPages allPages
        )

type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view
    
    let runner = 
        Program.mkProgram init update view
        |> Program.withConsoleTrace
        |> XamarinFormsProgram.run app