namespace FabulousContacts

open Helpers
open Models
open Style
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Essentials
open Xamarin.Forms.Maps
open System

module MapPage =
    // Declarations
    type Msg =
        | LoadPins of Contact list
        | RetrieveUserPosition
        | PinsLoaded of ContactPin list
        | UserPositionRetrieved of (double * double)

    type Model =
        { Pins: ContactPin list option
          UserPosition: (double * double) option }

    // Functions
    let getUserPositionAsync() = async {
        try
            let! location =
                Geolocation.GetLastKnownLocationAsync() |> Async.AwaitTask
            return
                location
                |> Option.ofObj
                |> Option.map (fun l -> UserPositionRetrieved (l.Latitude, l.Longitude))
        with _ ->
            return None
    }

    let loadPinsAsync (contacts: Contact list) = async {
        try
            let geocoder = Geocoder()
            let gettingPositions =
                contacts
                |> List.filter (fun c -> c.Address |> (not << String.IsNullOrWhiteSpace))
                |> List.map (fun c -> async {
                    try
                        let! positions = geocoder.GetPositionsForAddressAsync(c.Address) |> Async.AwaitTask
                        let position = positions |> Seq.tryHead
                        return Some (c, position)
                    with _ ->
                        return None
                })
                |> Async.Parallel

            let! contactsAndPositions = gettingPositions
            let pins =
                contactsAndPositions
                |> Array.filter Option.isSome
                |> Array.map (fun v -> v.Value)
                |> Array.filter (snd >> Option.isSome)
                |> Array.map (fun (c, p) ->
                    { Position = p.Value
                      Label = sprintf "%s %s " c.FirstName c.LastName
                      PinType = PinType.Place
                      Address = c.Address })
                |> Array.toList

            return Some (PinsLoaded pins)
        with exn ->
            do! displayAlert("Can't load map", exn.Message, "OK")
            return None
    }

    // Lifecycle
    let init () =
        { Pins = None
          UserPosition = None }, Cmd.none

    let update msg model =
        match msg with
        | LoadPins contacts ->
            let msg = loadPinsAsync contacts
            model, Cmd.ofAsyncMsgOption msg
        | RetrieveUserPosition ->
            let msg = getUserPositionAsync()
            model, Cmd.ofAsyncMsgOption msg
        | PinsLoaded pins ->
            { model with Pins = Some pins }, Cmd.none
        | UserPositionRetrieved location ->
            { model with UserPosition = Some location }, Cmd.none

    let paris = Position(48.8566, 2.3522)
    let center (userPosition: (double * double) option) =
        userPosition
        |> Option.map Position
        |> Option.defaultValue paris

    let mkContent pins userPosition =
        let mkPin pin =
            View.Pin(position = pin.Position,
                     label = pin.Label,
                     pinType = pin.PinType,
                     address = pin.Address)
        match pins with
        | None ->
            mkCentralLabel "Loading map..."
        | Some pins ->
            View.Map(hasZoomEnabled = true,
                     hasScrollEnabled = true,
                     requestedRegion = MapSpan.FromCenterAndRadius(center userPosition, Distance.FromKilometers(25.)),
                     pins = [
                        for pin in pins do
                            yield mkPin pin
                     ]
            )

    let view model dispatch =
        dependsOn (model.UserPosition, model.Pins) (fun model (userPosition, pins) ->
            View.ContentPage(
                title = "Map",
                icon = "maptab.png",
                appearing = (fun() -> dispatch RetrieveUserPosition),
                content = mkContent pins userPosition
            )
        )