namespace ElmishContacts

open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

module Style =
    let navigationPageBarTextColor = Color.White
    let navigationPageBarBackgroundColor = Color.FromHex("#3080b1")

    let mkCentralLabel text =
        View.Label(text=text, horizontalOptions=LayoutOptions.Center, verticalOptions=LayoutOptions.CenterAndExpand)

    let mkFormLabel text =
        View.Label(text=text, margin=new Thickness(20., 20., 20., 10.))

    let mkFormEntry text textChanged =
        View.Entry(text=text, textChanged=textChanged, margin=new Thickness(20., 0., 20., 0.))

    let mkFormSwitch isToggled toggled =
        View.Switch(isToggled=isToggled, toggled=toggled, margin=new Thickness(20., 0., 20., 0.))

    let mkDestroyButton text command isVisible =
        View.Button(text=text, command=command, isVisible=isVisible, backgroundColor=Color.Red, textColor=Color.White, margin=new Thickness(20., 0., 20., 20.), verticalOptions=LayoutOptions.EndAndExpand)

    let mkToolbarButton text command =
        View.ToolbarItem(order=ToolbarItemOrder.Primary, text=text, command=command)

    let mkGroupView name =
        View.StackLayout(
            heightRequest=25.,
            backgroundColor=navigationPageBarBackgroundColor,
            children=[
                View.Label(text=name, verticalOptions=LayoutOptions.FillAndExpand, verticalTextAlignment=TextAlignment.Center, margin=Thickness(20., 5.))
            ]
        )

    let mkCellView name address isFavorite =
        View.StackLayout(
            heightRequest=55.,
            orientation=StackOrientation.Horizontal,
            children=[
                View.StackLayout(
                    spacing=5.,
                    horizontalOptions=LayoutOptions.FillAndExpand,
                    margin=Thickness(20., 5.),
                    children=[
                        View.Label(text=name, fontSize=18.)
                        View.Label(text=address, fontSize=12.)
                    ]
                )
                View.Image(source="star.png", isVisible=isFavorite, verticalOptions=LayoutOptions.Center, margin=new Thickness(0., 0., 20., 0.), heightRequest=25., widthRequest=25.)
            ]
        )

    let mkCachedCellView name address isFavorite =
        dependsOn (name, address, isFavorite) (fun _ (cName, cAddress, cIsFavorite) -> mkCellView cName cAddress cIsFavorite)