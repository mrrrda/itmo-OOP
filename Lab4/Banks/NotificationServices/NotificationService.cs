using Banks.BankActors;

namespace Banks.NotificationServices;

public abstract class NotificationService
{
    public abstract void Notify(Client client, string notification);
}
