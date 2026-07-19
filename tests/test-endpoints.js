import http from "k6/http";
import {check} from 'k6';
import {factoryHeaders} from "./util.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:8080/dotnet-diagnostics-lab";
const READ_TIMEOUT = __ENV.READ_TIMEOUT || "10s";
const TEST_TYPE = __ENV.TEST_TYPE;

const SCENARIOS = {
    "string-allocation": () => {
        const iterations = 100;
        const stringLength = 1000;

        return `${BASE_URL}/diagnostics/v1/memory/string-allocation?iterations=${iterations}&stringLength=${stringLength}`;
    },
    "leak-static": () => {
        const objectCount = 1000;
        const objectSizeBytes = 1000;

        return `${BASE_URL}/diagnostics/v1/memory/leak-static?objectCount=${objectCount}&objectSizeBytes=${objectSizeBytes}`;
    },
    "gen2-promotion": () => {
        const objectCount = 1000;
        const objectSizeBytes = 10000;

        return `${BASE_URL}/diagnostics/v1/memory/gen2-promotion?objectCount=${objectCount}&objectSizeBytes=${objectSizeBytes}`;
    },
    "loh-pressure": () => {
        const objectCount = 200;
        const objectSizeBytes = 100000;

        return `${BASE_URL}/diagnostics/v1/memory/loh-pressure?objectCount=${objectCount}&objectSizeBytes=${objectSizeBytes}`;
    },
    "thread-pool-starvation": () => {
        const delayMs = 10000;
        const taskCount = 2;

        return `${BASE_URL}/diagnostics/v1/thread/thread-pool-starvation?delayMs=${delayMs}&taskCount=${taskCount}`;
    },
    "thread-leak": () => {
        const delayMs = 10000;
        const taskCount = 2;

        return `${BASE_URL}/diagnostics/v1/thread/thread-leak?delayMs=${delayMs}&taskCount=${taskCount}`;
    },
    "lock-contention": () => {
        const delayMs = 10000;
        const taskCount = 2;

        return `${BASE_URL}/diagnostics/v1/thread/lock-contention?delayMs=${delayMs}&taskCount=${taskCount}`;
    },
    "fibonacci": () => {
        const n = 32;

        return `${BASE_URL}/diagnostics/v1/cpu/fibonacci?n=${n}`;
    },
};

export const options = {
    insecureSkipTLSVerify: true,

    scenarios: {
        get_profile: {
            executor: "ramping-arrival-rate",
            startRate: 1,
            timeUnit: "1s",

            stages: [
                {duration: "5s", target: 1},
                {duration: "15s", target: 50},
                {duration: "30s", target: 50},
                {duration: "120s", target: 75},
                {duration: "30s", target: 0},
            ],

            preAllocatedVUs: 200,
            maxVUs: 250,
            gracefulStop: "30s",
        },
    },

    thresholds: {
        checks: ["rate>=0.99"],
        http_req_failed: ["rate<=0.01"],
        http_req_duration: ["p(95)<300"],
        dropped_iterations: ["count==0"],
    },

    summaryTrendStats: ["avg", "min", "med", "p(90)", "p(95)", "p(99)", "max"],
};

export function setup() {
    if (!TEST_TYPE || !SCENARIOS[TEST_TYPE]) {
        throw new Error(
            `TEST_TYPE inválido ou ausente: "${TEST_TYPE}". Valores aceitos: ${Object.keys(SCENARIOS).join(", ")}`
        );
    }

    const headers = factoryHeaders();

    return {headers};
}

export default function (data) {
    const headers = {
        ...data.headers,
        "x-correlation-id": crypto.randomUUID(),
    };

    const url = SCENARIOS[TEST_TYPE]();

    console.log(url)

    const res = http.get(url, {
        timeout: READ_TIMEOUT,
        headers: headers,
        tags: {name: "GET Stress Test"},
    });

    if(res.status !== 200) {
        console.error(`Error occurred: ${res.status} - ${res.body}`);
    }

    check(res, {
        "status is OK": (r) => r.status === 200,
    });
}