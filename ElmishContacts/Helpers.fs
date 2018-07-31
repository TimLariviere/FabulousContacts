namespace ElmishContacts

open Plugin.Media
open Plugin.Media.Abstractions
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
            | choice when choice = pickPicture -> CrossMedia.Current.PickPhotoAsync() |> Async.AwaitTask
            | choice when choice = takePicture -> CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()) |> Async.AwaitTask
            | _ -> System.Threading.Tasks.Task.FromResult<Abstractions.MediaFile>(null) |> Async.AwaitTask
    }

    let readFileAsBase64 (file: Plugin.Media.Abstractions.MediaFile) = async {
        use stream = file.GetStream()
        use memoryStream = new MemoryStream()
        do! stream.CopyToAsync(memoryStream) |> Async.AwaitTask
        let bytes = memoryStream.ToArray()
        return Convert.ToBase64String(bytes)
    }

    let getBase64 picture = 
        match picture with
        | None -> ""
        | Some base64 -> base64

    let getImageSourceFromBase64 base64 =
        let bytes = Convert.FromBase64String(base64)
        let stream = new MemoryStream(bytes) :> Stream
        ImageSource.FromStream(fun () -> stream)