namespace ElmishContacts

open Fabulous.Core
open Xamarin.Forms
open System
open AppCenter

module Tracing =
    let hasValue = (not << String.IsNullOrEmpty) >> string

    let rules msg _ =
        match msg with
        | Root.Msg.GoToAbout ->
            Some { EventName = "Page"; AdditionalParameters = [ { Key = "Page"; Value = "About"} ] }
        | Root.Msg.GoToDetail _ ->
            Some { EventName = "Page"; AdditionalParameters = [ { Key = "Page"; Value = "Detail"} ] }
        | Root.Msg.GoToEdit c ->
            match c with
            | Some _ ->
                Some { EventName = "Page"; AdditionalParameters = [ { Key = "Page"; Value = "Edit"} ] }
            | None ->
                Some { EventName = "Page"; AdditionalParameters = [ { Key = "Page"; Value = "Create"} ] }
        | Root.Msg.NavigationPopped ->
            Some { EventName = "BackNavigation"; AdditionalParameters = [ ] }
        | Root.Msg.UpdateWhenContactAdded c ->
            Some
                { EventName = "Contact added"
                  AdditionalParameters = 
                    [ { Key = "Has Email"; Value = hasValue c.Email }
                      { Key = "Has Phone"; Value = hasValue c.Phone }
                      { Key = "Has Address"; Value = hasValue c.Address } ] }
        | Root.Msg.UpdateWhenContactUpdated c ->
            Some
                { EventName = "Contact updated"
                  AdditionalParameters = 
                    [ { Key = "Has Email"; Value = hasValue c.Email }
                      { Key = "Has Phone"; Value = hasValue c.Phone }
                      { Key = "Has Address"; Value = hasValue c.Address } ] }
        | Root.Msg.UpdateWhenContactDeleted _ ->
            Some { EventName = "Contact deleted"; AdditionalParameters = [ ] }
        | Root.Msg.MainPageMsg (MainPage.Msg.TabMapMsg (MapPage.Msg.RetrieveUserPosition)) ->
            Some { EventName = "User Position"; AdditionalParameters = [ { Key = "Event"; Value = "Requested"} ] }
        | Root.Msg.MainPageMsg (MainPage.Msg.TabMapMsg (MapPage.Msg.UserPositionRetrieved _)) ->
            Some { EventName = "User Position"; AdditionalParameters = [ { Key = "Event"; Value = "Retrieved"} ] }
        | _ -> None

module DesignTime =
    let programLiveUpdate = 
        let init = Root.init ""
        let update = Root.update ""
        let view = Root.view
        Program.mkProgram init update view

type App (dbPath) as app = 
    inherit Application ()

    let init = Root.init dbPath
    let update = Root.update dbPath
    let view = Root.view

    do AppCenter.start()

    let runner = 
        Program.mkProgram init update view
#if DEBUG
        |> Program.withConsoleTrace
#else
        |> AppCenter.withAppCenterTrace Tracing.rules
#endif
        |> Program.runWithDynamicView app

    do runner.EnableLiveUpdate()