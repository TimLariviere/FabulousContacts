namespace ElmishContacts

open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open Helpers
open Images

module Style =
    let accentTextColor = Color.White
    let accentColor = Color.FromHex("#3080b1")

    let mkCentralLabel text =
        View.Label(text=text, horizontalOptions=LayoutOptions.Center, verticalOptions=LayoutOptions.CenterAndExpand)

    let mkFormLabel text =
        View.Label(text=text, margin=new Thickness(20., 20., 20., 10.))

    let mkFormEntry text textChanged =
        View.Entry(text=text, textChanged=textChanged, margin=new Thickness(20., 0., 20., 0.))

    let mkFormEditor text textChanged =
        View.Editor(text=text, textChanged=textChanged, heightRequest=100., margin=new Thickness(20., 0., 20., 0.))

    let mkFormSwitch isToggled toggled =
        View.Switch(isToggled=isToggled, toggled=toggled, margin=new Thickness(20., 0., 20., 0.))

    let mkDestroyButton text command isVisible =
        View.Button(text=text, command=command, isVisible=isVisible, backgroundColor=Color.Red, textColor=Color.White, margin=new Thickness(20., 0., 20., 20.), verticalOptions=LayoutOptions.EndAndExpand)

    let mkToolbarButton text command =
        View.ToolbarItem(order=ToolbarItemOrder.Primary, text=text, command=command)

    let mkGroupView name =
        View.StackLayout(
            backgroundColor=accentColor,
            children=[
                View.Label(text=name, textColor=accentTextColor, verticalOptions=LayoutOptions.FillAndExpand, verticalTextAlignment=TextAlignment.Center, margin=Thickness(20., 5.))
            ]
        )

    let mkCellView picture name address isFavorite =
        let source =
            match picture with
            | null -> "addphoto.png" :> obj
            | bytes -> createImageSource name bytes :> obj

        View.StackLayout(
            orientation=StackOrientation.Horizontal,
            padding=5.,
            spacing=10.,
            children=[
                View.Image(source=source, aspect=Aspect.AspectFill, margin=new Thickness(15., 0., 0., 0.), heightRequest=50., widthRequest=50.)
                View.StackLayout(
                    spacing=5.,
                    horizontalOptions=LayoutOptions.FillAndExpand,
                    margin=Thickness(0., 5., 0., 5.),
                    children=[
                        View.Label(text=name, fontSize=18., verticalOptions=LayoutOptions.FillAndExpand, verticalTextAlignment=TextAlignment.Center)
                        View.Label(text=address, fontSize=12., textColor=Color.Gray, lineBreakMode=LineBreakMode.TailTruncation)
                    ]
                )
                View.Image(source="star.png", isVisible=isFavorite, verticalOptions=LayoutOptions.Center, margin=new Thickness(0., 0., 15., 0.), heightRequest=25., widthRequest=25.)
            ]
        )

    let mkCachedCellView picture name address isFavorite =
        dependsOn (picture, name, address, isFavorite) (fun _ (cPicture, cName, cAddress, cIsFavorite) -> mkCellView cPicture cName cAddress cIsFavorite)