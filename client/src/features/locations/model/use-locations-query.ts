import { locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/error";
import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

const PAGE_SIZE = 10;

export function useLocationsQuery() {
  const {
    data, 
    isPending, 
    error, 
    isError, 
    refetch, 
    hasNextPage,
    fetchNextPage, 
    isFetchingNextPage } = useInfiniteQuery({
    ...locationsQueryOptions.getLocationsInfinityOptions({ pageSize: PAGE_SIZE })
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
