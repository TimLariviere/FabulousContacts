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
    with static member NewContact = { Id = 0; Name = ""; Address = ""; IsFavorite = false }

    type ContactPin =
        {
            Position: Position
            Label: string
            PinType: PinType
            Address: string
        }