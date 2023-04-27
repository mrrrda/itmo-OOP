using Shops.Models;

namespace Shops.Entities;

public class Inventory
{
    private static readonly int MaxProductQuantityLimit = 1024;
    private static readonly decimal DefaultPriceModifier = 1;

    private decimal _priceModifier;
    private decimal _basePrice;
    private int _quantity;

    public Inventory(Product product, int quantity, decimal basePrice)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product), "Invalid product");

        if (quantity < 0 || quantity > MaxProductQuantityLimit)
            throw new ArgumentOutOfRangeException(nameof(quantity), string.Format("Requested amount of products must be from 1 to {0}", MaxProductQuantityLimit));

        if (basePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice), "Base price must be positive");

        _priceModifier = DefaultPriceModifier;

        _basePrice = basePrice;
        _quantity = quantity;

        Product = product;
    }

    public Product Product { get; private set; }

    public decimal BasePrice
    {
        get => _basePrice;

        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Base price must be positive");

            _basePrice = value;
        }
    }

    public decimal PriceModifier
    {
        get => _priceModifier;

        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Price modifier must be positive");

            _priceModifier = value;
        }
    }

    public decimal PurchasePrice
    {
        get => BasePrice * PriceModifier;
    }

    public int Quantity
    {
        get => _quantity;

        set
        {
            if (value < 0 || value > MaxProductQuantityLimit)
                throw new ArgumentOutOfRangeException(nameof(value), string.Format("Requested amount of products must be from 1 to {0}", MaxProductQuantityLimit));

            _quantity = value;
        }
    }
}
