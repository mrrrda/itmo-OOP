using Banks.BankActors;

namespace Banks.NotificationServices;

public class ConsoleNotificationService : NotificationService
{
    public override void Notify(Client client, string notification)
    {
        Console.WriteLine(notification);
    }
}
