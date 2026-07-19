import http from "k6/http";
import {check} from "k6";
import {factoryHeaders, runner} from "./util.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:8080";
const BASE_PATH = __ENV.BASE_URL || "dotnet-diagnostics-lab";
const READ_TIMEOUT = __ENV.READ_TIMEOUT || "10s";
const TEST_TYPE = __ENV.TEST_TYPE;

const TEST_CASES = {
    "string-allocation": {path: "memory/string-allocation", params: {iterations: 100, stringLength: 1000}},
    "leak-static": {path: "memory/leak-static", params: {objectCount: 1000, objectSizeBytes: 1000}},
    "gen2-promotion": {path: "memory/gen2-promotion", params: {objectCount: 1000, objectSizeBytes: 10000}},
    "loh-pressure": {path: "memory/loh-pressure", params: {objectCount: 200, objectSizeBytes: 100000}},
    "thread-pool-starvation": {path: "thread/thread-pool-starvation", params: {delayMs: 10000, taskCount: 2}},
    "thread-leak": {path: "thread/thread-leak", params: {delayMs: 10000, taskCount: 2}},
    "lock-contention": {path: "thread/lock-contention", params: {delayMs: 10000, taskCount: 2}},
    "fibonacci": {path: "cpu/fibonacci", params: {sequencePosition: 32}},
};

function buildUrl({path, params}) {
    const query = Object.entries(params)
        .map(([key, value]) => `${key}=${value}`)
        .join("&");

    return `${BASE_URL}/${BASE_PATH}/diagnostics/v1/${path}?${query}`;
}

export const options = {
    insecureSkipTLSVerify: true,

    scenarios: {
        [TEST_TYPE || "undefined_test_type"]: {
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
    if (!TEST_TYPE || !TEST_CASES[TEST_TYPE]) {
        throw new Error(
            `TEST_TYPE inválido ou ausente: "${TEST_TYPE}". 
            Valores aceitos: ${Object.keys(TEST_CASES).join(", ")}`
        );
    }

    return {
        headers: factoryHeaders(),
        url: buildUrl(TEST_CASES[TEST_TYPE])
    };
}

export default function (data) {
    runner(data, READ_TIMEOUT, TEST_TYPE)
}
