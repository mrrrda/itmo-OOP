using MessagingSystem.Exceptions;
using MessagingSystem.Messages;
using MessagingSystem.MessageSources;
using MessagingSystem.Models;
using MessagingSystem.Reports;
using MessagingSystem.Roles;

namespace MessagingSystem.UserInterface;

public class UserInterface
{
    public static void Main()
    {
        string mainSeparator = "----------------";

        var applicationService = new ApplicationService();

        while (true)
        {
        MainMenu: { }

            Console.WriteLine("Choose an option:\n" +
                "1. Create account\n" +
                "2. Sign in account\n" +
                "3. Save system state\n" +
                "4. Restore system state\n" +
                "5. Exit\n");

            string? option = Console.ReadLine()?.Trim();
            Console.WriteLine(mainSeparator);

            switch (option)
            {
                case "1":
                    Console.WriteLine("Enter your full name:");
                    string? fullNameToSignUp = Console.ReadLine()?.Trim();

                    Console.WriteLine("Enter required role (Boss/Regular):");
                    string? roleToSignUp = Console.ReadLine()?.Trim().ToLower();

                    Role? role = null;
                    switch (roleToSignUp)
                    {
                        case "boss":
                            role = new BossRole();
                            break;

                        case "regular":
                            role = new RegularRole();
                            break;

                        default:
                            break;
                    }

                    if (role is null)
                    {
                        Console.WriteLine("Invalid role");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    Console.WriteLine("Enter login:");
                    string? loginToSignUp = Console.ReadLine()?.Trim();

                    Console.WriteLine("Enter password:");
                    string? passwordToSignUp = Console.ReadLine()?.Trim();

                    if (fullNameToSignUp is null || !Employee.RegexFullName.IsMatch(fullNameToSignUp))
                    {
                        Console.WriteLine("Invalid full name");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    if (loginToSignUp is null || !Account.RegexLogin.IsMatch(loginToSignUp))
                    {
                        Console.WriteLine("Invalid login");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    if (passwordToSignUp is null ||
                        !Account.RegexPassword.IsMatch(passwordToSignUp) ||
                        passwordToSignUp.Length < Account.MinPasswordLength ||
                        passwordToSignUp.Length > Account.MaxPasswordLength)
                    {
                        Console.WriteLine(
                            string.Format("Password must be an alphanumeric string with length from {0} from {1}", Account.MinPasswordLength, Account.MaxPasswordLength));
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    try
                    {
                        applicationService.SignUpUser(fullNameToSignUp, role, loginToSignUp, passwordToSignUp, new MessagingMailBox());
                    }
                    catch (DuplicateAccountException)
                    {
                        Console.WriteLine("Account with requested login already exists");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    Console.WriteLine("Account has been successfully created, sign in to continue");
                    Console.WriteLine(mainSeparator);

                    break;

                case "2":
                    Console.WriteLine("Enter login:");
                    string? loginToSignIn = Console.ReadLine()?.Trim();

                    if (loginToSignIn is null || !Account.RegexLogin.IsMatch(loginToSignIn))
                    {
                        Console.WriteLine("Invalid login");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    if (!applicationService.Accounts.Values.Any(account => account.Login.Equals(loginToSignIn)))
                    {
                        Console.WriteLine("Not existing account");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    Console.WriteLine("Enter password:");
                    string? passwordToSignIn = Console.ReadLine()?.Trim();

                    if (passwordToSignIn is null ||
                        !Account.RegexPassword.IsMatch(passwordToSignIn) ||
                        passwordToSignIn.Length < Account.MinPasswordLength ||
                        passwordToSignIn.Length > Account.MaxPasswordLength)
                    {
                        Console.WriteLine("Invalid password");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    Account account;
                    try
                    {
                        account = applicationService.SignInUser(loginToSignIn, passwordToSignIn);
                    }
                    catch (InvalidPasswordException)
                    {
                        Console.WriteLine("Invalid password");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    Console.WriteLine("You have been successfully logged in");
                    Console.WriteLine(mainSeparator);

                    var accountMailBox = (MessagingMailBox)account.MailBox;

                    while (true)
                    {
                    AccountMenu: { }

                        Console.WriteLine("Choose an option:\n" +
                            "1. Open mailbox\n" +
                            "2. Send message\n" +
                            "3. Read message\n" +
                            "4. Read report\n" +
                            "5. Exit\n");

                        string? optionInAccount = Console.ReadLine()?.Trim();
                        Console.WriteLine(mainSeparator);

                        switch (optionInAccount)
                        {
                            case "1":

                                while (true)
                                {
                                    Console.WriteLine("Choose message source:\n" +
                                        "1. Phone\n" +
                                        "2. Email\n" +
                                        "3. Messenger\n" +
                                        "4. Exit\n");

                                    string? messageSourceToOpenMailBox = Console.ReadLine()?.Trim();
                                    Console.WriteLine(mainSeparator);

                                    switch (messageSourceToOpenMailBox)
                                    {
                                        case "1":
                                            if (!accountMailBox.PhoneMessages.Any())
                                            {
                                                Console.WriteLine("No messages");
                                                Console.WriteLine(mainSeparator);
                                                break;
                                            }

                                            for (int i = 0; i < accountMailBox.PhoneMessages.Count; i++)
                                            {
                                                Console.WriteLine(string.Format("Id: {0}\n", i) +
                                                    string.Format("State: {0}\n", accountMailBox.PhoneMessages[i].State.ToString()) +
                                                    string.Format("Date: {0}\n", accountMailBox.PhoneMessages[i].CreationTime.ToString("M/d/yyyy H:mm")) +
                                                    string.Format("Sender full name: {0}\n", accountMailBox.PhoneMessages[i].Sender.Employee.FullName) +
                                                    mainSeparator);
                                            }

                                            break;

                                        case "2":
                                            if (!accountMailBox.EmailMessages.Any())
                                            {
                                                Console.WriteLine("No messages");
                                                Console.WriteLine(mainSeparator);
                                                break;
                                            }

                                            for (int i = 0; i < accountMailBox.EmailMessages.Count; i++)
                                            {
                                                Console.WriteLine(string.Format("Id: {0}\n", i) +
                                                    string.Format("State: {0}\n", accountMailBox.EmailMessages[i].State.ToString()) +
                                                    string.Format("Date: {0}\n", accountMailBox.EmailMessages[i].CreationTime.ToString("M/d/yyyy H:mm")) +
                                                    string.Format("Sender full name: {0}\n", accountMailBox.EmailMessages[i].Sender.Employee.FullName) +
                                                    mainSeparator);
                                            }

                                            break;

                                        case "3":
                                            if (!accountMailBox.MessengerMessages.Any())
                                            {
                                                Console.WriteLine("No messages");
                                                Console.WriteLine(mainSeparator);
                                                break;
                                            }

                                            for (int i = 0; i < accountMailBox.MessengerMessages.Count; i++)
                                            {
                                                Console.WriteLine(string.Format("Id: {0}\n", i) +
                                                    string.Format("State: {0}\n", accountMailBox.MessengerMessages[i].State.ToString()) +
                                                    string.Format("Date: {0}:\n", accountMailBox.MessengerMessages[i].CreationTime.ToString("M/d/yyyy H:mm")) +
                                                    string.Format("Sender full name: {0}\n", accountMailBox.MessengerMessages[i].Sender.Employee.FullName) +
                                                    mainSeparator);
                                            }

                                            break;

                                        case "4":
                                            goto AccountMenu;

                                        default:
                                            Console.WriteLine("Invalid option");

                                            goto AccountMenu;
                                    }
                                }

                            case "2":
                                var messageDataBuilder = new MessageDataBuilder();

                                messageDataBuilder.SetSender(account);

                                Console.WriteLine("Choose recipients by id:");

                                for (int i = 0; i < applicationService.Accounts.Values.Count; i++)
                                {
                                    if (applicationService.Accounts.Values.ToList<Account>()[i].Equals(account))
                                        continue;

                                    Console.WriteLine(string.Format("Id: {0}, Full name: {1}", i, applicationService.Accounts.Values.ToList<Account>()[i].Employee.FullName));
                                }

                                string? chosenAccountIds = Console.ReadLine()?.Trim();
                                Console.WriteLine(mainSeparator);

                                if (string.IsNullOrEmpty(chosenAccountIds))
                                {
                                    Console.WriteLine("Invalid recipients");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                string[] recipientAccountsIds = chosenAccountIds.Split(" ");

                                int[] accountIds = new int[recipientAccountsIds.Length];
                                for (int i = 0; i < recipientAccountsIds.Length; i++)
                                {
                                    bool isDigitAccountId = int.TryParse(recipientAccountsIds[i], out int id);

                                    if (!isDigitAccountId)
                                    {
                                        Console.WriteLine("Recipient id must be a digit");
                                        Console.WriteLine(mainSeparator);

                                        goto AccountMenu;
                                    }

                                    if (id < 0 || id > applicationService.Accounts.Count)
                                    {
                                        Console.WriteLine("Not existing account id");
                                        Console.WriteLine(mainSeparator);

                                        goto AccountMenu;
                                    }

                                    if (applicationService.Accounts.Values.ToList<Account>()[id].Equals(account))
                                    {
                                        Console.WriteLine("Impossible to set yourself as a recipient");
                                        Console.WriteLine(mainSeparator);

                                        goto AccountMenu;
                                    }

                                    accountIds[i] = id;
                                }

                                var recipients = new List<Account>();

                                foreach (int id in accountIds)
                                    recipients.Add(applicationService.Accounts.Values.ToList<Account>()[id]);

                                messageDataBuilder.SetRecipients(recipients);

                                Console.WriteLine("Choose message source (phone, email, messenger):");

                                string? messageSourceToSendMessage = Console.ReadLine()?.Trim();
                                Console.WriteLine(mainSeparator);

                                MessageSource? messageSource = null;

                                switch (messageSourceToSendMessage)
                                {
                                    case "phone":
                                        messageSource = new Phone();
                                        break;

                                    case "email":
                                        messageSource = new Email();
                                        break;

                                    case "messenger":
                                        messageSource = new Messenger();
                                        break;

                                    default:
                                        Console.WriteLine("Invalid message source");
                                        Console.WriteLine(mainSeparator);

                                        goto AccountMenu;
                                }

                                if (messageSource is null)
                                {
                                    Console.WriteLine("Invalid message source");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                if (messageSource is Email)
                                {
                                    Console.WriteLine("Do you want to set message topic? (y/n)");

                                    string? topicOption = Console.ReadLine()?.Trim();
                                    Console.WriteLine(mainSeparator);

                                    if (topicOption is null || (!topicOption.Equals("y") && !topicOption.Equals("n")))
                                    {
                                        Console.WriteLine("Invalid option");
                                        Console.WriteLine(mainSeparator);

                                        goto AccountMenu;
                                    }

                                    Console.WriteLine("Enter topic:");

                                    string? topic = Console.ReadLine()?.Trim();

                                    if (string.IsNullOrEmpty(topic))
                                    {
                                        Console.WriteLine("Invalid message topic");
                                        Console.WriteLine(mainSeparator);

                                        goto AccountMenu;
                                    }

                                    messageDataBuilder.SetTopic(topic);
                                }

                                Console.WriteLine("Enter message data:");

                                string? data = Console.ReadLine();
                                Console.WriteLine(mainSeparator);

                                if (string.IsNullOrEmpty(data))
                                {
                                    Console.WriteLine("Invalid message data");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                messageDataBuilder.SetData(data);

                                MessageData messageData = messageDataBuilder.Build();

                                account.SendMessage(messageSource, messageData);

                                Console.WriteLine("Message has been successfully delivered");
                                Console.WriteLine(mainSeparator);

                                break;

                            case "3":
                                if (!account.MailBox.Messages.Any(message => message.State.Equals(MessageState.Received)))
                                {
                                    Console.WriteLine("No new messages");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                Console.WriteLine("Choose message to read by id:");

                                var messageIds = new List<int>();

                                for (int i = 0; i < account.MailBox.Messages.Count && account.MailBox.Messages[i].State.Equals(MessageState.Received); i++)
                                {
                                    Console.WriteLine(string.Format("Id: {0}\n", i) +
                                        string.Format("Date: {0}\n", account.MailBox.Messages[i].CreationTime.ToString("M/d/yyyy H:mm")) +
                                        string.Format("Sender full name: {0}\n", account.MailBox.Messages[i].Sender.Employee.FullName));

                                    messageIds.Add(i);
                                }

                                string? chosenMessageId = Console.ReadLine()?.Trim();

                                if (string.IsNullOrEmpty(chosenMessageId) || chosenMessageId.Length > 1)
                                {
                                    Console.WriteLine("Invalid message id");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                bool isDigitMessageId = int.TryParse(chosenMessageId, out int messageId);

                                if (!isDigitMessageId)
                                {
                                    Console.WriteLine("Invalid message id");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                if (!messageIds.Contains(messageId))
                                {
                                    Console.WriteLine("Invalid message id");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                Console.WriteLine(account.ReadMessage(account.MailBox.Messages[messageId]));
                                Console.WriteLine(mainSeparator);

                                break;

                            case "4":
                                if (account.Role is not BossRole)
                                {
                                    Console.WriteLine("You do not have permission to read report");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                Console.WriteLine("Enter date from:");
                                string? chosenDateFrom = Console.ReadLine()?.Trim();

                                if (chosenDateFrom is null)
                                {
                                    Console.WriteLine("Invalid date from");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                bool isValidDateFrom = DateTime.TryParse(chosenDateFrom, out DateTime dateFrom);

                                if (!isValidDateFrom)
                                {
                                    Console.WriteLine("Invalid date from");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                if (DateTime.Compare(dateFrom, DateTime.Now) > 0)
                                {
                                    Console.WriteLine("Date from cannot be greater than current date");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                Console.WriteLine(mainSeparator);

                                Console.WriteLine("Enter date to:");
                                string? chosenDateTo = Console.ReadLine()?.Trim();

                                if (chosenDateTo is null)
                                {
                                    Console.WriteLine("Invalid date to");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                bool isValidDateTo = DateTime.TryParse(chosenDateTo, out DateTime dateTo);

                                if (!isValidDateTo)
                                {
                                    Console.WriteLine("Invalid date to");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                if (DateTime.Compare(dateTo, DateTime.Now) > 0)
                                {
                                    Console.WriteLine("Date to cannot be greater than current date");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                if (DateTime.Compare(dateFrom, dateTo) > 0)
                                {
                                    Console.WriteLine("Date from cannot be greater than date to");
                                    Console.WriteLine(mainSeparator);

                                    goto AccountMenu;
                                }

                                Console.WriteLine(mainSeparator);

                                Console.WriteLine("Requested report\n" +
                                    account.ReadReport(new MessageReportManager(), dateFrom, dateTo));

                                Console.WriteLine(mainSeparator);

                                break;

                            case "5":
                                goto MainMenu;

                            default:
                                Console.WriteLine("Invalid option");
                                goto AccountMenu;
                        }
                    }

                case "3":
                    Console.WriteLine("Enter configuration file path:");

                    string? pathToSave = Console.ReadLine()?.Trim();

                    if (pathToSave is null)
                    {
                        Console.WriteLine("Invalid path");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    try
                    {
                        new FileInfo(pathToSave);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid path");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    MessagingSystemConfiguration.ParseTo(applicationService, pathToSave);

                    Console.WriteLine("System has been successfully saved");
                    Console.WriteLine(mainSeparator);

                    break;

                case "4":
                    Console.WriteLine("Enter configuration file path:");

                    string? pathToRestore = Console.ReadLine()?.Trim();

                    if (pathToRestore is null)
                    {
                        Console.WriteLine("Invalid path");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    try
                    {
                        new FileInfo(pathToRestore);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid path");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    try
                    {
                        applicationService = MessagingSystemConfiguration.ParseFrom(pathToRestore);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to restore system");
                        Console.WriteLine(mainSeparator);
                        break;
                    }

                    Console.WriteLine("System has been successfully restored");
                    Console.WriteLine(mainSeparator);

                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine("Invalid option");
                    Console.WriteLine(mainSeparator);
                    break;
            }
        }
    }
}
