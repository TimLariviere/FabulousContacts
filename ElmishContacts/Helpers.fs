namespace ElmishContacts

open Plugin.Media
open Plugin.Media.Abstractions
open Plugin.Permissions
open Plugin.Permissions.Abstractions
open Xamarin.Forms
open System
open System.IO

module Helpers =
    let displayAlert(title, message, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, cancel) |> Async.AwaitTask

    let displayAlertWithConfirm(title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title, message, accept, cancel) |> Async.AwaitTask

    let displayActionSheet(title, cancel, destruction, buttons) =
        let title =
            match title with
            | None -> null
            | Some label -> label

        let cancel =
            match cancel with
            | None -> null
            | Some label -> label

        let destruction =
            match destruction with
            | None -> null
            | Some label -> label

        let buttons =
            match buttons with
            | None -> null
            | Some buttons -> buttons

        Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons) |> Async.AwaitTask

    let askPermissionAsync permission = async {
        try
            let! status = CrossPermissions.Current.CheckPermissionStatusAsync(permission) |> Async.AwaitTask
            if status = PermissionStatus.Granted then
                return true
            else
                let! status = CrossPermissions.Current.RequestPermissionsAsync([| permission |]) |> Async.AwaitTask
                return status.[permission] = PermissionStatus.Granted
                       || status.[permission] = PermissionStatus.Unknown
        with exn ->
            return false
    }

    let takePictureAsync() = async {
        let options = new StoreCameraMediaOptions()
        options.AllowCropping <- Nullable<bool>(true)
        options.PhotoSize <- PhotoSize.MaxWidthHeight
        options.MaxWidthHeight <- Nullable<int>(75)
        return! CrossMedia.Current.TakePhotoAsync(options) |> Async.AwaitTask
    }

    let pickPictureAsync() = async {
        let options = new PickMediaOptions()
        options.PhotoSize <- PhotoSize.MaxWidthHeight
        options.MaxWidthHeight <- Nullable<int>(75)
        return! CrossMedia.Current.PickPhotoAsync(options) |> Async.AwaitTask
    }

    let readBytesAsync (file: Plugin.Media.Abstractions.MediaFile) = async {
        match file with
        | null -> return None
        | _ ->
            use stream = file.GetStream()
            use memoryStream = new MemoryStream()
            do! stream.CopyToAsync(memoryStream) |> Async.AwaitTask
            return Some (memoryStream.ToArray())
    }

module Images =
    open System.Collections.Generic

    type internal Memoizations() = 
         static let t = Dictionary<byte array, ImageSource>(HashIdentity.Structural)
         static member T = t
         static member Add(key: byte array, res: ImageSource) = 
             if Memoizations.T.Count > 50000 then 
                 System.Diagnostics.Trace.WriteLine("Clearing 'imageSource' memoizations...")
                 Memoizations.T.Clear()
             
             Memoizations.T.[key] <- res

         static member Remove(key: byte array) =
            Memoizations.T.Remove(key)

    let createImageSource key =
        match Memoizations.T.TryGetValue(key) with
        | true, imageSource -> imageSource
        | _ ->
            let imageSource = ImageSource.FromStream(fun () -> new MemoryStream(key) :> Stream)
            Memoizations.T.Add(key, imageSource)
            imageSource

    let releaseImageSource key =
        match Memoizations.T.TryGetValue(key) with
        | true, _ -> Memoizations.Remove(key) |> ignore
        | _ -> ()