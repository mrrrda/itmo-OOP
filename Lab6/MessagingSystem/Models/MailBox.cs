using System.Collections.ObjectModel;

using MessagingSystem.Messages;

namespace MessagingSystem.Models;

public abstract class MailBox
{
    protected MailBox()
    {
        Messages = new List<Message>();
    }

    public List<Message> Messages { get; set; }

    public abstract void AddMessage(Message newMessage);
    public abstract void RemoveMessage(Message messageToRemove);
}
