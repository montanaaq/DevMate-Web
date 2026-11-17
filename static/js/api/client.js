import { HttpClient } from "../shared/http.js";

export const api = new HttpClient({ baseURL: "/api" });
