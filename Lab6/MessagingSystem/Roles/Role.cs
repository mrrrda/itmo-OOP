using MessagingSystem.Messages;
using MessagingSystem.MessageSources;
using MessagingSystem.Reports;

namespace MessagingSystem.Roles;

public abstract class Role
{
    public virtual void SendMessage(MessageSource messageSource, MessageData messageData)
    {
        messageSource.SendMessage(messageData);
    }

    public virtual string ReadMessage(Message message)
    {
        return message.Read();
    }

    public abstract string ReadReport(ReportManager reportManager, Account account, DateTime dateFrom, DateTime dateTo);
}
