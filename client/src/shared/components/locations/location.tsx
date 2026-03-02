import { locationsApi } from "@/entities/locations/api";
import type { Location } from "@/entities/locations/types";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { MapPin } from "lucide-react";
import { useEffect, useState } from "react";
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

const PAGE_SIZE = 10;
const TOTAL_COUNT = 15;
const TOTAL_PAGES = 5;

export default function Location() {
  const [page, setPage] = useState(1);
  const [locations, setLocations] = useState<Location[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    queueMicrotask(() => {
      if (!cancelled) {
        setIsLoading(true);
        setError(null);
      }
    });

    locationsApi
      .getLocations({
        totalCount: TOTAL_COUNT,
        page,
        pageSize: PAGE_SIZE,
        totalPages: TOTAL_PAGES,
      })
      .then((data) => {
        if (!cancelled) setLocations(data);
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err?.message ?? "Не удалось загрузить локации");
          setLocations([]);
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [page]);

  if (isLoading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Spinner className="size-8" />
      </div>
    );
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
            Всего: {isLoading ? "—" : locations.length} записей
          </CardDescription>
        </CardHeader>
        <CardContent>
          {error ? (
            <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-destructive">
              {error}
            </div>
          ) : locations.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center text-muted-foreground">
              <MapPin className="mb-4 size-12 opacity-50" />
              <p className="font-medium">Нет локаций</p>
              <p className="text-sm">Данные пока отсутствуют</p>
            </div>
          ) : (
            <div className="overflow-x-auto rounded-lg border">
              <table className="w-full min-w-[600px] text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="px-4 py-3 text-left font-medium">
                      Название
                    </th>
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
                  {locations.map((loc, index) => (
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
          )}
        </CardContent>
      </Card>
    </div>
  );
}
