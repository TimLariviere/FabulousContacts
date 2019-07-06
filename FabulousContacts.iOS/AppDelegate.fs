namespace FabulousContacts.iOS

open System
open UIKit
open Foundation
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open System.IO
open SQLite
open Xamarin
open Plugin.Media

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()

    let getDbPath() =
        let docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        let libFolder = Path.Combine(docFolder, "..", "Library", "Databases")

        if Directory.Exists(libFolder) = false then
            Directory.CreateDirectory(libFolder) |> ignore
        else
            ()

        Path.Combine(libFolder, "Contacts.db3")

    override this.FinishedLaunching (app, options) =
        Forms.Init()
        FormsMaps.Init()
        CrossMedia.Current.Initialize() |> Async.AwaitTask |> ignore
        let dbPath = getDbPath()
        let appcore = new FabulousContacts.App(dbPath)
        this.LoadApplication (appcore)
        base.FinishedLaunching(app, options)

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main(args, null, "AppDelegate")
        0

