import { GetLocationsRequest, locationsApi } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/error";
import { useQuery } from "@tanstack/react-query";

const DEFAULT_PAGE_SIZE = 10;

export function useLocationsQuery({page}: {page: number}) {
  const {data, isPending, error, isError} = useQuery({
    queryFn: () =>
      locationsApi.getLocations({
        page,
        pageSize: DEFAULT_PAGE_SIZE,
      }),
    queryKey: ["locations", { page, DEFAULT_PAGE_SIZE }],
  });

  return {
    locations: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    isPending,
    error: error instanceof EnvelopeError ? error : undefined,
    isError: isError
  }
}
