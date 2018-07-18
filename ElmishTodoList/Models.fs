namespace ElmishTodoList

module Models =
    type Contact =
        {
            Id: int
            Name: string
            IsFavorite: bool
        }
    with static member NewContact = { Id = 0; Name = ""; IsFavorite = false }