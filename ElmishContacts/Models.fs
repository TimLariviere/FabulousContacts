namespace ElmishContacts

module Models =
    open Xamarin.Forms.Maps

    type Contact =
        {
            Id: int
            Name: string
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