using System.Text;

namespace Shops.Models;

public class Product
{
    private string _category;
    private string _tradeMark;

    public Product(string category, string tradeMark)
    {
        if (string.IsNullOrEmpty(category) || string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Invalid product category", nameof(category));

        if (string.IsNullOrEmpty(tradeMark) || string.IsNullOrWhiteSpace(tradeMark))
            throw new ArgumentException("Invalid product trade mark", nameof(tradeMark));

        _category = category;
        _tradeMark = tradeMark;
    }

    public string Name
    {
        get => new StringBuilder()
                .Append(Category)
                .Append(' ')
                .Append(TradeMark)
                .ToString();
    }

    public string Category
    {
        get => _category;

        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid product category", nameof(value));

            _category = value;
        }
    }

    public string TradeMark
    {
        get => _tradeMark;

        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid product trade mark", nameof(value));

            _tradeMark = value;
        }
    }

    public bool IsMatch(string category, string tradeMark)
    {
        if (string.IsNullOrEmpty(category) || string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Invalid product category", nameof(category));

        if (string.IsNullOrEmpty(tradeMark) || string.IsNullOrWhiteSpace(tradeMark))
            throw new ArgumentException("Invalid product trade mark", nameof(tradeMark));

        string selfCategoryNormalized = Category.Trim().ToLower();
        string otherCategoryNormalized = category.Trim().ToLower();

        string selfTradeMarkNormalized = TradeMark.Trim().ToLower();
        string otherTradeMarkNormalized = tradeMark.Trim().ToLower();

        return selfCategoryNormalized.Equals(otherCategoryNormalized)
            && selfTradeMarkNormalized.Equals(otherTradeMarkNormalized);
    }
}
