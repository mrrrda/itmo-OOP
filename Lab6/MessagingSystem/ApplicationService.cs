using System.Text.Json.Serialization;
using MessagingSystem.Models;
using MessagingSystem.Roles;

namespace MessagingSystem;

public class ApplicationService
{
    private AuthenticationService authenticationService;

    public ApplicationService()
    {
        authenticationService = new MessagingAuthenticationService();
        Accounts = new Dictionary<string, Account>();
    }

    public ApplicationService(AuthenticationService authenticationService)
    {
        if (authenticationService is null)
            throw new ArgumentNullException(nameof(authenticationService), "Invalid authentication service");

        Accounts = new Dictionary<string, Account>();

        this.authenticationService = authenticationService;
    }

    [JsonIgnore]
    public AuthenticationService AuthenticationService
    {
        get => authenticationService;

        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Invalid authentication service");

            authenticationService = value;
        }
    }

    public Dictionary<string, Account> Accounts { get; set; }

    public Account SignUpUser(
        string fullName,
        Role role,
        string login,
        string password,
        MailBox mailBox)
    {
        return AuthenticationService.SignUp(Accounts, fullName, role, login, password, mailBox);
    }

    public Account SignInUser(string login, string password)
    {
        return AuthenticationService.SignIn(Accounts, login, password);
    }
}
