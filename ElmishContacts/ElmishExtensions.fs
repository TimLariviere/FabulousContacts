namespace ElmishContacts

open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module Cmd =
    let ofAsyncMsgOption (p: Async<'msg option>) : Cmd<'msg> =
        [ fun dispatch -> async { let! msg = p in match msg with None -> () | Some msg -> dispatch msg } |> Async.StartImmediate ]

[<AutoOpen>]
module CustomViews =

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