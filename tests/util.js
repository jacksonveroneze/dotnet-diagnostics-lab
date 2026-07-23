import http from "k6/http";
import { check, sleep } from "k6";

export function factoryHeaders(token) {
    return {
        'Accept': "application/json",
    };
}

export function runner(data, timeout, testType) {
    const headers = {
        ...data.headers,
        "x-correlation-id": crypto.randomUUID(),
    };

    const res = http.get(data.url, {
        timeout: timeout,
        headers: headers,
        tags: { name: testType },
    });

    const expectedStatus = data.expectedStatus || 200;

    if (res.status !== expectedStatus) {
        console.error(`Error occurred: ${res.status} - ${res.body}`);
    }

    const ok = check(res, {
        "status is expected": (response) => response.status === expectedStatus,
    });

    if (!ok){
        sleep(0.01);
    }
}