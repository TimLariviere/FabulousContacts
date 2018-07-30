namespace ElmishContacts

open SQLite

module Repository =
    open Models

    type ContactObject() =
        [<PrimaryKey>][<AutoIncrement>]
        member val Id = 0 with get, set
        member val Picture = "" with get, set
        member val FirstName = "" with get, set
        member val LastName = "" with get, set
        member val Address = "" with get, set
        member val IsFavorite = false with get, set

    let convertToObject (item: Contact) =
        let obj = ContactObject()
        obj.Id <- item.Id
        obj.Picture <- item.Picture
        obj.FirstName <- item.FirstName
        obj.LastName <- item.LastName
        obj.Address <- item.Address
        obj.IsFavorite <- item.IsFavorite
        obj

    let convertToModel (obj: ContactObject) : Contact =
        {
            Id = obj.Id
            Picture = obj.Picture
            FirstName = obj.FirstName
            LastName = obj.LastName
            Address = obj.Address
            IsFavorite = obj.IsFavorite
        }

    let connect dbPath = async {
        let db = new SQLiteAsyncConnection(dbPath)
        do! db.CreateTableAsync<ContactObject>() |> Async.AwaitTask |> Async.Ignore
        return db
    }

    let loadAllContacts dbPath = async {
        let! database = connect dbPath
        let! objs = database.Table<ContactObject>().ToListAsync() |> Async.AwaitTask
        return objs |> Seq.toList |> List.map convertToModel
    }

    let insertContact dbPath contact = async {
        let! database = connect dbPath
        let obj = convertToObject contact
        do! database.InsertAsync(obj) |> Async.AwaitTask |> Async.Ignore
        let! rowIdObj = database.ExecuteScalarAsync("select last_insert_rowid()", [||]) |> Async.AwaitTask
        let rowId = rowIdObj |> int
        return { contact with Id = rowId }
    }

    let updateContact dbPath contact = async {
        let! database = connect dbPath
        let obj = convertToObject contact
        do! database.UpdateAsync(obj) |> Async.AwaitTask |> Async.Ignore
        return contact
    }

    let deleteContact dbPath contact = async {
        let! database = connect dbPath
        let obj = convertToObject contact
        do! database.DeleteAsync(obj) |> Async.AwaitTask |> Async.Ignore
    }