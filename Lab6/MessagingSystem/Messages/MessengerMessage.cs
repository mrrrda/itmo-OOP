namespace MessagingSystem.Messages;

public class MessengerMessage : Message
{
    public MessengerMessage(string data, Account sender, IEnumerable<Account> recipients)
        : base(data, sender, recipients)
    { }
}
