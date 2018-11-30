namespace ElmishContacts.iOS

open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open System.ComponentModel
open UIKit

type ToolbarSide = Left | Right

type ExtendedContentPageRenderer() =
    inherit PageRenderer()

    let applyToolbarItems (element: Page) (controller: UINavigationController) =
        match controller with
        | null -> ()
        | _ ->
            let navigationItem = controller.TopViewController.NavigationItem

            let count = element.ToolbarItems.Count - 1

            let buttons =
                element.ToolbarItems
                |> Seq.mapi (fun index item ->
                    let i = count - index
                    match item.Priority with
                    | 0 -> (Left, navigationItem.RightBarButtonItems.[i])
                    | 1 -> (Right, navigationItem.RightBarButtonItems.[i])
                    | _ -> failwith "Not implemented"
                )

            let getButtonsFromSide side = buttons |> Seq.filter (fun (s, _) -> s = side) |> Seq.map snd |> Seq.toArray

            navigationItem.SetLeftBarButtonItems(getButtonsFromSide Left, false)
            navigationItem.SetRightBarButtonItems(getButtonsFromSide Right, false)

    member this.OnElementPropertyChanged sender (e: PropertyChangedEventArgs) =
        match e.PropertyName with
        | "ToolbarItems" -> applyToolbarItems (this.Element :?> Page) this.NavigationController
        | _ -> ()

    override this.OnElementChanged(e) =
        if e.OldElement <> null then e.OldElement.PropertyChanged.RemoveHandler(PropertyChangedEventHandler(this.OnElementPropertyChanged))
        if e.NewElement <> null then e.NewElement.PropertyChanged.AddHandler(PropertyChangedEventHandler(this.OnElementPropertyChanged))
        applyToolbarItems (this.Element :?> Page) this.NavigationController

    override this.ViewWillAppear(animated) =
        base.ViewWillAppear animated
        applyToolbarItems (this.Element :?> Page) this.NavigationController


module Dummy_ExtendedContentPageRenderer =
    [<assembly: Xamarin.Forms.ExportRenderer(typeof<Xamarin.Forms.ContentPage>, typeof<ExtendedContentPageRenderer>)>]
    do ()