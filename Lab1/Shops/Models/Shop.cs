using Shops.Entities;
using Shops.Services;

namespace Shops.Models;

public class Shop
{
    private static int nextShopId = 0;

    private string _name;
    private string _address;
    private decimal _cashBalance;

    public Shop(string name, string address, decimal cashBalance)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Invalid shop name", nameof(name));

        if (string.IsNullOrEmpty(address) || string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Invalid shop address", nameof(address));

        if (cashBalance < 0)
            throw new ArgumentException("Shop cash balance must be positive", nameof(cashBalance));

        Id = nextShopId++;

        _name = name;
        _address = address;
        _cashBalance = cashBalance;

        Products = new List<Inventory>();
    }

    public List<Inventory> Products { get; private set; }

    public int Id { get; private set; }

    public string Name
    {
        get => _name;

        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid shop name", nameof(value));

            _name = value;
        }
    }

    public string Address
    {
        get => _address;

        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid shop address", nameof(value));

            _address = value;
        }
    }

    public decimal CashBalance
    {
        get => _cashBalance;

        set
        {
            if (value < 0)
                throw new ArgumentException("Amount of money must be positive", nameof(value));

            _cashBalance = value;
        }
    }

    public void ChangeProductPrice(ShopService shopService, string category, string tradeMark, decimal newPrice)
    {
        shopService.ChangeProductPrice(this, category, tradeMark, newPrice);
    }

    public void SellProduct(ShopService shopService, Customer customer, string category, string tradeMark, int quantity)
    {
        shopService.SellProduct(this, customer, category, tradeMark, quantity);
    }

    public void RequestProduct(SupplyService supplyService, string category, string tradeMark, int quantity, decimal dealPrice, decimal priceModifier)
    {
        supplyService.SupplyProduct(this, category, tradeMark, quantity, dealPrice, priceModifier);
    }
}
