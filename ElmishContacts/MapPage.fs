namespace ElmishContacts

open Models
open Repository
open Style
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open Xamarin.Forms.Maps

module MapPage =
    type Msg = | PinsLoaded of ContactPin list

    type ExternalMsg = | NoOp

    type Model =
        {
            Pins: ContactPin list option
        }

    let loadPinsAsync dbPath = async {
        let geocoder = Geocoder()

        let! contacts = loadAllContacts dbPath
        let gettingPositions =
            contacts
            |> List.map (fun c -> async {
                let! positions = geocoder.GetPositionsForAddressAsync(c.Address) |> Async.AwaitTask
                let position = positions |> Seq.tryHead
                return (c, position)
            })
            |> Async.Parallel

        let! contactsAndPositions = gettingPositions

        let pins = contactsAndPositions
                   |> Array.filter (fun (_, p) -> Option.isSome p)
                   |> Array.map (fun (c, p) -> { Position = p.Value; Label = c.Name; PinType = PinType.Place; Address = c.Address})
                   |> Array.toList

        return PinsLoaded pins
    }

    let init dbPath =
        {
            Pins = None
        }, Cmd.ofAsyncMsg (loadPinsAsync dbPath)

    let update msg model =
        match msg with
        | PinsLoaded pins ->
            { model with Pins = Some pins }, Cmd.none, ExternalMsg.NoOp

    let view model dispatch =
        dependsOn model.Pins (fun model (mPins) ->
            let paris = Position(48.8566, 2.3522)

            View.ContentPage(
                content=
                    match mPins with
                    | None ->
                        mkCentralLabel "Loading..."
                    | Some pins ->
                        View.Map(
                            hasZoomEnabled=true,
                            hasScrollEnabled=true,
                            requestedRegion=MapSpan.FromCenterAndRadius(paris, Distance.FromKilometers(25.)),
                            pins=[
                                for pin in pins do
                                    yield View.Pin(position=pin.Position, label=pin.Label, pinType=pin.PinType, address=pin.Address)
                            ]
                        )
            )
        )