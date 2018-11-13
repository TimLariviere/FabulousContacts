namespace ElmishContacts.UITests

open Xamarin.UITest
open Xamarin.UITest.Queries

module UITestFunctions =
    let marked id = (fun (a: AppQuery) -> a.Marked id)

    let enterText (query: AppQuery -> AppQuery) value (app: IApp) =
        app.Tap query
        app.EnterText value
        app

    let waitFor (query: AppQuery -> AppQuery) (app: IApp) =
        app.WaitForElement query |> ignore
        app

    let tap (query: AppQuery -> AppQuery) (app: IApp) =
        app.Tap query
        app