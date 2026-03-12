import { z } from "zod";
import { TIMEZONES } from "@/shared/lib/timezones";

const MIN_LENGTH = 1;
const MAX_NAME_LENGTH = 150;

export const createLocationSchema = z.object({
  name: z
    .string()
    .trim()
    .min(MIN_LENGTH, "Название обязательно")
    .max(MAX_NAME_LENGTH, `Максимум ${MAX_NAME_LENGTH} символов`),
  country: z
    .string()
    .trim()
    .min(MIN_LENGTH, "Страна обязательна")
    .max(MAX_NAME_LENGTH, `Максимум ${MAX_NAME_LENGTH} символов`),
  city: z
    .string()
    .trim()
    .min(MIN_LENGTH, "Город обязателен")
    .max(MAX_NAME_LENGTH, `Максимум ${MAX_NAME_LENGTH} символов`),
  street: z
    .string()
    .trim()
    .min(MIN_LENGTH, "Улица обязательна")
    .max(MAX_NAME_LENGTH, `Максимум ${MAX_NAME_LENGTH} символов`),
  house: z
    .string()
    .trim()
    .min(MIN_LENGTH, "Номер дома обязателен")
    .max(MAX_NAME_LENGTH, `Максимум ${MAX_NAME_LENGTH} символов`),
  apartment: z.string().optional(),
  timezone: z
    .string()
    .min(1, "Выберите часовой пояс")
    .refine(
    (val) => (TIMEZONES as readonly string[]).includes(val),
    "Недопустимый часовой пояс"
  ),
});

export type CreateLocationFormData = z.infer<typeof createLocationSchema>;
