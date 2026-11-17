import { errorMessages } from "./constants/error-messages.js";

export class HttpError extends Error {
    constructor(status, body) {
        super(typeof body?.message === "string" ? body.message : errorMessages.requestFailed);
        this.name = "HttpError";
        this.status = status;
        this.body = body;
    }
}

export class HttpClient {
    constructor({ baseURL }) {
        this.baseURL = baseURL;
    }

    get(path) {
        return this.request({ method: "GET", url: path });
    }

    post(path, data) {
        return this.request({ method: "POST", url: path, data });
    }

    async request({ method, url, data }) {
        const options = { method, credentials: "same-origin", cache: "no-store" };

        if (data !== undefined) {
            options.headers = { "Content-Type": "application/json" };
            options.body = JSON.stringify(data);
        }

        let response;
        try {
            response = await fetch(`${this.baseURL}${url}`, options);
        } catch (cause) {
            throw new Error(errorMessages.connectionFailed, { cause });
        }

        let responseBody = null;
        if (response.status !== 204) {
            try {
                responseBody = await response.json();
            } catch (cause) {
                throw new Error(errorMessages.requestFailed, { cause });
            }
        }

        if (!response.ok) {
            throw new HttpError(response.status, responseBody);
        }

        return { data: responseBody, status: response.status, headers: response.headers };
    }
}
