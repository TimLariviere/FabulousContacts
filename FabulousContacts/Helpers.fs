namespace FabulousContacts

open System.IO
open Plugin.Media
open Plugin.Media.Abstractions
open Plugin.Permissions
open Plugin.Permissions.Abstractions
open Xamarin.Forms

module Helpers =
    let displayAlert (title, message, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, cancel)
        |> Async.AwaitTask

    let displayAlertWithConfirm (title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel)
        |> Async.AwaitTask

    let displayActionSheet (title, cancel, destruction, buttons) =
        let title = Option.toObj title
        let cancel = Option.toObj cancel
        let destruction = Option.toObj destruction
        let buttons = Option.toObj buttons
        Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons)
        |> Async.AwaitTask

    let requestPermissionAsync permission = async {
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
                
            if status = PermissionStatus.Granted then
                return true
            else
                return! requestPermissionAsync permission
        with _ ->
            return false
    }

    let takePictureAsync () = async {
        let options = StoreCameraMediaOptions()
        let! picture = CrossMedia.Current.TakePhotoAsync(options) |> Async.AwaitTask
        return picture |> Option.ofObj
    }

    let pickPictureAsync () = async {
        let options = PickMediaOptions()
        let! picture = CrossMedia.Current.PickPhotoAsync(options) |> Async.AwaitTask
        return picture |> Option.ofObj
    }

    let readBytesAsync (file: MediaFile) =  async {
        use stream = file.GetStream()
        use memoryStream = new MemoryStream()
        do! stream.CopyToAsync(memoryStream) |> Async.AwaitTask
        return memoryStream.ToArray()
    }
    
    let getValueOrDefault defaultValue value =
        match value with
        | None -> box defaultValue
        | Some bytes -> box bytes