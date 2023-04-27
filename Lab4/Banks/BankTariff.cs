using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using Banks.Exceptions;
using Banks.Models;

namespace Banks;

public class BankTariff
{
    private static readonly Regex RegexBankTariffName = new Regex("^([a-zA-Zа-яА-Я\\d]+[\\s-]?)+$", RegexOptions.Compiled);

    private List<DepositInterestRate> depositInterestRates;
    private decimal _debitInterestRate;
    private CreditCondition _creditCondition;
    private decimal _unverifiedClientTransactionLimit;

    public BankTariff(
        string name,
        List<DepositInterestRate> depositInterestRates,
        decimal debitInterestRate,
        CreditCondition creditComission,
        decimal unverifiedClientTransactionLimit)
    {
        if (string.IsNullOrEmpty(name) || !RegexBankTariffName.IsMatch(name))
            throw new ArgumentException("Invalid bank tariff name", nameof(name));

        if (depositInterestRates is null)
            throw new ArgumentNullException(nameof(depositInterestRates), "Invalid deposit interest rates");

        if (depositInterestRates.Count <= 0)
            throw new ArgumentException("Deposit interests rates must contain at least 1 rate", nameof(depositInterestRates));

        CheckDepositInterestRatesIntersection(depositInterestRates);

        if (debitInterestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(debitInterestRate), "Debit interest rate must be positive");

        if (creditComission is null)
            throw new ArgumentNullException(nameof(creditComission), "Invalid creit condition");

        if (unverifiedClientTransactionLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(unverifiedClientTransactionLimit), "Unverified client transaction limit must be positive");

        Name = name;

        this.depositInterestRates = depositInterestRates;
        _debitInterestRate = debitInterestRate;
        _creditCondition = creditComission;
        _unverifiedClientTransactionLimit = unverifiedClientTransactionLimit;

        AddAdditionalDepositInterestRates();
    }

    public string Name { get; }

    public ReadOnlyCollection<DepositInterestRate> DepositInterestRates => depositInterestRates.AsReadOnly();

    public decimal DebitInterestRate
    {
        get => _debitInterestRate;

        internal set
        {
            if (value < 0)
                throw new ArgumentNullException(nameof(value), "Debit interest rate must be positive");

            _debitInterestRate = value;
        }
    }

    public CreditCondition CreditCondition
    {
        get => _creditCondition;

        internal set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Invalid creit condition");

            _creditCondition = value;
        }
    }

    public decimal UnverifiedClientTransactionLimit
    {
        get => _unverifiedClientTransactionLimit;

        internal set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Unverified client transaction limit must be positive");

            _unverifiedClientTransactionLimit = value;
        }
    }

    public void ChangeDepositInterestRate(decimal lowerDepositBoundary, decimal upperDepositBoundary, decimal newInterestRate)
    {
        DepositInterestRate foundDepositInterestRate = GetDepositInterestRateByBoundaries(lowerDepositBoundary, upperDepositBoundary);

        foundDepositInterestRate.InterestRate = newInterestRate;
    }

    public void RemoveDepositInterestRate(decimal lowerDepositBoundary, decimal upperDepositBoundary, decimal interestRate)
    {
        DepositInterestRate foundDepositInterestRate = GetDepositInterestRate(lowerDepositBoundary, upperDepositBoundary, interestRate);

        if (depositInterestRates.Count <= 1)
            throw new UnavaliableOperationException("Deposit interests rates must contain at least 1 rate");

        depositInterestRates.Remove(foundDepositInterestRate);

        AddAdditionalDepositInterestRates();
    }

    public void ChangeCreditConditionLowerBoundary(decimal newLowerBoundary)
    {
        if (newLowerBoundary > 0)
            throw new ArgumentOutOfRangeException(nameof(newLowerBoundary), "Lower credit boundary must be negative");

        CreditCondition.LowerCreditBoundary = newLowerBoundary;
    }

    public void ChangeCreditConditionComission(decimal newComission)
    {
        if (newComission < 0)
            throw new ArgumentOutOfRangeException(nameof(newComission), "Comission must be positive");

        CreditCondition.Comission = newComission;
    }

    private DepositInterestRate GetDepositInterestRate(decimal lowerDepositBoundary, decimal upperDepositBoundary, decimal interestRate)
    {
        IEnumerable<DepositInterestRate> depositQuery = depositInterestRates
            .Where(currentDepositInterestRate =>
                currentDepositInterestRate.LowerDepositBoundary == lowerDepositBoundary &&
                currentDepositInterestRate.UpperDepositBoundary == upperDepositBoundary &&
                currentDepositInterestRate.InterestRate == interestRate);

        if (!depositQuery.Any())
            throw new NotExistingDepositInterestRateException();

        return depositQuery.ToList<DepositInterestRate>().First();
    }

    private DepositInterestRate GetDepositInterestRateByBoundaries(decimal lowerDepositBoundary, decimal upperDepositBoundary)
    {
        IEnumerable<DepositInterestRate> depositQuery = depositInterestRates
            .Where(currentDepositInterestRate =>
                currentDepositInterestRate.LowerDepositBoundary == lowerDepositBoundary &&
                currentDepositInterestRate.UpperDepositBoundary == upperDepositBoundary);

        if (!depositQuery.Any())
            throw new NotExistingDepositInterestRateException();

        return depositQuery.ToList<DepositInterestRate>().First();
    }

    private void CheckDepositInterestRatesIntersection(List<DepositInterestRate> depositInterestRates)
    {
        depositInterestRates.OrderBy(depositInterestRate => depositInterestRate.LowerDepositBoundary);

        for (int i = 0; i < depositInterestRates.Count - 1; i++)
        {
            if (depositInterestRates[i].UpperDepositBoundary > depositInterestRates[i + 1].LowerDepositBoundary)
                throw new DepositInterestRatesIntersectionException();
        }
    }

    private void AddAdditionalDepositInterestRates()
    {
        List<DepositInterestRate> orderedDepositInterestRates = depositInterestRates.OrderBy(depositInterestRate => depositInterestRate.LowerDepositBoundary)
            .ToList<DepositInterestRate>();

        if (orderedDepositInterestRates.Last().UpperDepositBoundary < DepositInterestRate.MaxUpperBoundary)
        {
            depositInterestRates.Add(
                new DepositInterestRate(
                    orderedDepositInterestRates.Last().UpperDepositBoundary,
                    DepositInterestRate.MaxUpperBoundary,
                    orderedDepositInterestRates.Last().InterestRate));
        }

        if (orderedDepositInterestRates[0].LowerDepositBoundary > DepositInterestRate.MinLowerBoundary)
        {
            depositInterestRates.Add(
                new DepositInterestRate(
                    DepositInterestRate.MinLowerBoundary,
                    orderedDepositInterestRates.First().LowerDepositBoundary,
                    orderedDepositInterestRates.First().InterestRate));
        }

        for (int i = 0; i < orderedDepositInterestRates.Count - 1; i++)
        {
            if (orderedDepositInterestRates[i].UpperDepositBoundary < orderedDepositInterestRates[i + 1].LowerDepositBoundary)
            {
                depositInterestRates.Add(
                    new DepositInterestRate(
                        orderedDepositInterestRates[i].UpperDepositBoundary,
                        orderedDepositInterestRates[i + 1].LowerDepositBoundary,
                        orderedDepositInterestRates[i].InterestRate));

                orderedDepositInterestRates = depositInterestRates.OrderBy(depositInterestRate => depositInterestRate.LowerDepositBoundary)
                    .ToList<DepositInterestRate>();
            }
        }
    }
}
