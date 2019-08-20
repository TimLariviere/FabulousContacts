namespace FabulousContacts

open Fabulous
open Fabulous.XamarinForms
open FabulousContacts.Controls
open FabulousContacts.Helpers
open FabulousContacts.Style
open Xamarin.Forms

module Components =
    let centralLabel text =
        View.Label(text = text,
                   horizontalOptions = LayoutOptions.Center,
                   verticalOptions = LayoutOptions.CenterAndExpand)

    let formLabel text =
        View.Label(text = text,
                   margin = Thickness(0., 20., 0., 5.))

    let formEntry placeholder text keyboard isValid textChanged =
        View.BorderedEntry(placeholder = placeholder,
                           text = text,
                           keyboard = keyboard,
                           textChanged = debounce 250 (fun e -> e.NewTextValue |> textChanged),
                           borderColor = if isValid then Color.Default else Color.Red)

    let formEditor text textChanged =
        View.Editor(text = text,
                    textChanged = debounce 250 (fun e -> e.NewTextValue |> textChanged),
                    heightRequest = 100.)

    let destroyButton text command isVisible =
        View.Button(text = text,
                    command = command,
                    isVisible = isVisible,
                    backgroundColor = Color.Red,
                    textColor = Color.White,
                    margin = Thickness(0., 20., 0., 0.),
                    verticalOptions = LayoutOptions.EndAndExpand)

    let toolbarButton text command =
        View.ToolbarItem(order = ToolbarItemOrder.Primary,
                         text = text,
                         command = command)

    let groupView name =
        View.StackLayout(
            backgroundColor = accentColor,
            children = [
                View.Label(text = name,
                           textColor = accentTextColor,
                           verticalOptions = LayoutOptions.FillAndExpand,
                           verticalTextAlignment = TextAlignment.Center,
                           margin = Thickness(20., 5.))
            ])

    let cellView picture name address isFavorite =
        let source = picture |> getValueOrDefault "addphoto.png"

        View.StackLayout(
            orientation = StackOrientation.Horizontal,
            padding = 5.,
            spacing = 10.,
            children = [
                View.Image(source = source,
                           aspect = Aspect.AspectFill,
                           margin = Thickness(15., 0., 0., 0.),
                           heightRequest = 50.,
                           widthRequest = 50.)
                View.StackLayout(spacing = 5.,
                                 horizontalOptions = LayoutOptions.FillAndExpand,
                                 margin = Thickness(0., 5., 0., 5.),
                                 children = [
                    View.Label(text = name,
                               fontSize = 18.,
                               verticalOptions = LayoutOptions.FillAndExpand,
                               verticalTextAlignment = TextAlignment.Center)
                    View.Label(text = address,
                               fontSize = 12.,
                               textColor = Color.Gray,
                               lineBreakMode = LineBreakMode.TailTruncation)
                ])
                View.Image(source = "star.png",
                           isVisible = isFavorite,
                           verticalOptions = LayoutOptions.Center,
                           margin = Thickness(0., 0., 15., 0.),
                           heightRequest = 25.,
                           widthRequest = 25.)
            ]
        )

    let cachedCellView picture name address isFavorite =
        dependsOn (picture, name, address, isFavorite) (fun _ (p, n, a, i) ->
            cellView p n a i)

    let detailActionButton image command =
        View.Button(image = image,
                    command = command,
                    backgroundColor = accentColor,
                    heightRequest = 35.,
                    horizontalOptions = LayoutOptions.FillAndExpand)

    let detailFieldTitle text =
        View.Label(text = text,
                   fontAttributes = FontAttributes.Bold,
                   margin = Thickness(0., 10., 0., 0.))

    let optionalLabel text =
        match text with
        | "" ->
            View.Label(text = Strings.Common_NotSpecified,
                       fontAttributes = FontAttributes.Italic)
        | _ ->
            View.Label(text = text)
            
    let favoriteField isFavorite markAsFavorite =
        View.StackLayout(orientation = StackOrientation.Horizontal,
                         margin = Thickness(0., 20., 0., 0.),
                         children = [
            View.Label(text = Strings.EditPage_MarkAsFavoriteField_Label,
                       verticalOptions = LayoutOptions.Center)
            View.Switch(isToggled = isFavorite,
                        toggled = markAsFavorite,
                        horizontalOptions = LayoutOptions.EndAndExpand,
                        verticalOptions = LayoutOptions.Center)
        ])
      
    let profilePictureButton picture updatePicture =
        match picture with
        | None ->
            View.Button(image = "addphoto.png",
                        backgroundColor = Color.White,
                        command = updatePicture)
                .GridRowSpan(2)
        | Some picture ->
            View.Image(source = picture,
                       aspect = Aspect.AspectFill,
                       gestureRecognizers = [
                View.TapGestureRecognizer(command = updatePicture)
            ]).GridRowSpan(2)