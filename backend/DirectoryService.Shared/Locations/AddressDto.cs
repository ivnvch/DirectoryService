namespace DirectoryService.Shared.Locations;

public record AddressDto(
    string Country, 
    string City, 
    string Street,
    string House,
    string? Apartment);