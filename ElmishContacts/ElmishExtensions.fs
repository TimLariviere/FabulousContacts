namespace ElmishContacts

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open System.IO
open Xamarin.Forms.PlatformConfiguration.AndroidSpecific

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

module ViewHelpers =
    let onPlatform setValueFunc iosValue androidValue (element: ViewElement) =
        match Device.RuntimePlatform with
        | p when p = Device.Android && (Option.isSome androidValue) -> setValueFunc element androidValue.Value
        | p when p = Device.iOS && (Option.isSome iosValue) -> setValueFunc element iosValue.Value
        | _ -> element

    type Elmish.XamarinForms.DynamicViews.ViewElement with
        member this.OnPlatform(setValueFunc, ?ios, ?android) =
            onPlatform setValueFunc ios android this

[<AutoOpen>]
module CustomViews =
    /// ListViewGrouped XamarinForms 3.1
    let ListViewGroupedSelectionModeAttributeKey = AttributeKey<_> "ListViewGrouped_SelectionMode"

    type Elmish.XamarinForms.DynamicViews.View with
        static member ListViewGrouped_XF31(?selectionMode: ListViewSelectionMode,
                                           ?items, ?showJumpList, ?rowHeight, ?itemTapped, ?verticalOptions) =

            let attribCount = match selectionMode with None -> 0 | Some _ -> 1
            let attribs = 
                View.BuildListViewGrouped(attribCount, ?items=items, ?showJumpList=showJumpList,
                                          ?rowHeight=rowHeight, ?itemTapped=itemTapped, ?verticalOptions=verticalOptions)

            match selectionMode with None -> () | Some v -> attribs.Add(ListViewGroupedSelectionModeAttributeKey, v)

            let update (prevOpt: ViewElement voption) (source: ViewElement) target =
                View.UpdateListViewGrouped(prevOpt, source, target)
                source.UpdatePrimitive(prevOpt, target, ListViewGroupedSelectionModeAttributeKey, (fun target v -> target.SelectionMode <- v))

            ViewElement.Create(CustomGroupListView, update, attribs)

    /// TabbedPage with Bottom Placement of NavBar on Android
    type Elmish.XamarinForms.DynamicViews.View with
        static member BottomTabbedPage_XF31(?title, ?children) =
            let attribs = View.BuildTabbedPage(0, ?title=title, ?children=children)

            let update (prevOpt: ViewElement voption) (source: ViewElement) target =
                View.UpdateTabbedPage(prevOpt, source, target)
                target.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom) |> ignore

            ViewElement.Create(TabbedPage, update, attribs)

    /// Image with bytes
    let ImageStreamSourceAttributeKey = AttributeKey<_> "ImageStream_Source"

    type Elmish.XamarinForms.DynamicViews.View with
        static member ImageEx(?source: obj, ?aspect, ?margin, ?heightRequest, ?widthRequest, ?gestureRecognizers, ?isVisible, ?horizontalOptions, ?verticalOptions) =
            let attribCount = match source with None -> 0 | Some _ -> 1
            let attribs =
                View.BuildImage(attribCount, ?aspect=aspect, ?margin=margin, ?heightRequest=heightRequest,
                                ?widthRequest=widthRequest, ?gestureRecognizers=gestureRecognizers, ?isVisible=isVisible,
                                ?horizontalOptions=horizontalOptions, ?verticalOptions=verticalOptions)

            match source with None -> () | Some v -> attribs.Add(ImageStreamSourceAttributeKey, v)       

            let update (prevOpt: ViewElement voption) (source: ViewElement) target =
                View.UpdateImage(prevOpt, source, target)
                source.UpdatePrimitive(prevOpt, target, ImageStreamSourceAttributeKey,
                  (fun target v ->
                    match v with
                    | :? string as path -> target.Source <- ImageSource.op_Implicit path
                    | :? (byte array) as bytes -> target.Source <- ImageSource.FromStream(fun () -> new MemoryStream(bytes) :> Stream)
                    | :? ImageSource as imageSource -> target.Source <- imageSource
                    | _ -> ()              
                  ))

            ViewElement.Create(Image, update, attribs)