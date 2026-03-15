import { apiClient } from "@/shared/api/axios-instance";
import type { Location } from "./types";
import { PaginationResponse } from "@/shared/api/types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { LocationsFilterState } from "@/features/locations/model/location-filter-store";


export type AddressDto = {
  country: string;
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
};


export function buildAddressDto(data: {
  country: string;
  city: string;
  street: string;
  house: string;
  apartment?: string;
}): AddressDto {
  const { country, city, street, house, apartment } = data;
  return {
    country: country.trim(),
    city: city.trim(),
    street: street.trim(),
    house: house.trim(),
    ...(apartment?.trim() && { apartment: apartment.trim() }),
  };
}

export type CreateLocationRequest = {

    name: string;
    address: AddressDto;
    timezone: string;
};

export type GetLocationsRequest = {
  search?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
};

const EMPTY_PAGINATION: PaginationResponse<Location> = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 10,
  totalPages: 0,
};

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest): Promise<PaginationResponse<Location>> => {
    const response = await apiClient.get<{ result?: PaginationResponse<Location> }>(
      "/locations",
      { params: request }
    );

    return response.data.result ?? EMPTY_PAGINATION;
  },

    createLocation: async (request: CreateLocationRequest) => {
        const response = await apiClient.post("/locations", request);

        return response.data;
    },

    updateLocation: async (locationId: string, request: CreateLocationRequest) => {
      const response = await apiClient.put(`/locations/${locationId}`, request);
      return response.data;
    },

    deleteLocation: async (locationId: string) => {
      const response = await apiClient.delete(`/locations/${locationId}`);
      return response.data;
    },
}

export const locationsQueryOptions = {
  baseKey: "locations",
  
getLocationsOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () =>
        locationsApi.getLocations({
          page,
          pageSize,
        }),
      queryKey: [locationsQueryOptions.baseKey, { page }],
    });
  },

  getLocationsInfinityOptions: ({ filter }: { filter: LocationsFilterState }) => {
      return infiniteQueryOptions({
        queryKey: [locationsQueryOptions.baseKey, { filter }],
        queryFn: ({ pageParam }) => {
          const { search, isDeleted, pageSize } = filter;
          return locationsApi.getLocations({
            search,
            isActive: isDeleted === undefined ? undefined : !isDeleted,
            page: pageParam,
            pageSize: pageSize ?? 10,
          });
        },
        initialPageParam: 1,
        getNextPageParam: (response) => {
          if(!response || response.page >= response.totalPages) return undefined;

          return response.page + 1;
        },
        select: (data): PaginationResponse<Location> => ({
          items: data.pages.flatMap((page) => page?.items ?? []),
          totalCount: data.pages[0]?.totalCount ?? 0,
          page: data.pages[0]?.page ?? 1,
          pageSize: data.pages[0]?.pageSize,
          totalPages: data.pages[0]?.totalPages ?? 0,
        }),
      });
  }
};
  