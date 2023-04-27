using System.Collections.ObjectModel;

using Banks.Accounts;
using Banks.Exceptions;
using Banks.Models;

namespace Banks.BankActors;

public class Client : BankActor
{
    private List<TransactionHandler> bankAccounts;

    public Client(
        string name,
        string lastname,
        Address? address,
        string? passportId)
        : base(name, lastname, address, passportId)
    {
        bankAccounts = new List<TransactionHandler>();
    }

    public ReadOnlyCollection<TransactionHandler> BankAccounts => bankAccounts.AsReadOnly();

    internal void AddBankAccount(TransactionHandler newBankAccount)
    {
        if (newBankAccount is null)
            throw new ArgumentNullException(nameof(newBankAccount), "Invalid bank account");

        if (bankAccounts.Contains(newBankAccount))
            throw new DuplicateBankAccountException();

        if (bankAccounts.Any(bankAccount => bankAccount.Id == newBankAccount.Id))
            throw new DuplicateBankAccountException();

        bankAccounts.Add(newBankAccount);
    }

    internal void RemoveBankAccount(TransactionHandler bankAccountToRemove)
    {
        if (bankAccountToRemove is null)
            throw new ArgumentNullException(nameof(bankAccountToRemove), "Invalid bank account");

        if (!bankAccounts.Contains(bankAccountToRemove))
            throw new NotExistingBankAccountException();

        bankAccounts.Remove(bankAccountToRemove);
    }
}
