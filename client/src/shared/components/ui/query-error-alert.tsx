type QueryErrorAlertProps = {
  error: { message?: string } | null;
  onRetry?: () => void;
  className?: string;
};

export function QueryErrorAlert({
  error,
  onRetry,
  className,
}: QueryErrorAlertProps) {
  if (!error) return null;

  return (
    <div
      role="alert"
      className={`rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-destructive ${className ?? ""}`}
    >
      <p className="font-medium">{error.message ?? "Произошла ошибка"}</p>
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="mt-2 text-sm underline hover:no-underline"
        >
          Повторить
        </button>
      )}
    </div>
  );
}
