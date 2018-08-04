namespace ElmishContacts

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open System.IO

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

[<AutoOpen>]
module CustomViews =
    /// ListViewGrouped XamarinForms 3.1
    let ListViewGroupedSelectionModeAttributeKey = AttributeKey<_> "ListViewGrouped_SelectionMode"

    type View with
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

    /// Image with bytes
    let ImageStreamSourceAttributeKey = AttributeKey<_> "ImageStream_Source"

    type View with
        static member Image_Stream(?source: obj, ?aspect, ?margin, ?heightRequest, ?widthRequest, ?gestureRecognizers) =
            let attribCount = match source with None -> 0 | Some _ -> 1
            let attribs =
                View.BuildImage(attribCount, ?aspect=aspect, ?margin=margin, ?heightRequest=heightRequest,
                                ?widthRequest=widthRequest, ?gestureRecognizers=gestureRecognizers)

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