using Shops.Models;

namespace Shops.Entities;

public class Item
{
    private int _quantity;

    public Item(Product product, int quantity)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product), "Invalid product");

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Product quantity must be positive");

        _quantity = quantity;

        Product = product;
    }

    public Product Product { get; private set; }

    public int Quantity
    {
        get => _quantity;

        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Requested amount of products must be positive");

            _quantity = value;
        }
    }
}
