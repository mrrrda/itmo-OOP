using Shops.Models;

namespace Shops.Services;

public class ProductFactory
{
    private readonly List<Product> products;

    public ProductFactory()
    {
        products = new List<Product>();
    }

    public Product CreateProduct(string category, string tradeMark)
    {
        if (string.IsNullOrEmpty(category) || string.IsNullOrWhiteSpace(category))
            throw new ArgumentException(string.Format("Invalid product category: {0}", category), nameof(category));

        if (string.IsNullOrEmpty(tradeMark) || string.IsNullOrWhiteSpace(tradeMark))
            throw new ArgumentException(string.Format("Invalid product trade mark: {0}", tradeMark), nameof(tradeMark));

        IEnumerable<Product> productsQuery = products
            .Where(product => product.IsMatch(category, tradeMark));

        var productsList = productsQuery.ToList<Product>();

        if (productsList.Count == 0)
        {
            var newProduct = new Product(category, tradeMark);
            products.Add(newProduct);

            return newProduct;
        }

        return productsList[0];
    }
}
