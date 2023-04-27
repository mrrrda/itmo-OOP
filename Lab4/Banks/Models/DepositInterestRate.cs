namespace Banks.Models;

public class DepositInterestRate
{
    public static readonly decimal MaxUpperBoundary = decimal.MaxValue;
    public static readonly decimal MinLowerBoundary = 0M;

    private decimal _lowerDepositBoundary;
    private decimal _upperDepositBoundary;
    private decimal _interestRate;

    public DepositInterestRate(decimal lowerDepositBoundary, decimal upperDepositBoundary, decimal interestRate)
    {
        if (lowerDepositBoundary < 0)
            throw new ArgumentOutOfRangeException(nameof(lowerDepositBoundary), "Lower deposit boundary must be positive");

        if (upperDepositBoundary < 0 || upperDepositBoundary < lowerDepositBoundary)
            throw new ArgumentOutOfRangeException(nameof(upperDepositBoundary), "Upper deposit boundary must be positive and exceed lower boundary");

        if (interestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(interestRate), "Interest rate must be positive");

        _lowerDepositBoundary = lowerDepositBoundary;
        _upperDepositBoundary = upperDepositBoundary;
        _interestRate = interestRate;
    }

    public decimal LowerDepositBoundary
    {
        get => _lowerDepositBoundary;

        internal set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Lower deposit boundary must be positive");

            _lowerDepositBoundary = value;
        }
    }

    public decimal UpperDepositBoundary
    {
        get => _upperDepositBoundary;

        internal set
        {
            if (value < 0 || value < LowerDepositBoundary)
                throw new ArgumentOutOfRangeException(nameof(value), "Lower deposit boundary must be positive and exceed lower boundary");

            _upperDepositBoundary = value;
        }
    }

    public decimal InterestRate
    {
        get => _interestRate;

        internal set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Interest rate must be positive");

            _interestRate = value;
        }
    }
}
