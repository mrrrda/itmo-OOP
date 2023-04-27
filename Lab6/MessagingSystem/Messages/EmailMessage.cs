using System.Text;

namespace MessagingSystem.Messages;

public class EmailMessage : Message
{
    private string? _topic;

    public EmailMessage(string data, Account sender, IEnumerable<Account> recipients)
        : base(data, sender, recipients)
    { }

    public string? Topic
    {
        get => _topic;

        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid topic", nameof(value));

            _topic = value;
        }
    }

    public override string Read()
    {
        return new StringBuilder()
            .Append(Topic is null ? string.Empty : string.Format("Topic: {0}\n", Topic))
            .Append(string.Format("Data: {0}", Data))
            .ToString();
    }
}
