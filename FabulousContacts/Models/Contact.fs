namespace FabulousContacts.Models

type Contact =
    { Id: int
      FirstName: string
      LastName: string
      Email: string
      Phone: string
      Address: string
      IsFavorite: bool
      Picture: byte array option }