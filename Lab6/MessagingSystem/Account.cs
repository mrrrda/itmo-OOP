using System.Text.RegularExpressions;

using MessagingSystem.Exceptions;
using MessagingSystem.Messages;
using MessagingSystem.MessageSources;
using MessagingSystem.Models;
using MessagingSystem.Reports;
using MessagingSystem.Roles;

namespace MessagingSystem;

public class Account
{
    public static readonly Regex RegexLogin = new Regex("^([a-zA-Zа-яА-Я\\d]+[@\\.]?)+$", RegexOptions.Compiled);

    public static readonly Regex RegexPassword = new Regex("^([a-zA-Zа-яА-Я\\d]+[\\s-]?)+$", RegexOptions.Compiled);
    public static readonly int MinPasswordLength = 8;
    public static readonly int MaxPasswordLength = 32;

    private MailBox mailBox;

    public Account(
        int id,
        string employeeFullName,
        Role role,
        string login,
        string password,
        MailBox mailBox)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be a positive number");

        if (string.IsNullOrWhiteSpace(employeeFullName))
            throw new ArgumentException("Invalid employee full name", nameof(employeeFullName));

        if (role is null)
            throw new ArgumentNullException(nameof(role), "Invalid role");

        if (!RegexLogin.IsMatch(login))
            throw new ArgumentException("Login name must be an alphabetic string", nameof(login));

        if (
            string.IsNullOrWhiteSpace(password) ||
            !RegexPassword.IsMatch(password) ||
            password.Length < MinPasswordLength ||
            password.Length > MaxPasswordLength)
        {
            throw new ArgumentException(
                string.Format("Password must be an alphanumeric string with length from {0} from {1}", MinPasswordLength, MaxPasswordLength),
                nameof(password));
        }

        if (mailBox is null)
            throw new ArgumentNullException(nameof(mailBox), "Invalid mail box");

        if (mailBox.Messages.Any())
            throw new ArgumentException(nameof(mailBox), "Initial mail box must be empty");

        this.mailBox = mailBox;
        Employee = new Employee(id, employeeFullName, this);

        Id = id;

        Login = login;
        Password = password;

        Role = role;
    }

    public int Id { get; set; }

    public Role Role { get; set; }

    public Employee Employee { get; set; }

    public MailBox MailBox
    {
        get
        {
            foreach (Message message in mailBox.Messages)
            {
                if (message.State.Equals(MessageState.New))
                    message.State = MessageState.Received;
            }

            return mailBox;
        }

        set => mailBox = value;
    }

    public string Login { get; set; }
    public string Password { get; set; }

    public void SendMessage(MessageSource messageSource, MessageData messageData)
    {
        if (messageSource is null)
            throw new ArgumentNullException(nameof(messageSource), "Invalid message source");

        if (messageData is null)
            throw new ArgumentNullException(nameof(messageData), "Invalid message data");

        if (!messageData.Sender.Equals(this))
            throw new ForbiddenOperationException("Invalid message sender");

        if (messageData.Recipients.Contains(this))
            throw new ForbiddenOperationException("Message recipient must be different from the sender");

        Role.SendMessage(messageSource, messageData);
    }

    public string ReadMessage(Message message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message), "Invalid message");

        if (!message.Recipients.Contains(this))
            throw new ForbiddenOperationException("Unrecognized message");

        return Role.ReadMessage(message);
    }

    public string ReadReport(ReportManager reportManager, DateTime dateFrom, DateTime dateTo)
    {
        if (reportManager is null)
            throw new ArgumentNullException(nameof(reportManager), "Invalid report manager");

        if (DateTime.Compare(dateFrom, dateTo) >= 0)
            throw new ArgumentException("Date to must be greater than date from");

        if (DateTime.Compare(dateTo, DateTime.Now) >= 0)
            throw new ArgumentException("Date to must not be greater than current date");

        return Role.ReadReport(reportManager, this, dateFrom, dateTo);
    }

    internal void AddMessage(Message newMessage)
    {
        if (newMessage is null)
            throw new ArgumentNullException(nameof(newMessage), "Invalid message");

        MailBox.AddMessage(newMessage);
    }

    internal void RemoveMessage(Message messageToRemove)
    {
        if (messageToRemove is null)
            throw new ArgumentNullException(nameof(messageToRemove), "Invalid message");

        MailBox.RemoveMessage(messageToRemove);
    }
}
