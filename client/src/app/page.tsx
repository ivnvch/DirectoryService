"use client";

import Todo from "@/shared/components/todo";
import { Building2, MapPin, Network, Users } from "lucide-react";

export default function Home() {
  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-zinc-200/70 bg-white/80 p-6 shadow-sm backdrop-blur dark:border-zinc-800 dark:bg-zinc-900/70">
        <h1 className="text-2xl font-semibold tracking-tight md:text-3xl">
          DirectoryService
        </h1>
        <p className="mt-3 max-w-3xl text-sm leading-6 text-zinc-600 dark:text-zinc-300">
          Проект для централизованного управления структурой компании: отделами,
          должностями, локациями и связанными сущностями. Интерфейс помогает
          быстро находить нужные данные, поддерживать единый справочник и
          упрощать навигацию по организационной структуре.
        </p>
        <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div className="flex items-center gap-2 rounded-lg border border-zinc-200/70 bg-zinc-50/80 px-3 py-2 text-sm dark:border-zinc-800 dark:bg-zinc-900">
            <Building2 className="h-4 w-4 text-blue-500" />
            <span>Отделы</span>
          </div>
          <div className="flex items-center gap-2 rounded-lg border border-zinc-200/70 bg-zinc-50/80 px-3 py-2 text-sm dark:border-zinc-800 dark:bg-zinc-900">
            <Network className="h-4 w-4 text-blue-500" />
            <span>Должности</span>
          </div>
          <div className="flex items-center gap-2 rounded-lg border border-zinc-200/70 bg-zinc-50/80 px-3 py-2 text-sm dark:border-zinc-800 dark:bg-zinc-900">
            <MapPin className="h-4 w-4 text-blue-500" />
            <span>Локации</span>
          </div>
          <div className="flex items-center gap-2 rounded-lg border border-zinc-200/70 bg-zinc-50/80 px-3 py-2 text-sm dark:border-zinc-800 dark:bg-zinc-900">
            <Users className="h-4 w-4 text-blue-500" />
            <span>Сотрудники</span>
          </div>
        </div>
      </section>
      <main>{/* <Todo /> */}</main>
    </div>
  );
}
