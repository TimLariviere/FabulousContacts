namespace ElmishContacts.UITests

open Xamarin.UITest
open Pages

module UseCases =
    type NewUser =
        { FirstName: string
          LastName: string
          IsFavorite: bool
          Email: string option
          Phone: string option
          Address: string option }

    type SelectUser =
        { FullName: string }

    let applyToggle fn data app =
        match data with
        | true -> fn app
        | false -> app

    let applySet fn data app =
        match data with
        | Some x -> fn x app
        | None -> app

    let waitForAppLoaded (app: IApp) =
        app
        |> MainPage.waitForPage

    let goToAboutPage (app: IApp) =
        app
        |> MainPage.goToAboutPage
        |> AboutPage.waitForPage

    let addUser (user: NewUser) (app: IApp) =
        app
        |> MainPage.waitForPage
        |> MainPage.addNewContact
        |> EditPage.waitForPage
        |> Common.screenshot "Edit page"
        |> EditPage.setFirstName user.FirstName
        |> EditPage.setLastName user.LastName
        |> EditPage.markIsFavorite user.IsFavorite
        |> applySet EditPage.setEmail user.Email
        |> applySet EditPage.setPhone user.Phone
        |> applySet EditPage.setAddress user.Address
        |> Common.screenshot "Edit page filled"
        |> EditPage.saveContact
        |> MainPage.waitForPage

    let deleteUser (user: SelectUser) (app: IApp) =
        app
        |> MainPage.selectContact user.FullName
        |> DetailPage.waitForPage
        |> Common.screenshot "Detail page"
        |> DetailPage.editContact
        |> EditPage.waitForPage
        |> EditPage.deleteContact
        |> Common.confirm
        |> MainPage.waitForPage

    let goToDetail (user: SelectUser) (app: IApp) =
        app
        |> MainPage.selectContact user.FullName
        |> DetailPage.waitForPage

    let switchToFavoriteTab (app: IApp) =
        app