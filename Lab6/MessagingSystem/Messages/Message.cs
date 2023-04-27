using System.Text.Json.Serialization;
using MessagingSystem.Exceptions;

namespace MessagingSystem.Messages;

public abstract class Message : ICloneable
{
    private MessageState _state;

    protected Message(string data, Account sender, IEnumerable<Account> recipients)
    {
        if (string.IsNullOrWhiteSpace(data))
            throw new ArgumentException("Invalid message data", nameof(data));

        if (sender is null)
            throw new ArgumentNullException(nameof(sender), "Invalid sender");

        if (recipients is null || !recipients.Any())
            throw new ArgumentException("Invalid recipients list", nameof(recipients));

        _state = MessageState.New;
        CreationTime = DateTime.Now;

        Data = data;
        Sender = sender;
        Recipients = recipients.ToList<Account>().AsReadOnly();
    }

    public MessageState State
    {
        get => _state;

        set
        {
            if (!Enum.IsDefined(typeof(MessageState), value))
                throw new ArgumentException(nameof(value), "Invalid message state");

            _state = value;
        }
    }

    public string Data { get; }

    public DateTime CreationTime { get; set; }

    public Account Sender { get; }

    [JsonIgnore]
    public IReadOnlyCollection<Account> Recipients { get; }

    public virtual string Read()
    {
        if (!State.Equals(MessageState.Received))
            throw new ForbiddenOperationException("Unrecognized message");

        State = MessageState.Processed;

        return string.Format("Data: {0}", Data);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
