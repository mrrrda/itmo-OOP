using System.Text;

using Shops.Entities;
using Shops.Exceptions;
using Shops.Models;

namespace Shops.Services;

public class ShopService
{
    public ShopService()
    { }

    public void ChangeProductPrice(Shop shop, string category, string tradeMark, decimal newPrice)
    {
        if (shop is null)
            throw new ArgumentNullException(nameof(shop), "Invalid shop");

        if (newPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Product price must be positive");

        IEnumerable<Inventory> productsQuery = shop.Products
            .Where(inventory => inventory.Product.IsMatch(category, tradeMark));

        var productsList = productsQuery.ToList<Inventory>();

        if (productsList.Count == 0)
            throw new LackOfProductException(productsList[0].Product.Name);
        else
            productsList[0].BasePrice = newPrice;
    }

    public void SellProduct(Shop shop, Customer customer, string category, string tradeMark, int quantity)
    {
        if (shop is null)
            throw new ArgumentNullException(nameof(shop), "Invalid shop");

        if (customer is null)
            throw new ArgumentNullException(nameof(customer), "Invalid customer");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Requested amount of products must be greater than 0");

        IEnumerable<Inventory> productsQuery = shop.Products
            .Where(inventory => inventory.Product.IsMatch(category, tradeMark));

        var productsList = productsQuery.ToList<Inventory>();

        if (productsList.Count == 0)
            throw new LackOfProductException(new StringBuilder().Append(category).Append(' ').Append(tradeMark).ToString());

        if (productsList[0].Quantity < quantity)
            throw new LackOfProductException(productsList[0].Product.Name);

        Inventory foundInventory = productsList[0];

        decimal totalPrice = foundInventory.PurchasePrice * quantity;

        if (customer.Balance < totalPrice)
            throw new InsufficientCustomerBalanceException();

        foundInventory.Quantity -= quantity;

        IEnumerable<Item> itemQuery = customer.Products.
            Where(item => item.Product.IsMatch(category, tradeMark));

        var productInfo = itemQuery.ToList<Item>();

        if (productInfo.Count == 0)
            customer.Products.Add(new Item(foundInventory.Product, quantity));
        else
            productInfo[0].Quantity += quantity;

        customer.Balance -= totalPrice;
        shop.CashBalance += totalPrice;
    }

    public void SellProductAtBestPrice(List<Shop> shops, Customer customer, string category, string tradeMark, int quantity)
    {
        if (shops is null)
            throw new ArgumentNullException(nameof(shops), "Invalid list of shops");

        if (customer is null)
            throw new ArgumentNullException(nameof(customer), "Invalid customer");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Requested amount of products must be greater than 0");

        Shop? bestShop = null;
        Inventory? bestProduct = null;

        foreach (Shop shop in shops)
        {
            foreach (Inventory inventory in shop.Products)
            {
                if (inventory.Quantity >= quantity &&
                    inventory.Product.IsMatch(category, tradeMark) &&
                    (bestProduct is null || inventory.PurchasePrice < bestProduct.PurchasePrice))
                {
                    bestShop = shop;
                    bestProduct = inventory;

                    break;
                }
            }
        }

        if (bestShop is null || bestProduct is null)
            throw new LackOfProductException(new StringBuilder().Append(category).Append(' ').Append(tradeMark).ToString());

        if (customer.Balance < bestProduct.PurchasePrice)
            throw new InsufficientCustomerBalanceException();

        SellProduct(bestShop, customer, category, tradeMark, quantity);
    }
}
