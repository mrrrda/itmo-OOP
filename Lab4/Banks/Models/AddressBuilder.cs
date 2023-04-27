namespace Banks.Models;

public class AddressBuilder
{
    private string? _zip;
    private string? _country;
    private string? _city;
    private string? _street;
    private int _streetNumber;
    private string? _building;

    public AddressBuilder()
    {
        _streetNumber = -1;
    }

    public Address FillAddress()
    {
        if (string.IsNullOrEmpty(_zip) || string.IsNullOrWhiteSpace(_zip))
            throw new ArgumentException("Address must contain zip", nameof(_zip));

        if (string.IsNullOrEmpty(_country) || string.IsNullOrWhiteSpace(_country))
            throw new ArgumentException("Address must contain country", nameof(_country));

        if (string.IsNullOrEmpty(_city) || string.IsNullOrWhiteSpace(_city))
            throw new ArgumentException("Address must contain city", nameof(_city));

        if (string.IsNullOrEmpty(_street) || string.IsNullOrWhiteSpace(_street))
            throw new ArgumentException("Address must contain street", nameof(_street));

        if (_streetNumber == -1)
            throw new ArgumentException("Address must contain street number", nameof(_streetNumber));

        if (_building is null)
            return new Address(_zip, _country, _city, _street, _streetNumber);

        return new Address(_zip, _country, _city, _street, _streetNumber, _building);
    }

    public AddressBuilder SetZip(string zip)
    {
        if (string.IsNullOrEmpty(zip) || string.IsNullOrWhiteSpace(zip))
            throw new ArgumentException("Invalid zip", nameof(zip));

        _zip = zip;

        return this;
    }

    public AddressBuilder SetCountry(string country)
    {
        if (string.IsNullOrEmpty(country) || string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Invalid country", nameof(country));

        _country = country;

        return this;
    }

    public AddressBuilder SetCity(string city)
    {
        if (string.IsNullOrEmpty(city) || string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Invalid city", nameof(city));

        _city = city;

        return this;
    }

    public AddressBuilder SetStreet(string street)
    {
        if (string.IsNullOrEmpty(street) || string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Invalid street", nameof(street));

        _street = street;

        return this;
    }

    public AddressBuilder SetStreetNumber(int streetNumber)
    {
        if (streetNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(streetNumber), "Street number must be positive");

        _streetNumber = streetNumber;

        return this;
    }

    public AddressBuilder SetBuilding(string? building)
    {
        _building = building;

        return this;
    }
}
