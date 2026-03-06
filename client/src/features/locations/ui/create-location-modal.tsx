import {
  buildAddressDto,
  locationsQueryOptions,
} from "@/entities/locations/api";
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
import { useState } from "react";
import { useCreateLocation } from "../api/use-create-location";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

const initialFormData = {
  name: "",
  country: "",
  city: "",
  street: "",
  house: "",
  apartment: "",
  timezone: "Europe/Moscow",
};

export default function CreateLocationModal({ open, onOpenChange }: Props) {
  const [formData, setFormData] = useState(initialFormData);

  const { createLocation, isPending, error } = useCreateLocation();

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    createLocation(
      {
        name: formData.name,
        address: buildAddressDto({
          country: formData.country,
          city: formData.city,
          street: formData.street,
          house: formData.house,
          apartment: formData.apartment,
        }),
        timezone: formData.timezone,
      },
      {
        onSuccess: () => {
          setFormData(initialFormData);
          onOpenChange(false);
        },
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Добавление локации</DialogTitle>
          <DialogDescription>
            Заполните форму для добавления новой локации
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4 py-4" onSubmit={handleSubmit}>
          <div className="grid gap-2">
            <Label htmlFor="name">Название</Label>
            <Input
              id="name"
              placeholder="Введите название локации"
              value={formData.name}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, name: e.target.value }))
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="country">Страна</Label>
            <Input
              id="country"
              placeholder="Название страны"
              value={formData.country}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, country: e.target.value }))
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="city">Город</Label>
            <Input
              id="city"
              placeholder="Название города"
              value={formData.city}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, city: e.target.value }))
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="street">Улица</Label>
            <Input
              id="street"
              placeholder="Название улицы"
              value={formData.street}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, street: e.target.value }))
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="house">№ дома</Label>
            <Input
              id="house"
              placeholder="Номер дома"
              value={formData.house}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, house: e.target.value }))
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="apartment">Квартира (необязательно)</Label>
            <Input
              id="apartment"
              placeholder="Номер квартиры"
              value={formData.apartment}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, apartment: e.target.value }))
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="timezone">Часовой пояс</Label>
            <Select
              id="timezone"
              value={formData.timezone}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  timezone: e.target.value,
                }))
              }
            >
              <option value="">Выберите часовой пояс</option>
              {TIMEZONE_OPTIONS.map(({ value, label }) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </Select>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Сохранение..." : "Добавить локацию"}
            </Button>
            {error && <div className="text-red-500">{error.message}</div>}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
