namespace ElmishContacts

module Models =
    open Xamarin.Forms.Maps

    type Contact =
        {
            Id: int
            FirstName: string
            LastName: string
            Email: string
            Phone: string
            Address: string
            IsFavorite: bool
            Picture: byte array
        }

    type ContactPin =
        {
            Position: Position
            Label: string
            PinType: PinType
            Address: string
        }