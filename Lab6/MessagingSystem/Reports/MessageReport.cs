using System.Text;

namespace MessagingSystem.Reports;

public class MessageReport : Report
{
    private int _processedMessagesCount;

    private int _phoneReceivedMessagesCount;
    private int _emailReceivedMessagesCount;
    private int _messengerReceivedMessagesCount;

    private int _overallMessageCount;

    public MessageReport(
        int processedMessagesCount,
        int phoneReceivedMessagesCount,
        int emailReceivedMessagesCount,
        int messengerReceivedMessagesCount,
        int overallMessageCount)
    {
        if (processedMessagesCount < 0)
            throw new ArgumentOutOfRangeException(nameof(processedMessagesCount), "Messages count must be positive");

        if (phoneReceivedMessagesCount < 0)
            throw new ArgumentOutOfRangeException(nameof(phoneReceivedMessagesCount), "Messages count must be positive");

        if (emailReceivedMessagesCount < 0)
            throw new ArgumentOutOfRangeException(nameof(emailReceivedMessagesCount), "Messages count must be positive");

        if (messengerReceivedMessagesCount < 0)
            throw new ArgumentOutOfRangeException(nameof(messengerReceivedMessagesCount), "Messages count must be positive");

        if (overallMessageCount < 0)
            throw new ArgumentOutOfRangeException(nameof(overallMessageCount), "Messages count must be positive");

        _processedMessagesCount = processedMessagesCount;

        _phoneReceivedMessagesCount = phoneReceivedMessagesCount;
        _emailReceivedMessagesCount = emailReceivedMessagesCount;
        _messengerReceivedMessagesCount = messengerReceivedMessagesCount;

        _overallMessageCount = overallMessageCount;
    }

    public override string Format()
    {
        return new StringBuilder()
            .Append(string.Format("Processed messages: {0}\n", _processedMessagesCount))
            .Append(string.Format("Phone received messages: {0}\n", _phoneReceivedMessagesCount))
            .Append(string.Format("Email received messages: {0}\n", _emailReceivedMessagesCount))
            .Append(string.Format("Messenger received messages: {0}\n", _messengerReceivedMessagesCount))
            .Append(string.Format("Overall received messages: {0}", _overallMessageCount))
            .ToString();
    }
}
