import { create } from 'zustand'
import { PAGE_SIZE } from "./use-locations-query";
import { useShallow } from 'zustand/shallow';

export type LocationsFilterState = {
     search?: string;
     isDeleted?: boolean;
     pageSize: number;
};

type Actions = {
     setSearch: (input: LocationsFilterState["search"]) => void;
     setIsDeleted: (isDeleted: LocationsFilterState["isDeleted"]) => void;
};

type LocationsFilterStore = LocationsFilterState & Actions;

const initialState: LocationsFilterState = {
     search: "",
     pageSize: PAGE_SIZE,
     isDeleted: undefined,
}
const useLocationsFilterStore = create<LocationsFilterStore>((set) => ({
     ...initialState,
     setSearch: (input: string) => 
          set(() => ({search: input.trim() || undefined})),
     setIsDeleted: (isDeleted: boolean | undefined) => set(() => ({ isDeleted }))
     

}));


export const useGetLocationsFilter = () => {
     return useLocationsFilterStore(useShallow ((state) => ({
          search: state.search,
          isDeleted: state.isDeleted,
          pageSize: state.pageSize
     })));
};

export const setFilterSearch = (input: LocationsFilterState["search"]) => 
     useLocationsFilterStore.getState().setSearch(input);

export const setFilterIsDeleted = (input: LocationsFilterState["isDeleted"]) =>
     useLocationsFilterStore.getState().setIsDeleted(input);
