namespace ElmishContacts

module Models =
    open Xamarin.Forms.Maps

    type Contact =
        {
            Id: int
            Picture: string
            FirstName: string
            LastName: string
            Address: string
            IsFavorite: bool
        }

    type ContactPin =
        {
            Position: Position
            Label: string
            PinType: PinType
            Address: string
        }