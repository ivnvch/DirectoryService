import { apiClient } from "@/shared/api/axios-instance";
import type { Location } from "./types";

export type CreateLocationRequest = {

    name: string;
};

export type GetLocationsRequest = {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;

};

export type Envelope<T = unknown> = {
    items: T | null;
    error: ApiError | null;
    isError: boolean;
    timeGenerated: string;
};

export type ApiError = {
    messages: ErrorMessage[];
    type: ErrorType;
};

export type ErrorMessage = {
    code: string;
    message: string;
    invalidField?: string | null;
};

export type ErrorType = 
    | "validation"
    | "not_found"
    | "failure"
    | "conflict"
    | "authentication"
    | "authorization";

export const locationsApi = {
    getLocations: async (request: GetLocationsRequest): Promise<Location[]> => {
        const response = await apiClient.get<{ result?: { items?: Location[] } }>("/locations", {
            params: request,
        });

        return response.data.result?.items || [];
    },

    createLocation: async (request: CreateLocationRequest) => {
        const response = await apiClient.post("/locations", request);

        return response.data;
    }
}