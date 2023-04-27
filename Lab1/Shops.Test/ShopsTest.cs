using Shops.Exceptions;
using Shops.Models;
using Shops.Services;

using Xunit;

namespace Shops.Test;

public class ShopsTest
{
    // Set data tests
    [Theory]
    [InlineData("", "Mayakovskaya 1", 100000)]
    [InlineData("TestShop", "", 100000)]
    [InlineData("TestShop", "Mayakovskaya 1", -100000)]
    public void InitializeShop_ThrowsArgumentException(string name, string address, decimal shopBalance)
    {
        Assert.Throws<ArgumentException>(() => new Shop(name, address, shopBalance));
    }

    [Fact]
    public void SetShopInfo_ThrowsArgumentException()
    {
        var shop = new Shop("TestShop", "Mayakovskaya 1", 100000);

        Assert.Throws<ArgumentException>(() => shop.Name = string.Empty);
        Assert.Throws<ArgumentException>(() => shop.Address = string.Empty);

        Assert.Throws<ArgumentException>(() => shop.CashBalance = -100000);
    }

    [Theory]
    [InlineData("", 50000)]
    [InlineData("----", 50000)]
    [InlineData("Mariya$", 50000)]
    [InlineData(".Mariya", 50000)]
    [InlineData("Mariya", -50000)]
    public void InitializeCustomer_ThrowsArgumentException(string name, decimal balance)
    {
        Assert.Throws<ArgumentException>(() => new Customer(name, balance));
    }

    [Fact]
    public void SetCustomerInfo_ThrowsArgumentException()
    {
        var customer = new Customer("Mariya", 50000);

        Assert.Throws<ArgumentException>(() => customer.Name = string.Empty);

        Assert.Throws<ArgumentException>(() => customer.Balance = -50000);
    }

    // Shop tests
    [Theory]
    [InlineData("Moloko", "Sveze", 10)]
    [InlineData("Moloko", "Sveze", 5)]
    [InlineData("Moloko", "Sveze", 50)]
    [InlineData("Moloko", "Sveze", 2)]
    public void AddProductsToShop_ProductsCanBePurchased(string category, string tradeMark, int productQuantity)
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;
        decimal customerBalance = 50000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, category, tradeMark, productQuantity, basePrice, priceModifier);
        customer.BuyProduct(shopService, shop, category, tradeMark, productQuantity);

        Assert.Single(customer.Products);
        Assert.True(customer.Products[0].Product.IsMatch(category, tradeMark));
        Assert.Equal<int>(productQuantity, customer.Products[0].Quantity);
    }

    [Theory]
    [InlineData("Moloko", "Sveze", 10, 10)]
    [InlineData("Moloko", "Sveze", 5, 5)]
    [InlineData("Moloko", "Sveze", 50, 50)]
    [InlineData("Moloko", "Sveze", 2, 2)]
    public void AddProductToShop_ProductExists(string category, string tradeMark, int productQuantity, int excpectedProductQuantity)
    {
        var supplyService = new SupplyService();

        decimal shopBalance = 100000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);

        shop.RequestProduct(supplyService, category, tradeMark, productQuantity, basePrice, priceModifier);

        Assert.Single(shop.Products);
        Assert.True(shop.Products[0].Product.IsMatch(category, tradeMark));
        Assert.Equal<int>(excpectedProductQuantity, shop.Products[0].Quantity);
    }

    [Theory]
    [InlineData("Moloko", "Sveze", 10, 50, 100000, 99500)]
    [InlineData("Moloko", "Sveze", 5, 20, 100000, 99900)]
    [InlineData("Moloko", "Sveze", 50, 1, 100000, 99950)]
    [InlineData("Moloko", "Sveze", 2, 14, 100000, 99972)]
    public void AddProductToShop_BalanceDecreased(string category, string tradeMark, int productQuantity, decimal basePrice, decimal initialShopBalance, decimal expectedBalance)
    {
        var supplyService = new SupplyService();

        decimal priceModifier = 1.5m;

        var shop = new Shop("TestShop", "Mayakovskaya 1", initialShopBalance);

        shop.RequestProduct(supplyService, category, tradeMark, productQuantity, basePrice, priceModifier);

        Assert.Equal<decimal>(expectedBalance, shop.CashBalance);
    }

    [Fact]
    public void AddProductToShop_ExistingProductQuantityIncreased()
    {
        var supplyService = new SupplyService();

        decimal shopBalance = 100000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;

        int firstSupply = 10;
        int secondSupply = 15;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", firstSupply, basePrice, priceModifier);
        shop.RequestProduct(supplyService, "Moloko", "Sveze", secondSupply, basePrice, priceModifier);

        int excpectedProductsQuantity = firstSupply + secondSupply;

        Assert.Equal<int>(excpectedProductsQuantity, shop.Products[0].Quantity);
    }

    [Fact]
    public void BuyProduct_ShopProductQuantityDecreased()
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;
        decimal customerBalance = 50000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;
        int productQuantity = 10;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, basePrice, priceModifier);

        int purchasedQuantity = productQuantity / 2;

        int excpectedProductQuantity = shop.Products[0].Quantity - purchasedQuantity;

        customer.BuyProduct(shopService, shop, "Moloko", "Sveze", purchasedQuantity);

        Assert.Equal<int>(excpectedProductQuantity, shop.Products[0].Quantity);
    }

    [Fact]
    public void BuyProduct_ShopBalanceIncreased()
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal initialShopBalance = 100000;
        decimal customerBalance = 50000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;
        int productQuantity = 10;

        var shop = new Shop("TestShop", "Mayakovskaya 1", initialShopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, basePrice, priceModifier);

        decimal excpectedShopBalanceAfterPurchase = shop.CashBalance + (shop.Products[0].PurchasePrice * productQuantity);

        customer.BuyProduct(shopService, shop, "Moloko", "Sveze", productQuantity);

        Assert.Equal<decimal>(excpectedShopBalanceAfterPurchase, shop.CashBalance);
    }

    [Theory]
    [InlineData("Moloko", "Sveze", 10)]
    [InlineData("Moloko", "Svez", 1)]
    public void BuyProduct_ShopThrowsLackOfProductException(string category, string tradeMark, int productQuantity)
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;
        decimal customerBalance = 50000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;
        int shopProductQuantity = 1;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", shopProductQuantity, basePrice, priceModifier);

        Assert.Throws<LackOfProductException>(() => customer.BuyProduct(shopService, shop, category, tradeMark, productQuantity));
    }

    [Theory]
    [InlineData(10, 101, 1000)]
    [InlineData(10, 100.5, 1000)]
    [InlineData(1, 5, 4.99)]
    public void AddProductToShop_ThrowInsufficientShopBalanceException(int productQuantity, decimal basePrice, decimal shopBalance)
    {
        var supplyService = new SupplyService();

        decimal priceModifier = 1.5m;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);

        Assert.Throws<InsufficientShopBalanceException>(() => shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, basePrice, priceModifier));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AddProductQuantityToShop_ThrowsArgumentOutOfRangeException(int productQuantity)
    {
        var supplyService = new SupplyService();

        decimal shopBalance = 100000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);

        Assert.Throws<ArgumentOutOfRangeException>(() => shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, basePrice, priceModifier));
    }

    [Theory]
    [InlineData(20, 30)]
    [InlineData(500, 100)]
    [InlineData(500, 500.01)]
    public void ChangeProductPrice_PriceIsChanged(decimal initialPrice, decimal newPrice)
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;

        decimal priceModifier = 1.5m;
        int productQuantity = 10;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, initialPrice, priceModifier);

        shop.ChangeProductPrice(shopService, "Moloko", "Sveze", newPrice);

        Assert.Equal<decimal>(shop.Products[0].BasePrice, newPrice);
    }

    [Fact]
    public void ChangeProductPrice_ThrowsArgumentOutOfRangeException()
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;

        decimal initialPrice = 100;
        decimal invalidPrice = -1;

        decimal priceModifier = 1.5m;
        int productQuantity = 10;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, initialPrice, priceModifier);

        Assert.Throws<ArgumentOutOfRangeException>(() => shop.ChangeProductPrice(shopService, "Moloko", "Sveze", invalidPrice));
    }

    // Customer tests
    [Theory]
    [InlineData(100, 25, 1, 5)]
    [InlineData(99.99, 100, 1, 1)]
    [InlineData(200, 100.01, 2, 1)]
    public void BuyProduct_ThrowsInsufficientCustomerBalanceException(decimal customerBalance, decimal basePrice, decimal priceModifier, int productQuantity)
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, basePrice, priceModifier);

        Assert.Throws<InsufficientCustomerBalanceException>(() => customer.BuyProduct(shopService, shop, "Moloko", "Sveze", productQuantity));
    }

    [Theory]
    [InlineData("", "Sveze")]
    [InlineData("Moloko", "")]
    public void BuyInvalidProduct_ThrowsArgumentException(string category, string tradeMark)
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;
        decimal customerBalance = 50000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;
        int productQuantity = 1;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, basePrice, priceModifier);

        Assert.Throws<ArgumentException>(() => customer.BuyProduct(shopService, shop, category, tradeMark, productQuantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void BuyProductQuantity_ThrowsArgumentOutOfRangeException(int productQuantity)
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal shopBalance = 100000;
        decimal customerBalance = 50000;

        decimal basePrice = 50;
        decimal priceModifier = 1.5m;
        int shopProductQuantity = 1;

        var shop = new Shop("TestShop", "Mayakovskaya 1", shopBalance);
        var customer = new Customer("Mariya", customerBalance);

        shop.RequestProduct(supplyService, "Moloko", "Sveze", shopProductQuantity, basePrice, priceModifier);

        Assert.Throws<ArgumentOutOfRangeException>(() => customer.BuyProduct(shopService, shop, "Moloko", "Sveze", productQuantity));
    }

    [Fact]
    public void BuyProduct_ProductAtBestPricePurchased()
    {
        var supplyService = new SupplyService();
        var shopService = new ShopService();

        decimal customerBalance = 50000;
        var customer = new Customer("Mariya", customerBalance);

        var shops = new List<Shop>();
        decimal shopsBalance = 100000;

        decimal priceModifier = 1.5m;
        int productQuantity = 10;

        decimal firstBasePrice = 49;
        decimal secondBasePrice = 50;
        decimal thirdBasePrice = 51;

        decimal bestPurchasePrice = Math.Min(firstBasePrice, Math.Min(secondBasePrice, thirdBasePrice)) * priceModifier;

        shops.Add(new Shop("TestShop1", "Mayakovskaya 1", shopsBalance));
        shops.Add(new Shop("TestShop2", "Mayakovskaya 1", shopsBalance));
        shops.Add(new Shop("TestShop3", "Mayakovskaya 1", shopsBalance));

        shops[0].RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, firstBasePrice, priceModifier);
        shops[1].RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, secondBasePrice, priceModifier);
        shops[2].RequestProduct(supplyService, "Moloko", "Sveze", productQuantity, thirdBasePrice, priceModifier);

        customer.BuyProductAtBestPrice(shopService, shops, "Moloko", "Sveze", productQuantity);

        Assert.Equal<decimal>(bestPurchasePrice * productQuantity, customerBalance - customer.Balance);
        Assert.Equal<int>(productQuantity, customer.Products[0].Quantity);
    }
}
