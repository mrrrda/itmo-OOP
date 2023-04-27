using MessagingSystem.Exceptions;
using MessagingSystem.Messages;
using MessagingSystem.MessageSources;
using MessagingSystem.Models;
using MessagingSystem.Reports;
using MessagingSystem.Roles;

using Xunit;

namespace MessagingSystem.Test;

public class MessagingSystemTest
{
    private ApplicationService applicationService;

    public MessagingSystemTest()
    {
        applicationService = new ApplicationService(new MessagingAuthenticationService());
    }

    [Fact]
    public void SignUpUser_UserRegistered()
    {
        Account newAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Assert.Single(applicationService.Accounts);
        Assert.Contains(newAccount, applicationService.Accounts.Values);
    }

    [Fact]
    public void SignInUser_UserAuthenticated()
    {
        Account newAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account account = applicationService.SignInUser("mariyordab@gmail.com", "mariya12345");

        Assert.Equal(newAccount, account);

    }

    [Fact]
    public void SignInUser_ThrowsNotExistingUserException()
    {
        Assert.Throws<NotExistingAccountException>(() => applicationService.SignInUser("mariyordab@gmail.com", "mariya12345"));

        applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Assert.Throws<NotExistingAccountException>(() => applicationService.SignInUser("mariyorda@gmail.com", "mariya12345"));
    }

    [Fact]
    public void SignInUser_ThrowsInvalidPasswordException()
    {
        applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Assert.Throws<InvalidPasswordException>(() => applicationService.SignInUser("mariyordab@gmail.com", "mariya1234"));
    }

    [Fact]
    public void SendMessage_MessageDelievered()
    {
        Account firstAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account secondAccount = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox());

        var recipients = new List<Account>() { secondAccount };

        MessageData firstMessageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();
        MessageData secondMessageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();
        MessageData thirdMessageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();

        firstAccount.SendMessage(new Phone(), firstMessageData);
        firstAccount.SendMessage(new Email(), secondMessageData);
        firstAccount.SendMessage(new Messenger(), thirdMessageData);

        var secondAccountMailBox = (MessagingMailBox)secondAccount.MailBox;

        Assert.Single(secondAccountMailBox.PhoneMessages);
        Assert.Equal(firstMessageData.Data, secondAccountMailBox.PhoneMessages[0].Data);
        Assert.Equal(MessageState.Received, secondAccountMailBox.PhoneMessages[0].State);

        Assert.Single(secondAccountMailBox.EmailMessages);
        Assert.Equal(secondMessageData.Data, secondAccountMailBox.EmailMessages[0].Data);
        Assert.Equal(MessageState.Received, secondAccountMailBox.EmailMessages[0].State);

        Assert.Single(secondAccountMailBox.MessengerMessages);
        Assert.Equal(thirdMessageData.Data, secondAccountMailBox.MessengerMessages[0].Data);
        Assert.Equal(MessageState.Received, secondAccountMailBox.MessengerMessages[0].State);

        Assert.Equal(3, secondAccount.MailBox.Messages.Count);
    }

    [Fact]
    public void SendMessage_ThrowsInvalidSenderException()
    {
        Account firstAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account secondAccount = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox());

        var recipients = new List<Account>() { secondAccount };

        MessageData messageData = new MessageDataBuilder().SetData("Hi!").SetSender(secondAccount).SetRecipients(recipients).Build();

        Assert.Throws<ForbiddenOperationException>(() => firstAccount.SendMessage(new Phone(), messageData));
    }

    [Fact]
    public void SendMessage_ThrowsInvalidRecipientException()
    {
        Account firstAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account secondAccount = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox());

        var recipients = new List<Account>() { firstAccount, secondAccount };

        MessageData messageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();

        Assert.Throws<ForbiddenOperationException>(() => firstAccount.SendMessage(new Phone(), messageData));
    }

    [Fact]
    public void ReadMessage_MessageIsRead()
    {
        Account firstAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account secondAccount = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox());

        var recipients = new List<Account>() { secondAccount };

        MessageData messageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();

        firstAccount.SendMessage(new Phone(), messageData);

        var secondAccountMailBox = (MessagingMailBox)secondAccount.MailBox;

        secondAccount.ReadMessage(secondAccountMailBox.PhoneMessages[0]);

        Assert.Equal(MessageState.Processed, secondAccountMailBox.PhoneMessages[0].State);
    }

    [Fact]
    public void ReadMessage_ThrowsForbiddenOperationException()
    {
        Account firstAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account secondAccount = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox());

        var recipients = new List<Account>() { secondAccount };

        MessageData messageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();

        firstAccount.SendMessage(new Phone(), messageData);

        var secondAccountMailBox = (MessagingMailBox)secondAccount.MailBox;

        Assert.Throws<ForbiddenOperationException>(() => firstAccount.ReadMessage(secondAccountMailBox.PhoneMessages[0]));
    }

    [Fact]
    public void ReadMessage_ThrowsUnrecognizedMessageException()
    {
        Account firstAccount = applicationService.SignUpUser(
            "Mariya Izerakova",
            new BossRole(),
            "mariyordab@gmail.com",
            "mariya12345",
            new MessagingMailBox());

        Account secondAccount = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox());

        var recipients = new List<Account>() { secondAccount };

        MessageData messageData = new MessageDataBuilder().SetData("Hi!").SetSender(firstAccount).SetRecipients(recipients).Build();

        firstAccount.SendMessage(new Phone(), messageData);

        var secondAccountMailBox = (MessagingMailBox)secondAccount.MailBox;

        secondAccount.ReadMessage(secondAccountMailBox.PhoneMessages[0]);

        Assert.Throws<ForbiddenOperationException>(() => secondAccount.ReadMessage(secondAccountMailBox.PhoneMessages[0]));
    }

    [Fact]
    public void ReadReport_ThrowsForbiddenOperationException()
    {
        Account account = applicationService.SignUpUser(
           "Mariya Izerakova",
           new RegularRole(),
           "mariyordab@gmail.com",
           "mariya12345",
           new MessagingMailBox());

        Assert.Throws<ForbiddenOperationException>(() =>
            account.ReadReport(new MessageReportManager(), new DateTime(2022, 10, 14), new DateTime(2022, 11, 14)));
    }

    [Fact]
    public void AssignSuperior_ThrowsInvalidSuperiourException()
    {
        Employee firstEmployee = applicationService.SignUpUser(
           "Mariya Izerakova",
           new BossRole(),
           "mariyordab@gmail.com",
           "mariya12345",
           new MessagingMailBox()).Employee;

        Employee secondEmployee = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox()).Employee;

        firstEmployee.AddSubordinate(secondEmployee);

        Assert.Throws<InvalidSuperiorException>(() => firstEmployee.Superior = secondEmployee);
    }

    [Fact]
    public void AddSubordinate_ThrowsInvalidSubordinateException()
    {
        Employee firstEmployee = applicationService.SignUpUser(
           "Mariya Izerakova",
           new BossRole(),
           "mariyordab@gmail.com",
           "mariya12345",
           new MessagingMailBox()).Employee;

        Employee secondEmployee = applicationService.SignUpUser(
            "Nikita Hoffman",
            new BossRole(),
            "hoffmanmyst@gmail.com",
            "nikita12345",
            new MessagingMailBox()).Employee;
        firstEmployee.Superior = secondEmployee;

        Assert.Throws<InvalidSubordinateException>(() => firstEmployee.AddSubordinate(secondEmployee));
    }
}
