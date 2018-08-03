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

    let askCameraPermissionsAsync () = async {
        try
            let! cameraStatus = CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera) |> Async.AwaitTask
            let! photosStatus = CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos) |> Async.AwaitTask

            if cameraStatus = PermissionStatus.Granted && photosStatus = PermissionStatus.Granted then
                return true
            else
                let! status = CrossPermissions.Current.RequestPermissionsAsync([| Permission.Camera; Permission.Photos |]) |> Async.AwaitTask
                return status.[Permission.Camera] = PermissionStatus.Granted
                       && status.[Permission.Photos] = PermissionStatus.Granted
        with exn ->
            return false
    }

    let pickOrTakePictureAsync () = async {
        let canPickPicture = CrossMedia.Current.IsPickPhotoSupported
        let canTakePicture = CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported

        let pickPicture = "Choose from gallery"
        let takePicture = "Take picture"
        let cancel = "Cancel"

        let choices = [|
            if canPickPicture then yield pickPicture
            if canTakePicture then yield takePicture
        |]

        let! source =
            displayActionSheet(None, Some cancel, None, Some choices)

        return!
            match source with
            | choice when choice = pickPicture ->
                let options = new PickMediaOptions()
                options.PhotoSize <- PhotoSize.MaxWidthHeight
                options.MaxWidthHeight <- Nullable<int>(75)
                CrossMedia.Current.PickPhotoAsync(options) |> Async.AwaitTask

            | choice when choice = takePicture -> 
                let options = new StoreCameraMediaOptions()
                options.AllowCropping <- Nullable<bool>(true)
                options.PhotoSize <- PhotoSize.MaxWidthHeight
                options.MaxWidthHeight <- Nullable<int>(75)
                CrossMedia.Current.TakePhotoAsync(options) |> Async.AwaitTask

            | _ -> System.Threading.Tasks.Task.FromResult<Abstractions.MediaFile>(null) |> Async.AwaitTask
    }

    let readBytesAsync (file: Plugin.Media.Abstractions.MediaFile) = async {
        use stream = file.GetStream()
        use memoryStream = new MemoryStream()
        do! stream.CopyToAsync(memoryStream) |> Async.AwaitTask
        return memoryStream.ToArray()
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