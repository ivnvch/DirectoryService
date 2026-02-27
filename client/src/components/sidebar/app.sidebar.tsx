"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Building2, Globe, Home, Layers3, LayoutDashboard } from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "../ui/sidebar";
import { cn } from "@/lib/utils";
import { useSidebar } from "@/components/ui/sidebar";
import { routes } from "@/shared/routes";

const navigationItems = [
  { title: "Main", href: routes.home, icon: Home },
  { title: "Departments", href: routes.departments, icon: Building2 },
  { title: "Positions", href: routes.positions, icon: Layers3 },
  { title: "Locations", href: routes.locations, icon: Globe },
] as const;

export function AppSidebar() {
  const pathname = usePathname();

  const { setOpenMobile } = useSidebar();

  return (
    <Sidebar
      collapsible="icon"
      className="border-r border-zinc-200/70 bg-white/95 backdrop-blur dark:border-zinc-800 dark:bg-zinc-950/90"
    >
      <SidebarHeader className="h-14 justify-center overflow-hidden border-b border-zinc-200/70 px-3 py-2 dark:border-zinc-800 group-data-[collapsible=icon]:px-2">
        <div className="flex h-10 items-center gap-2 rounded-lg border border-zinc-200/80 bg-zinc-50 px-3 transition-all duration-200 dark:border-zinc-800 dark:bg-zinc-900 group-data-[collapsible=icon]:w-8 group-data-[collapsible=icon]:justify-center group-data-[collapsible=icon]:gap-0 group-data-[collapsible=icon]:px-0">
          <LayoutDashboard className="h-4 w-4 text-blue-500" />
          <span className="whitespace-nowrap text-sm font-semibold tracking-wide transition-all duration-200 group-data-[collapsible=icon]:pointer-events-none group-data-[collapsible=icon]:-translate-x-2 group-data-[collapsible=icon]:opacity-0">
            Navigation
          </span>
        </div>
      </SidebarHeader>

      <SidebarContent className="px-2 py-3 group-data-[collapsible=icon]:px-1">
        <SidebarGroup className="group-data-[collapsible=icon]:p-1">
          <SidebarGroupLabel className="px-2 text-[11px] uppercase tracking-wider text-zinc-500">
            Directory
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu className="group-data-[collapsible=icon]:items-center">
              {navigationItems.map((item) => {
                const isActive = pathname === item.href;
                const Icon = item.icon;

                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      onClick={() => setOpenMobile(false)}
                      asChild
                      isActive={isActive}
                      tooltip={item.title}
                      className="group relative h-10 rounded-lg transition-all duration-200 hover:translate-x-0.5 hover:bg-zinc-100/80 data-[active=true]:bg-blue-50 data-[active=true]:text-blue-700 data-[active=true]:shadow-sm dark:hover:bg-zinc-800/70 dark:data-[active=true]:bg-blue-500/15 dark:data-[active=true]:text-blue-200 group-data-[collapsible=icon]:mx-auto"
                    >
                      <Link href={item.href} className="relative">
                        <span
                          aria-hidden="true"
                          className={cn(
                            "absolute top-1/2 -left-1 h-6 w-1 -translate-y-1/2 rounded-full bg-blue-500 transition-all duration-200 group-data-[collapsible=icon]:hidden",
                            isActive ? "opacity-100" : "opacity-0"
                          )}
                        />
                        <Icon className="h-4 w-4" />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarRail />
    </Sidebar>
  );
}
