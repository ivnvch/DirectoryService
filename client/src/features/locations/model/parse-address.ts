/**
 * Парсит адрес из формата "Country, City, Street, House, Apartment"
 */
export function parseAddressString(address: string): {
  country: string;
  city: string;
  street: string;
  house: string;
  apartment: string;
} {
  const parts = address.split(", ").map((p) => p.trim());
  return {
    country: parts[0] ?? "",
    city: parts[1] ?? "",
    street: parts[2] ?? "",
    house: parts[3] ?? "",
    apartment: parts[4] ?? "",
  };
}
