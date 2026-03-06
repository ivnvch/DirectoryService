import axios from "axios";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./error";

const baseURL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5036/api";

export const apiClient = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.response.use((response) => {
  const data = response.data as Envelope;

  if(data.isError && data.error) {

  }
  return response;
  },
  (error) => {
    if(axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as Envelope;

      if (envelope?.isError && envelope.error) {
        throw new EnvelopeError(envelope.error);
      }
    }

    return Promise.reject(error);
  }
);
