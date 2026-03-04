import { apiClient } from "@/shared/api/axios-instance";
import type { Location } from "./types";
import { PaginationResponse } from "@/shared/api/types";

export type CreateLocationRequest = {

    name: string;
};

export type GetLocationsRequest = {
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
    }
}