namespace MessagingSystem.Messages;

public class PhoneMessage : Message
{
    public PhoneMessage(string data, Account sender, IEnumerable<Account> recipients)
        : base(data, sender, recipients)
    { }
}
