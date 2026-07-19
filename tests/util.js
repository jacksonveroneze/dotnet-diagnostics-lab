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

    if (res.status !== 200) {
        console.error(`Error occurred: ${res.status} - ${res.body}`);
    }

    const ok = check(res, {
        "status is OK": (response) => response.status === 200,
    });

    if (!ok){
        sleep(0.01);
    }
}