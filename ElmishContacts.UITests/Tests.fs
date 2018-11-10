namespace ElmishContacts.UITests

open NUnit.Framework
open Xamarin.UITest
open Helpers

type Tests() =

  [<TestCase (Platform.Android)>]
  [<TestCase (Platform.iOS)>]
  member this.AddAndDeleteContact (platform: Platform) =
    AppInitializer.startApp platform
    |> MainPage.waitForPage
    |> screenshot "First screen"
    |> MainPage.goToAboutPage
    |> AboutPage.waitForPage
    |> screenshot "About page"
    |> goBack
    |> MainPage.waitForPage
    |> MainPage.addNewContact
    |> EditPage.waitForPage
    |> screenshot "Edit page"
    |> EditPage.setFirstName "Dr Philip"
    |> EditPage.setLastName "Neverbetter"
    |> EditPage.toggleFavorite
    |> EditPage.setEmail "dr.philip.neverbetter@fabulous.com"
    |> EditPage.setPhone "123-456789"
    |> EditPage.setAddress "1542 Orange Street\nLegoville"
    |> screenshot "Edit page filled"
    |> EditPage.saveContact
    |> MainPage.waitForPage
    |> screenshot "Contact added"
    |> MainPage.selectContact "Dr Philip Neverbetter"
    |> DetailPage.waitForPage
    |> screenshot "Detail page"
    |> DetailPage.editContact
    |> EditPage.waitForPage
    |> EditPage.deleteContact
    |> confirm
    |> MainPage.waitForPage
    |> screenshot "Contact deleted"
    |> ignore