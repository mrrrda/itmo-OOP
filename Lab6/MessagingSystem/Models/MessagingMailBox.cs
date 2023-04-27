using System.Collections.ObjectModel;

using MessagingSystem.Exceptions;
using MessagingSystem.Messages;

namespace MessagingSystem.Models;

public class MessagingMailBox : MailBox
{
    private List<PhoneMessage> phoneMessages;
    private List<EmailMessage> emailMessages;
    private List<MessengerMessage> messengerMessages;

    public MessagingMailBox()
        : base()
    {
        phoneMessages = new List<PhoneMessage>();
        emailMessages = new List<EmailMessage>();
        messengerMessages = new List<MessengerMessage>();
    }

    public ReadOnlyCollection<PhoneMessage> PhoneMessages => phoneMessages.AsReadOnly();
    public ReadOnlyCollection<EmailMessage> EmailMessages => emailMessages.AsReadOnly();
    public ReadOnlyCollection<MessengerMessage> MessengerMessages => messengerMessages.AsReadOnly();

    public override void AddMessage(Message newMessage)
    {
        switch (newMessage)
        {
            case PhoneMessage:

                var newPhoneMessage = (PhoneMessage)newMessage;

                if (phoneMessages.Contains(newPhoneMessage))
                    throw new DuplicateMessageException();

                phoneMessages.Add(newPhoneMessage);
                Messages.Add(newPhoneMessage);

                break;

            case EmailMessage:

                var newEmailMessage = (EmailMessage)newMessage;

                if (emailMessages.Contains(newEmailMessage))
                    throw new DuplicateMessageException();

                emailMessages.Add(newEmailMessage);
                Messages.Add(newEmailMessage);

                break;

            case MessengerMessage:

                var newMessengerMessage = (MessengerMessage)newMessage;

                if (messengerMessages.Contains(newMessengerMessage))
                    throw new DuplicateMessageException();

                messengerMessages.Add(newMessengerMessage);
                Messages.Add(newMessengerMessage);

                break;

            default:
                throw new ArgumentException(nameof(newMessage), "Unsupported message source");
        }
    }

    public override void RemoveMessage(Message messageToRemove)
    {
        switch (messageToRemove)
        {
            case PhoneMessage:

                var phoneMessageToRemove = (PhoneMessage)messageToRemove;

                if (!phoneMessages.Contains(phoneMessageToRemove))
                    throw new NotExistingMessageException();

                phoneMessages.Remove(phoneMessageToRemove);
                Messages.Remove(phoneMessageToRemove);

                break;

            case EmailMessage:

                var emailMessageToRemove = (EmailMessage)messageToRemove;

                if (!emailMessages.Contains(emailMessageToRemove))
                    throw new NotExistingMessageException();

                emailMessages.Remove(emailMessageToRemove);
                Messages.Remove(emailMessageToRemove);

                break;

            case MessengerMessage:

                var messengerMessageToRemove = (MessengerMessage)messageToRemove;

                if (!messengerMessages.Contains(messengerMessageToRemove))
                    throw new NotExistingMessageException();

                messengerMessages.Remove(messengerMessageToRemove);
                Messages.Remove(messengerMessageToRemove);

                break;

            default:
                throw new ArgumentException(nameof(messageToRemove), "Unsupported message source");
        }
    }
}
