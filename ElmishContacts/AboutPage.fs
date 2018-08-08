namespace ElmishContacts

open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open Style
open Xamarin.Essentials
open System
open ElmishContacts.Controls

module AboutPage =
    let openBrowser url =
        View.TapGestureRecognizer(command=(fun() -> Browser.OpenAsync(Uri(url)) |> ignore))

    let view () =
        dependsOn () (fun _ () ->
            View.ContentPage(
                content=View.ScrollView(
                    content=View.StackLayout(
                        padding=Thickness(20., 10., 20., 20.),
                        children=[
                            View.StackLayout(
                                heightRequest=100.,
                                widthRequest=100.,
                                horizontalOptions=LayoutOptions.Center,
                                backgroundColor=accentColor,
                                padding=15.,
                                children=[
                                    View.Image(source="icon.png")
                                ]
                            )
                            View.Label(text="ElmishContacts v1.0", fontAttributes=FontAttributes.Bold, horizontalOptions=LayoutOptions.Center)

                            View.Label(text="Description", fontAttributes=FontAttributes.Bold, margin=Thickness(0., 20., 0., 0.))
                            View.Label(text="ElmishContacts is an open-source sample Contacts app")
                            View.UnderlinedLabel(text="https://github.com/TimLariviere/ElmishContacts", gestureRecognizers=[ openBrowser "https://github.com/TimLariviere/ElmishContacts" ])
                            View.Label(text="Made with")
                            View.StackLayout(
                                horizontalOptions=LayoutOptions.Center,
                                orientation=StackOrientation.Horizontal,
                                spacing=30.,
                                children=[
                                    View.StackLayout(
                                        gestureRecognizers=[ openBrowser "https://fsharp.org" ],
                                        children=[
                                            View.Image(source="fsharp.png", heightRequest=50., widthRequest=50.)
                                            View.Label(text="F#", horizontalTextAlignment=TextAlignment.Center)
                                        ]
                                    )
                                    View.StackLayout(
                                        gestureRecognizers=[ openBrowser "https://github.com/fsprojects/Elmish.XamarinForms" ],
                                        children=[
                                            View.Image(source="xamarin.png", heightRequest=50., widthRequest=50.)
                                            View.Label(text="Elmish.XamarinForms", horizontalTextAlignment=TextAlignment.Center)
                                        ]
                                    )
                                ]
                            )

                            View.Label(text="Credits", fontAttributes=FontAttributes.Bold, margin=Thickness(0., 20., 0., 0.))
                            View.UnderlinedLabel(text="Some icons by Freepik", gestureRecognizers=[ openBrowser "https://www.flaticon.com/authors/freepik" ])
                            View.UnderlinedLabel(text="Xamarin.Essentials", gestureRecognizers=[ openBrowser "https://github.com/xamarin/Essentials" ])

                            View.Label(text="Author", fontAttributes=FontAttributes.Bold, margin=Thickness(0., 20., 0., 0.))
                            View.Label(text="Timothé Larivière")
                            View.StackLayout(
                                orientation=StackOrientation.Horizontal,
                                spacing=15.,
                                gestureRecognizers=[ openBrowser "https://timothelariviere.com" ],
                                children=[
                                    View.Image(source="blog.png", heightRequest=35., widthRequest=35.)
                                    View.UnderlinedLabel(text="https://timothelariviere.com", verticalOptions=LayoutOptions.Center)
                                ]
                            )
                            View.StackLayout(
                                orientation=StackOrientation.Horizontal,
                                spacing=15.,
                                gestureRecognizers=[ openBrowser "https://github.com/TimLariviere" ],
                                children=[
                                    View.Image(source="github.png", heightRequest=35., widthRequest=35.)
                                    View.UnderlinedLabel(text="TimLariviere", verticalOptions=LayoutOptions.Center)
                                ]
                            )
                            View.Label(text="If you want to know more about this app or just want to reach me:", margin=Thickness(0., 10., 0., 0.))
                            View.StackLayout(
                                horizontalOptions=LayoutOptions.Center,
                                orientation=StackOrientation.Horizontal,
                                margin=Thickness(0., 10., 0., 0.),
                                spacing=15.,
                                children=[
                                    View.StackLayout(
                                        gestureRecognizers=[ openBrowser "https://twitter.com/Tim_Lariviere" ],
                                        children=[
                                            View.Image(source="twitter.png", heightRequest=50., widthRequest=50.)
                                            View.Label(text="@Tim_Lariviere", horizontalTextAlignment=TextAlignment.Center)
                                        ]
                                    )
                                    View.StackLayout(
                                        gestureRecognizers=[ openBrowser "https://fsharp.org/guides/slack/" ],
                                        children=[
                                            View.Image(source="slack.png", heightRequest=50., widthRequest=50.)
                                            View.Label(text="@Timothé Larivière", horizontalTextAlignment=TextAlignment.Center)
                                        ]
                                    )
                                ]
                            )
                        ]
                    )
                )
            )
        )