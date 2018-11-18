namespace ElmishContacts

open Fabulous.Core
open Xamarin.Forms
open System

module Tracing =
    let hasValue = (not << String.IsNullOrEmpty) >> string

    let rules msg _ =
        match msg with
        | Root.Msg.GoToAbout -> Some ("Navigation", [ ("Page", "About") ])
        | Root.Msg.GoToDetail _ -> Some ("Navigation", [ ("Page", "Detail") ])
        | Root.Msg.GoToEdit c ->
            match c with
            | Some _ -> Some ("Navigation", [ ("Page", "Edit") ])
            | None -> Some ("Navigation", [ ("Page", "Create") ])
        | Root.Msg.NavigationPopped ->
            Some ("Back Navigation", [])
        | Root.Msg.UpdateWhenContactAdded c ->
            Some ("Contact added", [
                ("Has Email", hasValue c.Email)
                ("Has Phone", hasValue c.Phone)
                ("Has Address", hasValue c.Address)
            ])
        | Root.Msg.UpdateWhenContactUpdated c ->
            Some ("Contact updated", [
                ("Has Email", hasValue c.Email)
                ("Has Phone", hasValue c.Phone)
                ("Has Address", hasValue c.Address)
            ])
        | Root.Msg.UpdateWhenContactDeleted _ ->
            Some ("Contact deleted", [])
        | Root.Msg.MainPageMsg (MainPage.Msg.TabMapMsg (MapPage.Msg.RetrieveUserPosition)) ->
            Some ("User Position", [ ( "Event", "Requested") ])
        | Root.Msg.MainPageMsg (MainPage.Msg.TabMapMsg (MapPage.Msg.UserPositionRetrieved _)) ->
            Some ("User Position", [( "Event", "Retrieved") ])
        | _ -> None


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