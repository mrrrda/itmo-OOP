using MessagingSystem.Messages;

namespace MessagingSystem.MessageSources;

public class Messenger : MessageSource
{
    public override void SendMessage(MessageData messageData)
    {
        if (messageData is null)
            throw new ArgumentNullException(nameof(messageData), "Invalid message data");

        MessengerMessage message = CreateMessage(messageData);

        foreach (Account recipient in message.Recipients)
            recipient.AddMessage((Message)message.Clone());
    }

    private MessengerMessage CreateMessage(MessageData messageData)
    {
        return new MessengerMessage(messageData.Data, messageData.Sender, messageData.Recipients);
    }
}
