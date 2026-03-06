/**
 * Часовые пояса: value — IANA для API, label — читаемое название для пользователя.
 */
export type TimezoneOption = {
  value: string;
  label: string;
};

const CITY_LABELS: Record<string, string> = {
  Moscow: "Москва",
  Minsk: "Минск",
  Kyiv: "Киев",
  London: "Лондон",
  Berlin: "Берлин",
  Paris: "Париж",
  Warsaw: "Варшава",
  Istanbul: "Стамбул",
  Almaty: "Алматы",
  Tbilisi: "Тбилиси",
  Yerevan: "Ереван",
  Baku: "Баку",
  Tashkent: "Ташкент",
  Dubai: "Дубай",
  "New York": "Нью-Йорк",
  "Los Angeles": "Лос-Анджелес",
};

function formatLabel(iana: string): string {
  if (iana === "UTC") return "UTC";
  const city = iana.split("/").pop()?.replace(/_/g, " ") ?? iana;
  return CITY_LABELS[city] ?? city;
}

function getOffset(iana: string): string {
  try {
    const formatter = new Intl.DateTimeFormat("ru-RU", {
      timeZone: iana,
      timeZoneName: "shortOffset",
    });
    const parts = formatter.formatToParts(new Date());
    const tz = parts.find((p) => p.type === "timeZoneName");
    return tz?.value ?? "";
  } catch {
    return "";
  }
}

const IANA_LIST = [
  "Europe/Moscow",
  "Europe/Minsk",
  "Europe/Kyiv",
  "Europe/London",
  "Europe/Berlin",
  "Europe/Paris",
  "Europe/Warsaw",
  "Europe/Istanbul",
  "Asia/Almaty",
  "Asia/Tbilisi",
  "Asia/Yerevan",
  "Asia/Baku",
  "Asia/Tashkent",
  "Asia/Dubai",
  "America/New_York",
  "America/Los_Angeles",
  "UTC",
] as const;

/** Опции для Select: label — для отображения, value — для API */
export const TIMEZONE_OPTIONS: TimezoneOption[] = IANA_LIST.map((iana) => {
  const city = formatLabel(iana);
  const offset = getOffset(iana);
  return {
    value: iana,
    label: offset ? `${city} (${offset})` : city,
  };
});

/** Только IANA-значения (для обратной совместимости) */
export const TIMEZONES = IANA_LIST;
