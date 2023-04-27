using System.Text;
using System.Text.RegularExpressions;

namespace Banks.Models;

public class Address
{
    public static readonly string DefaultBuilding = string.Empty;

    private static readonly Regex RegexZip = new Regex("^\\d{6}$", RegexOptions.Compiled);
    private static readonly Regex RegexLocation = new Regex("^([a-zA-Zа-яА-Я]+[\\s-]?)+$", RegexOptions.Compiled);
    private static readonly Regex RegexBuilding = new Regex("^[a-zA-Zа-яА-Я\\d]+$", RegexOptions.Compiled);

    private static readonly string AddressSeparator = ", ";

    private string _zip;
    private string _country;
    private string _city;
    private string _street;
    private int _streetNumber;
    private string _building;

    public Address(
        string zip,
        string country,
        string city,
        string street,
        int streetNumber)
    {
        if (string.IsNullOrEmpty(zip) || !RegexZip.IsMatch(zip))
            throw new ArgumentException("Zip must be a 6-char numeric string", nameof(zip));

        if (string.IsNullOrEmpty(country) || !RegexLocation.IsMatch(country))
            throw new ArgumentException("Country must be a non-null alphabetic string", nameof(country));

        if (string.IsNullOrEmpty(city) || !RegexLocation.IsMatch(city))
            throw new ArgumentException("City must be a non-null alphabetic string", nameof(city));

        if (streetNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(streetNumber), "Street number must be positive");

        _building = DefaultBuilding;

        _zip = zip;
        _country = country;
        _city = city;
        _street = street;
        _streetNumber = streetNumber;
    }

    public Address(
        string zip,
        string country,
        string city,
        string street,
        int streetNumber,
        string building)
    {
        if (string.IsNullOrEmpty(zip) || !RegexZip.IsMatch(zip))
            throw new ArgumentException("Zip must be a 6-char numeric string", nameof(zip));

        if (string.IsNullOrEmpty(country) || !RegexLocation.IsMatch(country))
            throw new ArgumentException("Country must be a non-null alphabetic string", nameof(country));

        if (string.IsNullOrEmpty(city) || !RegexLocation.IsMatch(city))
            throw new ArgumentException("City must be a non-null alphabetic string", nameof(city));

        if (streetNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(streetNumber), "Street number must be positive");

        if (string.IsNullOrEmpty(building) || !RegexBuilding.IsMatch(building))
            throw new ArgumentException("Building must be a non-null alphanumeric string", nameof(city));

        _zip = zip;
        _country = country;
        _city = city;
        _street = street;
        _streetNumber = streetNumber;
        _building = building;
    }

    public string Zip { get => _zip; }
    public string Country { get => _country; }
    public string City { get => _city; }
    public string Street { get => _street; }
    public int StreetNumber { get => _streetNumber; }
    public string Building { get => _building; }

    public override string ToString()
    {
        var fullAddress = new StringBuilder(Zip)
            .Append(Country)
            .Append(AddressSeparator)
            .Append(City)
            .Append(AddressSeparator)
            .Append(Street)
            .Append(AddressSeparator)
            .Append(StreetNumber);

        if (!Building.Equals(DefaultBuilding))
            fullAddress.Append(AddressSeparator).Append(Building);

        return fullAddress.ToString();
    }
}
