using MessagingSystem.Models;
using MessagingSystem.Roles;

namespace MessagingSystem;

public abstract class AuthenticationService
{
    public abstract Account SignUp(
        Dictionary<string, Account> accounts,
        string fullName,
        Role role,
        string login,
        string password,
        MailBox mailBox);

    public abstract Account SignIn(Dictionary<string, Account> accounts, string login, string password);
}
