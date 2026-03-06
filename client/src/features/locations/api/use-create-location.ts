import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation() {
    const queryClient = useQueryClient();

    const mutation = useMutation({ 
    mutationFn: locationsApi.createLocation,
    onSettled: () =>
         queryClient.invalidateQueries({
            queryKey: [locationsQueryOptions.baseKey]}),
            onError: () => {
                toast.error("Ошибка при добавлении локации");
            },
            onSuccess: () => {
                toast.success("Локация успешно добавлена");
            }
});

return {
    createLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending
  };
}