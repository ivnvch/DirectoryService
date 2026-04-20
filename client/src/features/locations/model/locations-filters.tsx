import { Input } from "@/shared/components/ui/input";
import { Search } from "lucide-react";
import { useDebounce } from "use-debounce";
import {
  setFilterIsDeleted,
  setFilterSearch,
  useGetLocationsFilter,
} from "./location-filter-store";
import { useEffect, useState } from "react";

export function LocationFilters() {
  const { search, isDeleted } = useGetLocationsFilter();
  const [localSearch, setLocalSearch] = useState<string>(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  useEffect(() => {
    setFilterSearch(debouncedSearch);
  }, [debouncedSearch]);

  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="relative flex-1 max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          value={localSearch}
          onChange={(e) => setLocalSearch(e.target.value)}
          placeholder="Поиск по названию ..."
          className="pl-9"
        />
      </div>
      <label className="flex cursor-pointer items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={isDeleted}
          onChange={(e) => setFilterIsDeleted(e.target.checked)}
          className="h-4 w-4 rounded border-zinc-300 text-zinc-900 focus:ring-zinc-500"
        />
        <span>Показать удалённые</span>
      </label>
    </div>
  );
}
