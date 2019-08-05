namespace FabulousContacts

open System
open System.IO
open Xamarin.Forms
open Plugin.Media
open Plugin.Media.Abstractions
open Plugin.Permissions
open Plugin.Permissions.Abstractions
open FSharp.Linq.RuntimeHelpers

module Helpers =
    let displayAlert(title, message, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, cancel)
        |> Async.AwaitTask

    let displayAlertWithConfirm(title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel)
        |> Async.AwaitTask

    let displayActionSheet(title, cancel, destruction, buttons) =
        let title = Option.toObj title
        let cancel = Option.toObj cancel
        let destruction = Option.toObj destruction
        let buttons = Option.toObj buttons
        Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons)
        |> Async.AwaitTask

    let requestPermission permission = async {
        try
            let! status =
                CrossPermissions.Current.RequestPermissionsAsync([| permission |])
                |> Async.AwaitTask
            return status.[permission] = PermissionStatus.Granted
                || status.[permission] = PermissionStatus.Unknown
        with _ ->
            return false
    }

    let askPermissionAsync permission = async {
        try
            let! status =
                CrossPermissions.Current.CheckPermissionStatusAsync(permission)
                |> Async.AwaitTask
            let request () =
                requestPermission permission
                |> Async.RunSynchronously
            return status = PermissionStatus.Granted
                || request()
        with _ ->
            return false
    }

    let takePictureAsync() = async {
        let options = StoreCameraMediaOptions()
        return! CrossMedia.Current.TakePhotoAsync(options) |> Async.AwaitTask
    }

    let pickPictureAsync() = async {
        let options = PickMediaOptions()
        return! CrossMedia.Current.PickPhotoAsync(options) |> Async.AwaitTask
    }

    let streamToArray (file: MediaFile) = async {
        let stream = file.GetStream()
        use memoryStream = new MemoryStream()
        do! stream.CopyToAsync(memoryStream) |> Async.AwaitTask
        return memoryStream.ToArray()
    }

    let readBytesAsync (file: MediaFile) =  async {
        return
            file
            |> Option.ofObj
            |> Option.map (streamToArray >> Async.RunSynchronously)
    }