import axios from "axios";

export const apiClient = axios.create({
    baseURL: "http://localhost:5036/api",
    headers: {"Content-Type": "application/json" },
})