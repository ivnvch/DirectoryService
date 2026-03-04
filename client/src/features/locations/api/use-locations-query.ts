import { GetLocationsRequest, locationsApi } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";

const DEFAULT_PAGE_SIZE = 10;

export function useLocationsQuery(request?: Partial<GetLocationsRequest>) {
  const page = request?.page ?? 1;
  const pageSize = request?.pageSize ?? DEFAULT_PAGE_SIZE;

  return useQuery({
    queryFn: () =>
      locationsApi.getLocations({
        page,
        pageSize,
      }),
    queryKey: ["locations", { page, pageSize }],
  });
}
