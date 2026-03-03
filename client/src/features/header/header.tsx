import Link from "next/link";
import { Search, Video, Bell } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { SidebarTrigger } from "@/shared/components/ui/sidebar";
import { routes } from "@/shared/routes";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 flex h-14 w-full items-center justify-between border-b border-zinc-200/70 bg-white/95 px-4 backdrop-blur shadow-none dark:border-zinc-800 dark:bg-zinc-950/90">
      {/* Левая часть: Логотип и меню */}
      <div className="flex items-center gap-4">
        <SidebarTrigger className="h-10 w-10 rounded-md" />
        <Link href={routes.home} className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-red-600">
            <Video className="h-5 w-5 text-white" />
          </div>
          <span className="text-xl font-bold tracking-tight">
            DirectoryService
          </span>
        </Link>
      </div>

      {/* Центральная часть: Поиск */}
      <div className="flex flex-1 items-center justify-center px-4">
        <div className="flex w-full max-w-xl items-center gap-2">
          <Input
            type="text"
            placeholder="Search"
            className="h-10 flex-1 rounded-full border border-zinc-300 bg-zinc-50 px-4 focus:border-blue-500 dark:border-zinc-700 dark:bg-zinc-800"
          />
          <Button
            size="icon"
            className="h-10 w-12 rounded-full bg-zinc-100 hover:bg-zinc-200 dark:bg-zinc-800 dark:hover:bg-zinc-700"
          >
            <Search className="h-5 w-5" />
          </Button>
        </div>
      </div>

      {/* Правая часть: Уведомления и аватарка */}
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" className="h-10 w-10">
          <Bell className="h-5 w-5" />
        </Button>
        <Button className="h-8 w-8 rounded-full bg-gradient-to-br from-blue-500 to-purple-600 p-0">
          <span className="text-sm font-medium text-white">U</span>
        </Button>
      </div>
    </header>
  );
}
