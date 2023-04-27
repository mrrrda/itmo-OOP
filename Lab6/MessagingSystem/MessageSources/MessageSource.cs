using MessagingSystem.Messages;

namespace MessagingSystem.MessageSources;

public abstract class MessageSource
{
    public abstract void SendMessage(MessageData messageData);
}
