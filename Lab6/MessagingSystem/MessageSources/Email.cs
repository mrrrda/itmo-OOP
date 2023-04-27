using MessagingSystem.Messages;

namespace MessagingSystem.MessageSources;

public class Email : MessageSource
{
    public override void SendMessage(MessageData messageData)
    {
        if (messageData is null)
            throw new ArgumentNullException(nameof(messageData), "Invalid message data");

        EmailMessage message = CreateMessage(messageData);

        foreach (Account recipient in message.Recipients)
            recipient.AddMessage((Message)message.Clone());
    }

    private EmailMessage CreateMessage(MessageData messageData)
    {
        var emailMessage = new EmailMessage(messageData.Data, messageData.Sender, messageData.Recipients);

        if (messageData.Topic is not null)
            emailMessage.Topic = messageData.Topic;

        return emailMessage;
    }
}
