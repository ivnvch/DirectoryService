import { buildAddressDto } from "@/entities/locations/api";
import type { Location } from "@/entities/locations/types";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Select } from "@/shared/components/ui/select";
import { TIMEZONE_OPTIONS } from "@/shared/lib/timezones";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useUpdateLocation } from "../api/use-update-location";
import { parseAddressString } from "../model/parse-address";
import {
  createLocationSchema,
  type CreateLocationFormData,
} from "../model/create-location-schema";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  location: Location | null;
};

export default function UpdateLocationModal({
  open,
  onOpenChange,
  location,
}: Props) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateLocationFormData>({
    resolver: zodResolver(createLocationSchema),
  });

  const { updateLocation, isPending } = useUpdateLocation();

  useEffect(() => {
    if (open && location) {
      const parsed = parseAddressString(location.address);
      reset({
        name: location.name,
        country: parsed.country,
        city: parsed.city,
        street: parsed.street,
        house: parsed.house,
        apartment: parsed.apartment,
        timezone: location.timezone || "Europe/Moscow",
      });
    }
  }, [open, location, reset]);

  const onSubmit = (data: CreateLocationFormData) => {
    if (!location?.id) return;
    updateLocation(
      {
        id: location.id,
        data: {
          name: data.name,
          address: buildAddressDto({
            country: data.country,
            city: data.city,
            street: data.street,
            house: data.house,
            apartment: data.apartment,
          }),
          timezone: data.timezone,
        },
      },
      {
        onSuccess: () => {
          onOpenChange(false);
        },
      }
    );
  };

  if (!location) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Редактирование локации</DialogTitle>
          <DialogDescription>
            Измените данные локации и сохраните
          </DialogDescription>
        </DialogHeader>

        <form
          className="grid gap-4 py-4"
          onSubmit={handleSubmit(onSubmit)}
        >
          <div className="grid gap-2">
            <Label htmlFor="name">Название</Label>
            <Input
              id="name"
              placeholder="Введите название локации"
              aria-invalid={!!errors.name}
              {...register("name")}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>
          <div className="grid gap-2">
            <Label htmlFor="country">Страна</Label>
            <Input
              id="country"
              placeholder="Название страны"
              aria-invalid={!!errors.country}
              {...register("country")}
            />
            {errors.country && (
              <p className="text-sm text-destructive">
                {errors.country.message}
              </p>
            )}
          </div>
          <div className="grid gap-2">
            <Label htmlFor="city">Город</Label>
            <Input
              id="city"
              placeholder="Название города"
              aria-invalid={!!errors.city}
              {...register("city")}
            />
            {errors.city && (
              <p className="text-sm text-destructive">{errors.city.message}</p>
            )}
          </div>
          <div className="grid gap-2">
            <Label htmlFor="street">Улица</Label>
            <Input
              id="street"
              placeholder="Название улицы"
              aria-invalid={!!errors.street}
              {...register("street")}
            />
            {errors.street && (
              <p className="text-sm text-destructive">
                {errors.street.message}
              </p>
            )}
          </div>
          <div className="grid gap-2">
            <Label htmlFor="house">№ дома</Label>
            <Input
              id="house"
              placeholder="Номер дома"
              aria-invalid={!!errors.house}
              {...register("house")}
            />
            {errors.house && (
              <p className="text-sm text-destructive">
                {errors.house.message}
              </p>
            )}
          </div>
          <div className="grid gap-2">
            <Label htmlFor="apartment">Квартира (необязательно)</Label>
            <Input
              id="apartment"
              placeholder="Номер квартиры"
              {...register("apartment")}
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="timezone">Часовой пояс</Label>
            <Select
              id="timezone"
              aria-invalid={!!errors.timezone}
              {...register("timezone")}
            >
              <option value="">Выберите часовой пояс</option>
              {TIMEZONE_OPTIONS.map(({ value, label }) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </Select>
            {errors.timezone && (
              <p className="text-sm text-destructive">
                {errors.timezone.message}
              </p>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Сохранение..." : "Сохранить"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
