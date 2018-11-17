namespace ElmishContacts.UITests

open System
open System.IO
open Xamarin.UITest
open Xamarin.UITest.Queries

module AppInitializer =
  let startApp (platform: Platform) =
    if platform = Platform.Android then
      ConfigureApp.Android.EnableLocalScreenshots().StartApp () :> IApp
    else
      ConfigureApp.iOS.StartApp () :> _
