using System.Collections.ObjectModel;

namespace MessagingSystem.Messages;

public class MessageData
{
    public MessageData(string? topic, string data, Account sender, IEnumerable<Account> recipients)
    {
        Topic = topic;
        Data = data;
        Sender = sender;
        Recipients = recipients.ToList<Account>().AsReadOnly();
    }

    public string? Topic { get; }
    public string Data { get; }
    public Account Sender { get; }
    public ReadOnlyCollection<Account> Recipients { get; }
}
