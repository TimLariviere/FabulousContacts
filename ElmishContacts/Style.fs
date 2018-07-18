namespace ElmishContacts

open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module Style =
    let mkFormLabel text =
        View.Label(text=text, margin=new Thickness(20., 30., 20., 20.))

    let mkFormEntry text textChanged =
        View.Entry(text=text, textChanged=textChanged, margin=new Thickness(20., 0., 20., 0.))

    let mkFormSwitch isToggled toggled =
        View.Switch(isToggled=isToggled, toggled=toggled, margin=new Thickness(20., 0., 20., 0.))

    let mkDestroyButton text command isVisible =
        View.Button(text=text, command=command, isVisible=isVisible, backgroundColor=Color.Red, textColor=Color.White, margin=new Thickness(20., 0., 20., 20.), verticalOptions=LayoutOptions.EndAndExpand)

    let mkToolbarButton text command =
        View.ToolbarItem(order=ToolbarItemOrder.Primary, text=text, command=command)

    let mkCellView name isFavorite =
        View.StackLayout(
            orientation=StackOrientation.Horizontal,
            children=[
                View.Label(text=name, horizontalOptions=LayoutOptions.StartAndExpand, verticalTextAlignment=TextAlignment.Center, margin=new Thickness(20., 0.))
                View.Image(source="star.png", isVisible=isFavorite, verticalOptions=LayoutOptions.Center, margin=new Thickness(0., 0., 20., 0.), heightRequest=25., widthRequest=25.)
            ]
        )