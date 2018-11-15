namespace ElmishContacts.UITests

open Xamarin.UITest
open Xamarin.UITest.Android
open Xamarin.UITest.iOS
open System.Linq
open UITestFunctions

module Common =
    let repl (app: IApp) =
        app.Repl()

    let screenshot title (app: IApp) =
        app.Screenshot title |> ignore
        app

    let goBack (app: IApp) =
        app.Back()
        app

    let confirm (app: IApp) =
        app.Tap (fun a -> a.Marked "Yes")
        app

module Pages =
    module MainPage =
        let about = marked "About"
        let add = marked "+"
        let favoritesTab = marked "Favorites"

        let waitForPage app = waitFor about app
        let goToAboutPage app = tap about app
        let addNewContact app = tap add app
        let selectContact name = tap (marked name)
        let switchToFavoritesTab app = tap favoritesTab app

    module AboutPage =
        let icon = marked "Icon"

        let waitForPage app = waitFor icon app

    module EditPage =
        let save = marked "Save"
        let firstName = marked "FirstName"
        let lastName = marked "LastName"
        let markAsFavorite = marked "MarkAsFavorite"
        let email = marked "EmailField"
        let phone = marked "PhoneField"
        let address = marked "AddressField"
        let delete = marked "Delete"
        let scrollView = marked "ScrollView"

        let waitForPage app = waitFor save app
        let setFirstName value = scrollAndEnterText firstName scrollView value
        let setLastName value = scrollAndEnterText lastName scrollView value
        let setEmail value = scrollAndEnterText email scrollView value
        let setPhone value = scrollAndEnterText phone scrollView value
        let setAddress value = scrollAndEnterText address scrollView value
        let saveContact app = scrollAndTap save scrollView app
        let deleteContact app = scrollAndTap delete scrollView app

        let markIsFavorite value (app: IApp) =
            let isOn =
                match app with
                | :? iOSApp -> app.Query(fun a -> let switch = a |> markAsFavorite.Query in switch.Invoke("isOn").Value<int>()).First() = 1
                | :? AndroidApp -> app.Query(fun a -> let switch = a |> markAsFavorite.Query in switch.Invoke("isChecked").Value<bool>()).First()
                | _ -> false

            match isOn, value with
            | true, false | false, true -> tap markAsFavorite app
            | _ -> app


    module DetailPage =
        let picture = marked "Picture"
        let edit = marked "Edit"

        let waitForPage app = waitFor picture app
        let editContact app = tap edit app