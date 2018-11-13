namespace ElmishContacts.UITests

open NUnit.Framework
open Xamarin.UITest
open Common
open Pages

type Tests() =
    [<TestCase (Platform.Android)>]
    [<TestCase (Platform.iOS)>]
    member this.AddAndDeleteContact (platform: Platform) =
        AppInitializer.startApp platform
        |> UseCases.waitForAppLoaded
        |> screenshot "First screen"
        |> UseCases.goToAboutPage
        |> screenshot "About page"
        |> goBack
        |> MainPage.waitForPage
        |> UseCases.addUser
            { FirstName = "Dr Philip"
              LastName = "Neverbetter"
              IsFavorite = true
              Email = Some "dr.philip.neverbetter@fabulous.com"
              Phone = Some "123-456789"
              Address = Some "1542 Orange Street\nLegoville" }
        |> screenshot "Contact added"
        |> UseCases.deleteUser { FullName = "Dr Philip Neverbetter" }
        |> screenshot "Contact deleted"
        |> ignore

    member this.FavoriteContacts (platform: Platform) =
        AppInitializer.startApp platform
        |> UseCases.waitForAppLoaded
        |> screenshot "First screen"
        |> UseCases.addUser
            { FirstName = "John"
              LastName = "Doe"
              IsFavorite = false
              Email = Some "john.doe@fabulous.com"
              Phone = Some "123-456789"
              Address = Some "1542 Orange Street\nLegoville" }
        |> UseCases.addUser
            { FirstName = "Jane"
              LastName = "Doe"
              IsFavorite = true
              Email = Some "jane.doe@fabulous.com"
              Phone = Some "123-456789"
              Address = Some "1542 Orange Street\nLegoville" }
        |> UseCases.addUser
            { FirstName = "Dr Philip"
              LastName = "Neverbetter"
              IsFavorite = true
              Email = Some "dr.philip.neverbetter@fabulous.com"
              Phone = Some "123-456789"
              Address = Some "1542 Orange Street\nLegoville" }
        |> UseCases.goToDetail { FullName = "Jane Doe" }
        |> screenshot "Favorite contact"
        |> goBack
        |> MainPage.waitForPage
        |> UseCases.goToDetail { FullName = "John Doe" }
        |> screenshot "Normal contact"
        |> goBack
        |> MainPage.waitForPage
        |> UseCases.switchToFavoriteTab
        |> screenshot "Favorite tab"