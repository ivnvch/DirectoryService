import { ApiError } from "./error";

export type Envelope<T = unknown> = {
  items: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};