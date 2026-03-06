import { useLocationsQuery } from "@/features/locations/api/use-locations-query";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { QueryErrorAlert } from "@/shared/components/ui/query-error-alert";
import { MapPin } from "lucide-react";
import { useState } from "react";
import { Spinner } from "@/shared/components/ui/spinner";

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
  const [page, setPage] = useState(1);

  const { locations, isPending, error, isError, totalPages } =
    useLocationsQuery({
      page,
    });

  if (isPending) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Spinner className="size-8" />
      </div>
    );
  }

  if (isError) {
    return <div>Ошибка: {error ? error.message : "Неизвестнаня ошибка!"}</div>;
  }

  return (
    <div className="space-y-6 p-6">
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
          (
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
                </tr>
              </thead>
              <tbody>
                {locations?.map((loc, index) => (
                  <tr
                    key={`${loc.name}-${index}`}
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
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          )
        </CardContent>
      </Card>
    </div>
  );
}
