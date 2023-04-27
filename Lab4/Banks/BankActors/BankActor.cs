using System.Text.RegularExpressions;

using Banks.Models;

namespace Banks.BankActors;

public abstract class BankActor
{
    private static readonly Regex RegexBankActorInitials = new Regex("^([a-zA-Zа-яА-Я]+[\\s-]?)+$", RegexOptions.Compiled);
    private static readonly Regex RegexBankActorPassportId = new Regex("^\\d{10}$", RegexOptions.Compiled);

    private string _name;
    private string _lastname;

    protected BankActor(
        string name,
        string lastname,
        Address? address,
        string? passportId)
    {
        if (string.IsNullOrEmpty(name) || !RegexBankActorInitials.IsMatch(name))
            throw new ArgumentException("Bank actor name must be a non-null alphabetic string", nameof(name));

        if (string.IsNullOrEmpty(lastname) || !RegexBankActorInitials.IsMatch(name))
            throw new ArgumentException("Bank actor lastname must be a non-null alphabetic string", nameof(lastname));

        if (passportId is not null && !RegexBankActorPassportId.IsMatch(passportId))
            throw new ArgumentException("Bank actor passport id must be a 10-char numeric string", nameof(passportId));

        Address = address;
        PassportId = passportId;

        _name = name;
        _lastname = lastname;
    }

    public int Id { get; internal set; }

    public string Name
    {
        get => _name;

        set
        {
            if (string.IsNullOrEmpty(value) || !RegexBankActorInitials.IsMatch(value))
                throw new ArgumentException("Bank actor name must be a non-null alphabetic string", nameof(value));

            _name = value;
        }
    }

    public string Lastname
    {
        get => _lastname;

        set
        {
            if (string.IsNullOrEmpty(value) || !RegexBankActorInitials.IsMatch(value))
                throw new ArgumentException("Bank actor lastname must be a non-null alphabetic string", nameof(value));

            _name = value;
        }
    }

    public Address? Address { get; set; }

    public string? PassportId { get; set; }
}
