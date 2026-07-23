import {factoryHeaders, runner} from "./util.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:7000";
const BASE_PATH = __ENV.BASE_URL || "";

//const BASE_URL = __ENV.BASE_URL || "http://10.0.0.150:8080";
//const BASE_PATH = __ENV.BASE_URL || "dotnet-diagnostics-lab/";

//const BASE_URL = __ENV.BASE_URL || "http://10.0.0.195:8080";
//const BASE_PATH = __ENV.BASE_URL || "dotnet-diagnostics-lab/";

const READ_TIMEOUT = __ENV.READ_TIMEOUT || "10s";
const TEST_TYPE = __ENV.TEST_TYPE;

// Test
const MAIN_SCENARIO = TEST_TYPE || "undefined_test_type";

// Warmup
const WARMUP_RATE = Number(__ENV.WARMUP_RATE || 1);
const WARMUP_DURATION_SECONDS = Number(__ENV.WARMUP_DURATION_SECONDS || 5);

// Shutdown
const SHUTDOWN_RATE = Number(__ENV.SHUTDOWN_RATE || 1);
const SHUTDOWN_DURATION_SECONDS = Number(__ENV.SHUTDOWN_DURATION_SECONDS || 15);

// k6 VUs
const PRE_VUS = Number(__ENV.PRE_VUS || 100);
const MAX_VUS = Number(__ENV.MAX_VUS || 500);

// Carga
const START_RPS = Number(__ENV.START_RPS || 15);
const STEP_RPS = Number(__ENV.STEP_RPS || 50);
const TOTAL_STEPS = Number(__ENV.STEPS || 6);
const STEP_DURATION_SECONDS = Number(__ENV.STEP_DURATION || 10);

const TEST_CASES = {
    "memory-string-allocation": {path: "memory/string-allocation", params: {iterations: 10, stringLength: 250}},
    "memory-leak-static": {path: "memory/leak-static", params: {objectCount: 200, objectSizeBytes: 500}},
    "memory-loh-pressure": {path: "memory/loh-pressure", params: {objectCount: 200, objectSizeBytes: 100000}},
    "memory-leak-event": {path: "memory/leak-event", params: {subscriberCount: 100, payloadSizeBytes: 500}},
    "memory-leak-cache": {path: "memory/leak-cache", params: {objectCount: 10, objectSizeBytes: 5000}},
    "memory-leak-closure": {path: "memory/leak-closure", params: {objectCount: 50, objectSizeBytes: 100000}},
    "memory-leak-cancellation-token-source": {path: "memory/leak-cancellation-token-source", params: {delayMs: 10000, taskCount: 2}},
    "memory-leak-timer": {path: "memory/leak-timer", params: {timerCount: 500, intervalMs: 3600000}},
    "thread-pool-starvation": {path: "thread/thread-pool-starvation", params: {delayMs: 10000, taskCount: 2}},
    "thread-leak": {path: "thread/thread-leak", params: {delayMs: 10000, taskCount: 2}},
    "thread-lock-contention": {path: "thread/lock-contention", params: {delayMs: 10000, taskCount: 2}},
    "cpu-fibonacci": {path: "cpu/fibonacci", params: {sequencePosition: 32}},
    "cpu-regex-backtracking": {path: "cpu/regex-backtracking", params: {inputLength: 25}},
    "exception-argument": {path: "exception/throw", params: {type: "Argument"}, expectedStatus: 400},
    "exception-unhandled": {path: "exception/throw", params: {type: "Unhandled"}, expectedStatus: 500}
};

function buildUrl({path, params}) {
    const query = Object.entries(params)
        .map(([key, value]) => `${key}=${value}`)
        .join("&");

    return `${BASE_URL}/${BASE_PATH}diagnostics/v1/${path}?${query}`;
}

function buildStages() {
    const stages = [];
    for (let i = 0; i < TOTAL_STEPS; i++) {
        stages.push({target: START_RPS + i * STEP_RPS, duration: STEP_DURATION_SECONDS + "s"});
    }
    return stages;
}

export const options = {
    insecureSkipTLSVerify: true,

    scenarios: {
        warmup: {
            executor: "constant-arrival-rate",
            rate: WARMUP_RATE,
            timeUnit: "1s",
            duration: WARMUP_DURATION_SECONDS + "s",
            preAllocatedVUs: 5,
            maxVUs: 10,
            gracefulStop: "0s",
        },
        [MAIN_SCENARIO]: {
            executor: "ramping-arrival-rate",
            startRate: START_RPS,
            timeUnit: "1s",
            preAllocatedVUs: PRE_VUS,
            maxVUs: MAX_VUS,
            startTime: (WARMUP_DURATION_SECONDS + 1) + "s",
            stages: buildStages(),
            tags: {phase: "step", test: MAIN_SCENARIO},
            gracefulStop: "0s",
        },
        shutdown: {
            executor: "constant-arrival-rate",
            rate: SHUTDOWN_RATE,
            timeUnit: "1s",
            duration: SHUTDOWN_DURATION_SECONDS + "s",
            preAllocatedVUs: 5,
            maxVUs: 10,
            startTime: (WARMUP_DURATION_SECONDS + (TOTAL_STEPS * STEP_DURATION_SECONDS) + 2) + "s",
            gracefulStop: "0s",
        },
    },

    thresholds: {
        [`checks{scenario:${MAIN_SCENARIO}}`]: ["rate >= 0.99"],
        [`http_req_failed{scenario:${MAIN_SCENARIO}}`]: ["rate <= 0.01"],
        [`http_req_duration{scenario:${MAIN_SCENARIO}}`]: ["p(95) < 300"],
        [`dropped_iterations{scenario:${MAIN_SCENARIO}}`]: ["count == 0"],
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

    const url = buildUrl(TEST_CASES[TEST_TYPE]);

    console.log(url)

    return {
        headers: factoryHeaders(),
        url: url,
        expectedStatus: TEST_CASES[TEST_TYPE].expectedStatus || 200
    };
}

export default function (data) {
    runner(data, READ_TIMEOUT, TEST_TYPE)
}