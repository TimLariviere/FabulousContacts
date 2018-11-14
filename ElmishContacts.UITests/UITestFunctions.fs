namespace ElmishContacts.UITests

open Xamarin.UITest
open Xamarin.UITest.Queries
open System.Threading

module UITestFunctions =
    let marked id = (fun (a: AppQuery) -> a.Marked id)

    let sleepFor (ms: int) (app: IApp) =
        Thread.Sleep ms
        app

    let enterText (query: AppQuery -> AppQuery) value (app: IApp) =
        app.Tap query
        app.EnterText value
        app.DismissKeyboard()
        sleepFor 500 app

    let waitFor (query: AppQuery -> AppQuery) (app: IApp) =
        app.WaitForElement query |> ignore
        app

    let tap (query: AppQuery -> AppQuery) (app: IApp) =
        app.Tap query
        app