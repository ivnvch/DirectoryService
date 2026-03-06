"use client";

import { AppSidebar } from "@/shared/components/sidebar/app.sidebar";
import { SidebarInset, SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import Header from "../../features/header/header";
import { queryClient } from "@/shared/api/query-client";
import { Toaster } from "sonner";

export function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider>
        <AppSidebar />
        <SidebarInset>
          <Header />
          <main className="p-6 md:p-10">{children}</main>
          <Toaster position="top-center" duration={3000} richColors={true} />
        </SidebarInset>
      </SidebarProvider>
    </QueryClientProvider>
  );
}
