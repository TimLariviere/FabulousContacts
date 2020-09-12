namespace FabulousContacts

open System
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.Maps
open FabulousContacts.Components
open FabulousContacts.Helpers
open FabulousContacts.Models
open Xamarin.Essentials
open Xamarin.Forms.Maps

module MapPage =
    // Declarations
    type ContactPin =
        { Position: Position
          Label: string
          PinType: PinType
          Address: string }
        
    type Msg =
        | LoadPins of Contact list
        | RetrieveUserPosition
        | PinsLoaded of ContactPin list
        | UserPositionRetrieved of (double * double)

    type Model =
        { Pins: ContactPin list option
          UserPosition: (double * double) option }

    // Functions
    let tryGetUserPositionAsync () = async {
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
            do! displayAlert(Strings.MapPage_MapLoadFailed, exn.Message, Strings.Common_OK)
            return None
    }
    
    let paris = Position(48.8566, 2.3522)
    let getUserPositionOrDefault (userPosition: (double * double) option) =
        userPosition
        |> Option.map Position
        |> Option.defaultValue paris

    // Lifecycle
    let initModel =
        { Pins = None
          UserPosition = None }
    
    let init () =
        initModel, Cmd.none

    let update msg model =
        match msg with
        | LoadPins contacts ->
            let msg = loadPinsAsync contacts
            model, Cmd.ofAsyncMsgOption msg

        | RetrieveUserPosition ->
            let msg = tryGetUserPositionAsync ()
            model, Cmd.ofAsyncMsgOption msg

        | PinsLoaded pins ->
            { model with Pins = Some pins }, Cmd.none

        | UserPositionRetrieved location ->
            { model with UserPosition = Some location }, Cmd.none
        
    let view model dispatch =
        let map userPositionOpt pins =
            View.Map(
                hasZoomEnabled = true,
                hasScrollEnabled = true,
                requestedRegion = MapSpan.FromCenterAndRadius(getUserPositionOrDefault userPositionOpt, Distance.FromKilometers(25.)),
                pins = [
                    for pin in pins do
                        yield View.Pin(
                           position = pin.Position,
                           label = pin.Label,
                           pinType = pin.PinType,
                           address = pin.Address
                        )
                ]
            )
        
        dependsOn (model.UserPosition, model.Pins) (fun model (userPositionOpt, pins) ->
            // Actions
            let retrieveUserPosition = fun () -> dispatch RetrieveUserPosition
            
            // View
            View.ContentPage(title = Strings.MapPage_Title,
                             icon = Image.fromPath "maptab.png",
                             appearing = retrieveUserPosition,
                             content =
                match pins with
                | None ->
                    centralLabel Strings.MapPage_Loading
                | Some pins ->
                    map userPositionOpt pins
            )
        )