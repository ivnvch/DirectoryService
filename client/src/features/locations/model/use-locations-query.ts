import { locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/error";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";
import { LocationsFilter } from "../ui/locations-list";
import { LocationsFilterState } from "./location-filter-store";

export const PAGE_SIZE = 5;

export function useLocationsQuery({ search, pageSize, isDeleted }: LocationsFilterState) {
  const {
    data, 
    isPending, 
    error, 
    isError, 
    refetch, 
    hasNextPage,
    fetchNextPage, 
    isFetchingNextPage } = useInfiniteQuery({
    ...locationsQueryOptions.getLocationsInfinityOptions({ 
      filter: { search, isDeleted, pageSize } })
  });


  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
      (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage){
          fetchNextPage();
        }
      },
      {
        threshold: 0.5,
      });
  
      if (el) {
        observer.observe(el);

        return () => observer.disconnect();
      }
    }, [fetchNextPage, hasNextPage, isFetchingNextPage]
  );


  return {
    locations: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    isPending,
    error: error instanceof EnvelopeError ? error : undefined,
    isError: isError,
    refetch,
    isFetchingNextPage,
    cursorRef
  }
}
