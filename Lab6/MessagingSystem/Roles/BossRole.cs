using MessagingSystem.Messages;
using MessagingSystem.MessageSources;
using MessagingSystem.Reports;

namespace MessagingSystem.Roles;

public class BossRole : Role
{
    public override void SendMessage(MessageSource messageSource, MessageData messageData)
    {
        base.SendMessage(messageSource, messageData);
    }

    public override string ReadMessage(Message message)
    {
        return base.ReadMessage(message);
    }

    public override string ReadReport(ReportManager reportManager, Account account, DateTime dateFrom, DateTime dateTo)
    {
        return reportManager.CreateReport(account, dateFrom, dateTo).Format();
    }
}
