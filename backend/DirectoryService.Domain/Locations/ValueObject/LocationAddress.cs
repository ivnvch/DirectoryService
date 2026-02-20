using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationAddress
{
    public LocationAddress(
        string country,
        string city,
        string street,
        string house,
        string? apartment = null)
    {
        Country = country;
        City = city;
        Street = street;
        House = house;
        Apartment = apartment;
    }
    
    
    public string Country { get; private set; }
    public string City { get; private set; }
    public string Street { get; private set; }
    public string House { get; private set; }
    public string? Apartment  { get; private set; }
    

    public static Result<LocationAddress, Error> Create(
        string country,
        string city,
        string street,
        string house,
        string? apartment)
    {
        if(string.IsNullOrWhiteSpace(country))
            return GeneralErrors.ValueIsInvalid($"{country}");
        
        if(string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsInvalid($"{city}");
        
        if(string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsInvalid($"{street}");
        
        if(string.IsNullOrWhiteSpace(house))
            return GeneralErrors.ValueIsInvalid($"{house}");
        
        return new LocationAddress(
            country,
            city,
            street,
            house,
            apartment);
    }
}