using MessagingSystem.Messages;

namespace MessagingSystem.MessageSources;

public class Phone : MessageSource
{
    public override void SendMessage(MessageData messageData)
    {
        if (messageData is null)
            throw new ArgumentNullException(nameof(messageData), "Invalid message data");

        PhoneMessage message = CreateMessage(messageData);

        foreach (Account recipient in message.Recipients)
            recipient.AddMessage((Message)message.Clone());
    }

    private PhoneMessage CreateMessage(MessageData messageData)
    {
        return new PhoneMessage(messageData.Data, messageData.Sender, messageData.Recipients);
    }
}
