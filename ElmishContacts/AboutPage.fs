namespace ElmishContacts

open Elmish.XamarinForms.DynamicViews

module AboutPage =
    let view () =
        View.ContentPage(
            content=View.StackLayout(
                children=[
                    View.Label(text="Icons made by https://www.flaticon.com/authors/freepik from www.flaticon.com")
                ]
            )
        )