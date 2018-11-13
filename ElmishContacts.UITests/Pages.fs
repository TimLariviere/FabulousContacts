namespace ElmishContacts.UITests

open Xamarin.UITest
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

        let waitForPage app = waitFor about app
        let goToAboutPage app = tap about app
        let addNewContact app = tap add app
        let selectContact name = tap (fun a -> a.Marked name)

    module AboutPage =
        let icon = marked "Icon"

        let waitForPage app = waitFor icon app

    module EditPage =
        let save = marked "Save"
        let firstName = marked "FirstName"
        let lastName = marked "LastName"
        let markAsFavorite = marked "MarkAsFavorite"
        let email = marked "Email"
        let phone = marked "Phone"
        let address = marked "Address"
        let delete = marked "Delete"

        let waitForPage app = waitFor save app
        let setFirstName value = enterText firstName value
        let setLastName value = enterText lastName value
        let setEmail value = enterText email value
        let setPhone value = enterText phone value
        let setAddress value = enterText address value
        let markIsFavorite app = tap markAsFavorite app
        let saveContact app = tap save app
        let deleteContact app = tap delete app

    module DetailPage =
        let picture = marked "Picture"
        let edit = marked "Edit"

        let waitForPage app = waitFor picture app
        let editContact app = tap edit app