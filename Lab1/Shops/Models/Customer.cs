using System.Text.RegularExpressions;

using Shops.Entities;
using Shops.Services;

namespace Shops.Models;

public class Customer
{
    private static readonly string NameRegex = "^([a-zA-Zа-яА-Я]+[\\s-]?)+$";

    private string _name;
    private decimal _balance;

    public Customer(string name, decimal balance)
    {
        if (string.IsNullOrEmpty(name) || !Regex.IsMatch(name, NameRegex))
            throw new ArgumentException(string.Format("Invalid customer name: {0}", name), nameof(name));

        if (balance < 0)
            throw new ArgumentException("Amount of money should be positive", nameof(balance));

        Products = new List<Item>();

        _name = name;
        _balance = balance;
    }

    public List<Item> Products { get; private set; }

    public string Name
    {
        get => _name;

        set
        {
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, NameRegex))
                throw new ArgumentException(string.Format("Invalid customer name: {0}", value), nameof(value));

            _name = value;
        }
    }

    public decimal Balance
    {
        get => _balance;

        set
        {
            if (value < 0)
                throw new ArgumentException("Amount of money should be positive", nameof(value));

            _balance = value;
        }
    }

    public void BuyProduct(ShopService shopService, Shop shop, string category, string tradeMark, int quantity)
    {
        shopService.SellProduct(shop, this, category, tradeMark, quantity);
    }

    public void BuyProductAtBestPrice(ShopService shopService, List<Shop> shops, string category, string tradeMark, int quantity)
    {
        shopService.SellProductAtBestPrice(shops, this, category, tradeMark, quantity);
    }
}
