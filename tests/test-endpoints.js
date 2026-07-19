import {factoryHeaders, runner} from "./util.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:8080";
const BASE_PATH = __ENV.BASE_URL || "dotnet-diagnostics-lab";
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
const START_RPS = Number(__ENV.START_RPS || 50);
const STEP_RPS = Number(__ENV.STEP_RPS || 50);
const TOTAL_STEPS = Number(__ENV.STEPS || 8);
const STEP_DURATION_SECONDS = Number(__ENV.STEP_DURATION || 30);

const TEST_CASES = {
    "string-allocation": {path: "memory/string-allocation", params: {iterations: 10, stringLength: 500}},
    "leak-static": {path: "memory/leak-static", params: {objectCount: 100, objectSizeBytes: 200}},
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

    return {
        headers: factoryHeaders(),
        url: buildUrl(TEST_CASES[TEST_TYPE])
    };
}

export default function (data) {
    runner(data, READ_TIMEOUT, TEST_TYPE)
}
