using Banks.Models;

namespace Banks.BankActors;

public class ClientBuilder
{
    private string? _name;
    private string? _lastname;
    private Address? address;
    private string? _passportId;

    public Client Register()
    {
        if (_name is null)
            throw new ArgumentNullException("Client must have name", nameof(_name));

        if (_lastname is null)
            throw new ArgumentNullException("Client must have lastname", nameof(_lastname));

        return new Client(_name, _lastname, address, _passportId);
    }

    public ClientBuilder SetName(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Invalid client name", nameof(name));

        _name = name;

        return this;
    }

    public ClientBuilder SetLastname(string lastname)
    {
        if (string.IsNullOrEmpty(lastname) || string.IsNullOrWhiteSpace(lastname))
            throw new ArgumentException("Invalid client lastname", nameof(lastname));

        _lastname = lastname;

        return this;
    }

    public ClientBuilder SetAddress(Address? address)
    {
        this.address = address;

        return this;
    }

    public ClientBuilder SetPassportId(string? passportId)
    {
        _passportId = passportId;

        return this;
    }
}
