using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using MessagingSystem.Exceptions;

namespace MessagingSystem.Models;

public class Employee
{
    public static readonly Regex RegexFullName = new Regex("^([a-zA-Zа-яА-Я]+[\\s-]?)+$", RegexOptions.Compiled);

    private List<Employee> subordinates;
    private Employee? superior;

    public Employee(int id, string fullName, Account account)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be positive number");

        if (!RegexFullName.IsMatch(fullName))
            throw new ArgumentException("Full name must be an alphabetic string", nameof(fullName));

        if (account is null)
            throw new ArgumentNullException(nameof(account), "Invalid account");

        subordinates = new List<Employee>();

        Id = id;
        FullName = fullName;

        Account = account;
    }

    public int Id { get; }
    public string FullName { get; }

    [JsonIgnore]
    public Account Account { get; }

    public ReadOnlyCollection<Employee> Subordinates => subordinates.AsReadOnly();

    public Employee? Superior
    {
        get => superior;

        set
        {
            if (value is not null && subordinates.Contains(value))
                throw new InvalidSuperiorException();

            superior = value;
        }
    }

    public void AddSubordinate(Employee newSubordinate)
    {
        if (newSubordinate is null)
            throw new ArgumentNullException(nameof(newSubordinate), "Invalid suborinate");

        if (subordinates.Contains(newSubordinate))
            throw new DuplicateSubordinateException();

        if (Superior is not null && Superior.Equals(newSubordinate))
            throw new InvalidSubordinateException();

        subordinates.Add(newSubordinate);
    }

    public void RemoveSubordinate(Employee subordinateToRemove)
    {
        if (subordinateToRemove is null)
            throw new ArgumentNullException(nameof(subordinateToRemove), "Invalid suborinate");

        if (!subordinates.Contains(subordinateToRemove))
            throw new NotExistingSubordinateException();

        subordinates.Remove(subordinateToRemove);
    }
}
