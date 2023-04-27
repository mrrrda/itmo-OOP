namespace Banks.Models;

public class CreditCondition
{
    private decimal _lowerCreditBoundary;
    private decimal _comission;

    public CreditCondition(decimal lowerCreditBoundary, decimal comission)
    {
        if (lowerCreditBoundary > 0)
            throw new ArgumentOutOfRangeException(nameof(lowerCreditBoundary), "Lower credit boundary must be negative");

        if (comission < 0)
            throw new ArgumentOutOfRangeException(nameof(comission), "Comission must be positive");

        _lowerCreditBoundary = lowerCreditBoundary;
        _comission = comission;
    }

    public decimal LowerCreditBoundary
    {
        get => _lowerCreditBoundary;

        internal set
        {
            if (value > 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Lower credit boundary must be negative");

            _lowerCreditBoundary = value;
        }
    }

    public decimal Comission
    {
        get => _comission;

        internal set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Comission must be positive");

            _comission = value;
        }
    }
}
