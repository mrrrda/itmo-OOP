using MessagingSystem.Exceptions;
using MessagingSystem.Messages;
using MessagingSystem.Models;
using MessagingSystem.Roles;

namespace MessagingSystem.Reports;

public class MessageReportManager : ReportManager
{
    public override Report CreateReport(Account account, DateTime dateFrom, DateTime dateTo)
    {
        if (account is null)
            throw new ArgumentNullException(nameof(account), "Invalid account");

        if (account.Role is not BossRole)
            throw new ForbiddenOperationException("Requested operation is not allowed");

        int processedMessagesCount = 0;

        int phoneReceivedMessagesCount = 0;
        int emailReceivedMessagesCount = 0;
        int messengerReceivedMessagesCount = 0;

        int overallMessageCount = 0;

        foreach (Employee subordinate in account.Employee.Subordinates)
        {
            if (subordinate.Account.MailBox is not MessagingMailBox messagingMailBox)
                throw new ArgumentException("Unsupported report manager for requested account mail box");

            foreach (Message message in messagingMailBox.PhoneMessages)
            {
                if (DateTime.Compare(message.CreationTime, dateFrom) < 0 || DateTime.Compare(message.CreationTime, dateTo) > 0)
                    continue;

                if (message.State.Equals(MessageState.Received))
                    phoneReceivedMessagesCount++;

                if (message.State.Equals(MessageState.Processed))
                    processedMessagesCount++;

                overallMessageCount++;
            }

            foreach (Message message in messagingMailBox.EmailMessages)
            {
                if (message.State.Equals(MessageState.Received))
                    emailReceivedMessagesCount++;

                if (message.State.Equals(MessageState.Processed))
                    processedMessagesCount++;

                overallMessageCount++;
            }

            foreach (Message message in messagingMailBox.MessengerMessages)
            {
                if (message.State.Equals(MessageState.Received))
                    messengerReceivedMessagesCount++;

                if (message.State.Equals(MessageState.Processed))
                    processedMessagesCount++;

                overallMessageCount++;
            }
        }

        return new MessageReport(
            processedMessagesCount,
            phoneReceivedMessagesCount,
            emailReceivedMessagesCount,
            messengerReceivedMessagesCount,
            overallMessageCount);
    }
}
