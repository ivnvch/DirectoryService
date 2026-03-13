import { useLocationsQuery } from "@/features/locations/model/use-locations-query";
import { useDeleteLocation } from "@/features/locations/model/use-delete-location";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { MapPin, Pencil, Trash2 } from "lucide-react";
import { RefCallback, useCallback, useState } from "react";
import { Spinner } from "@/shared/components/ui/spinner";
import CreateLocationModal from "./create-location-modal";
import UpdateLocationModal from "./update-location-modal";
import type { Location } from "@/entities/locations/types";
import { Button } from "@/shared/components/ui/button";
import { RefCallBack } from "react-hook-form";

function formatDate(value: Date | string | undefined): string {
  if (!value) return "—";
  const d = typeof value === "string" ? new Date(value) : value;
  return d.toLocaleDateString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

export default function LocationsList() {
  const [open, setOpen] = useState(false);
  const [updateModalOpen, setUpdateModalOpen] = useState(false);
  const [locationToEdit, setLocationToEdit] = useState<Location | null>(null);

  const {
    locations,
    isPending,
    error,
    isError,
    refetch,
    cursorRef,
    isFetchingNextPage,
  } = useLocationsQuery();

  const { deleteLocation, isPending: isDeleting } = useDeleteLocation();

  if (isPending) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Spinner className="size-8" />
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      <Button onClick={() => setOpen(true)} disabled={isPending}>
        Добавить локацию
      </Button>
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Локации</h1>
        <p className="text-muted-foreground mt-1">
          Список всех локаций организации
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MapPin className="size-5" />
            Список локаций
          </CardTitle>
          <CardDescription>
            Всего: {isPending ? "—" : locations?.length} записей
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full min-w-[600px] text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="px-4 py-3 text-left font-medium">Название</th>
                  <th className="px-4 py-3 text-left font-medium">Адрес</th>
                  <th className="px-4 py-3 text-left font-medium">
                    Часовой пояс
                  </th>
                  <th className="px-4 py-3 text-left font-medium">
                    Дата создания
                  </th>
                  <th className="px-4 py-3 text-right font-medium w-24">
                    Действия
                  </th>
                </tr>
              </thead>
              <tbody>
                {locations?.map((loc, index) => (
                  <tr
                    key={loc.id ?? `loc-${loc.name}-${index}`}
                    className="border-b last:border-0 hover:bg-muted/30 transition-colors"
                  >
                    <td className="px-4 py-3 font-medium">{loc.name}</td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {loc.address}
                    </td>
                    <td className="px-4 py-3">{loc.timezone}</td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {formatDate(loc.createdAt)}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            setLocationToEdit(loc);
                            setUpdateModalOpen(true);
                          }}
                          disabled={!loc.id}
                          aria-label="Редактировать локацию"
                        >
                          <Pencil className="size-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => loc.id && deleteLocation(loc.id)}
                          disabled={isDeleting || !loc.id}
                          className="text-destructive hover:text-destructive hover:bg-destructive/10"
                          aria-label="Удалить локацию"
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
      <CreateLocationModal open={open} onOpenChange={setOpen} />
      <UpdateLocationModal
        open={updateModalOpen}
        onOpenChange={(open) => {
          setUpdateModalOpen(open);
          if (!open) setLocationToEdit(null);
        }}
        location={locationToEdit}
      />
      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
