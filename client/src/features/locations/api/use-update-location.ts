import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof locationsApi.updateLocation>[1] }) =>
      locationsApi.updateLocation(id, data),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      }),
    onError: () => {
      toast.error("Ошибка при обновлении локации");
    },
    onSuccess: (_, variables) => {
      toast.success(`'${variables.data.name}' успешно обновлена`);
    },
  });

  return {
    updateLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
