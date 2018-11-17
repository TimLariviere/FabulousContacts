namespace ElmishContacts.UITests

open Xamarin.UITest
open Xamarin.UITest.Queries
open System.Threading

module UITestFunctions =
    type Marked =
        { Id: string
          Query: AppQuery -> AppQuery }

    let marked id =
      { Id = id
        Query = (fun a -> a.Marked id) }

    let sleepFor (ms: int) (app: IApp) =
        Thread.Sleep ms
        app

    let enterText (control: Marked) value (app: IApp) =
        app.Tap control.Query
        sleepFor 250 app |> ignore
        app.EnterText value
        app.DismissKeyboard()
        sleepFor 250 app

    let waitFor (control: Marked) (app: IApp) =
        app.WaitForElement control.Query |> ignore
        app

    let tap (control: Marked) (app: IApp) =
        app.Tap control.Query
        app

    let scroll (control: Marked) (scrollView: Marked) (app: IApp) =
        app.ScrollTo(control.Id, scrollView.Id, ScrollStrategy.Auto, 1., 1, false, System.Nullable<System.TimeSpan>())
        app

    let scrollAndTap (query: Marked) (scrollQuery: Marked) (app: IApp) =
        app
        |> scroll query scrollQuery
        |> tap query

    let scrollAndEnterText (query: Marked) (scrollQuery: Marked) value (app: IApp) =
        app
        |> scroll query scrollQuery
        |> enterText query value