using MessagingSystem.Exceptions;
using MessagingSystem.Models;
using MessagingSystem.Roles;

namespace MessagingSystem;

public class MessagingAuthenticationService : AuthenticationService
{
    private int nextAccountId;

    public MessagingAuthenticationService()
    {
        nextAccountId = 0;
    }

    public override Account SignUp(
        Dictionary<string, Account> accounts,
        string fullName,
        Role role,
        string login,
        string password,
        MailBox mailBox)
    {
        if (accounts is null)
            throw new ArgumentNullException(nameof(accounts), "Invalid accounts list");

        if (accounts.TryGetValue(login, out Account? foundAccount))
            throw new DuplicateAccountException();

        var newAccount = new Account(nextAccountId, fullName, role, login, password, mailBox);

        accounts.Add(login, newAccount);

        return newAccount;
    }

    public override Account SignIn(Dictionary<string, Account> accounts, string login, string password)
    {
        if (accounts is null)
            throw new ArgumentNullException(nameof(accounts), "Invalid accounts list");

        bool accountExists = accounts.TryGetValue(login, out Account? foundAccount);

        if (!accountExists || foundAccount is null)
            throw new NotExistingAccountException();

        if (!foundAccount.Password.Equals(password))
            throw new InvalidPasswordException();

        return foundAccount;
    }

    private int GetNextAccountId()
    {
        return nextAccountId++;
    }
}
