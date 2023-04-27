using Shops.Entities;
using Shops.Exceptions;
using Shops.Models;

namespace Shops.Services;

public class SupplyService
{
    public static readonly int MaxProductSupplyLimit = 1024;

    public SupplyService()
    {
        ProductFactory = new ProductFactory();
    }

    private ProductFactory ProductFactory { get; set; }

    public void SupplyProduct(Shop shop, string category, string tradeMark, int quantity, decimal dealPrice, decimal priceModifier)
    {
        if (dealPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(dealPrice), "Deal price must be positive");

        if (quantity <= 0 || quantity > SupplyService.MaxProductSupplyLimit)
            throw new ArgumentOutOfRangeException(nameof(quantity), string.Format("Invalid supply amount requested: must be from 1 to {0}", SupplyService.MaxProductSupplyLimit));

        if (dealPrice * quantity > shop.CashBalance)
            throw new InsufficientShopBalanceException();

        if (priceModifier < 0)
            throw new ArgumentOutOfRangeException(nameof(priceModifier), "Price modifier must be positive");

        Product product = ProductFactory.CreateProduct(category, tradeMark);

        var suppliedInventory = new Inventory(product, quantity, dealPrice);
        suppliedInventory.PriceModifier = priceModifier;

        shop.CashBalance -= dealPrice * quantity;

        IEnumerable<Inventory> productsQuery = shop.Products
            .Where(inventory => inventory.Product.IsMatch(category, tradeMark));

        var productsList = productsQuery.ToList<Inventory>();

        if (productsList.Count == 0)
        {
            shop.Products.Add(suppliedInventory);
        }
        else
        {
            productsList[0].BasePrice = dealPrice;
            productsList[0].Quantity += suppliedInventory.Quantity;
        }
    }
}
