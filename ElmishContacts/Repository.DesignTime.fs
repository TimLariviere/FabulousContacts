namespace ElmishContacts

#if DISABLE_LIVEUPDATE

open Models

module Repository =
    type ContactObject() =
        member val Id = 0 with get, set
        member val FirstName = "" with get, set
        member val LastName = "" with get, set
        member val Email = "" with get, set
        member val Phone = "" with get, set
        member val Address = "" with get, set
        member val IsFavorite = false with get, set
        member val Picture: byte array = null with get, set

    let convertToObject (item: Contact) =
        let obj = ContactObject()
        obj.Id <- item.Id
        obj.FirstName <- item.FirstName
        obj.LastName <- item.LastName
        obj.Email <- item.Email
        obj.Phone <- item.Phone
        obj.Address <- item.Address
        obj.IsFavorite <- item.IsFavorite
        obj.Picture <- item.Picture
        obj

    let convertToModel (obj: ContactObject) : Contact =
        { Id = obj.Id
          FirstName = obj.FirstName
          LastName = obj.LastName
          Email = obj.Email
          Phone = obj.Phone
          Address = obj.Address
          IsFavorite = obj.IsFavorite
          Picture = obj.Picture }

    let loadAllContacts dbPath = async {
        return [
            { Id = 1
              FirstName = "Roberts"
              LastName = "The Pirate"
              Email = "pirate.robert@lab.com"
              Phone = ""
              Address = ""
              IsFavorite = true
              Picture = [||] }
        ]
    }

    let insertContact dbPath contact = async {
        return { contact with Id = 2 }
    }

    let updateContact dbPath contact = async {
        return contact
    }

    let deleteContact dbPath contact = async {
        return ()
    }

#endif