namespace MessagingSystem.Messages;

public class MessageDataBuilder
{
    private string? _data;
    private string? _topic;
    private Account? sender;
    private IEnumerable<Account>? recipients;

    public MessageData Build()
    {
        if (_data is null)
            throw new ArgumentNullException(nameof(_data), "Invalid data");

        if (sender is null)
            throw new ArgumentNullException(nameof(sender), "Invalid sender");

        if (recipients is null || !recipients.Any())
            throw new ArgumentException(nameof(recipients), "Invalid recipients");

        return new MessageData(_topic, _data, sender, recipients);
    }

    public MessageDataBuilder SetTopic(string topic)
    {
        if (string.IsNullOrEmpty(topic))
            throw new ArgumentException("Invalid topic", nameof(topic));

        _topic = topic;

        return this;
    }

    public MessageDataBuilder SetData(string data)
    {
        if (string.IsNullOrEmpty(data))
            throw new ArgumentException("Invalid data", nameof(data));

        _data = data;

        return this;
    }

    public MessageDataBuilder SetSender(Account sender)
    {
        if (sender is null)
            throw new ArgumentNullException(nameof(sender), "Invalid sender");

        this.sender = sender;

        return this;
    }

    public MessageDataBuilder SetRecipients(IEnumerable<Account> recipients)
    {
        if (recipients is null || !recipients.Any())
            throw new ArgumentException(nameof(recipients), "Invalid recipients");

        this.recipients = recipients;

        return this;
    }
}
