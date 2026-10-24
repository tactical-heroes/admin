import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { createContext, SourceTextModule } from "node:vm";
import { test } from "node:test";

const scriptUrl = new URL("../../../src/TacticalHeroes.Admin/Components/SessionMonitor.razor.js", import.meta.url);
const source = await readFile(scriptUrl, "utf8");
const flush = () => new Promise(setImmediate);

async function start(overrides = {}) {
    const state = { calls: 0, status: 204, version: "initial", redirects: [], ...overrides };
    const document = Object.assign(new EventTarget(), {
        visibilityState: state.visibility ?? "visible",
        baseURI: "https://admin.example.test/",
        querySelector: () => ({ dataset: {
            sessionUrl: "/authentication/session",
            refreshUrl: "/authentication/refresh",
            sessionVersion: "initial"
        } })
    });
    const window = new EventTarget();
    const context = createContext({
        document, window, URL, AbortSignal,
        location: {
            pathname: "/heroes", search: "?page=2", hash: "#details",
            replace: value => state.redirects.push(value)
        },
        setInterval: (callback, interval) => { state.tick = callback; state.interval = interval; },
        fetch: async (url, options) => {
            state.calls++;
            assert.equal(url.href, "https://admin.example.test/authentication/session");
            assert.equal(options.credentials, "same-origin");
            assert.equal(options.cache, "no-store");
            assert.equal(options.redirect, "error");
            if (state.error) throw state.error;
            if (state.pending) await state.pending;
            return new Response(null, { status: state.status, headers:
                state.version === null ? {} : { "X-Session-Permissions-Version": state.version } });
        }
    });
    const module = new SourceTextModule(source, { context, identifier: fileURLToPath(scriptUrl) });
    await module.link(() => { throw new Error("Session monitor must not import dependencies."); });
    await module.evaluate();
    return { state, document, window };
}

test("keeps the page when tokens renew with unchanged permissions", async () => {
    const { state } = await start();
    assert.equal(state.calls, 1);
    assert.equal(state.interval, 30000);
    await state.tick();
    assert.equal(state.calls, 2);
    assert.deepEqual(state.redirects, []);
});

test("redirects revoked sessions once and preserves the return URL", async () => {
    const { state, window } = await start({ status: 401 });
    await state.tick();
    window.dispatchEvent(new Event("focus"));
    await flush();
    assert.equal(state.calls, 1);
    assert.equal(state.redirects.length, 1);
    const target = new URL(state.redirects[0]);
    assert.equal(target.pathname, "/authentication/refresh");
    assert.equal(target.searchParams.get("returnUrl"), "/heroes?page=2#details");
});

test("refreshes when another tab changes the session permissions", async () => {
    const { state } = await start();
    state.version = "changed";
    await state.tick();
    assert.equal(state.redirects.length, 1);
});

for (const status of [429, 500, 503]) {
    test(`preserves the page on HTTP ${status} and retries`, async () => {
        const { state } = await start({ status, version: "changed" });
        assert.deepEqual(state.redirects, []);
        state.status = 401;
        await state.tick();
        assert.equal(state.redirects.length, 1);
    });
}

for (const version of [null, ""]) {
    test(`ignores missing version ${JSON.stringify(version)}`, async () => {
        const { state } = await start({ version });
        assert.deepEqual(state.redirects, []);
    });
}

test("retries after a network failure without logging out", async () => {
    const { state } = await start({ error: new TypeError("Network unavailable") });
    assert.deepEqual(state.redirects, []);
    state.error = null;
    state.status = 401;
    await state.tick();
    assert.equal(state.redirects.length, 1);
});

test("pauses hidden tabs and checks when the tab becomes visible", async () => {
    const { state, document } = await start({ visibility: "hidden", status: 401 });
    await state.tick();
    assert.equal(state.calls, 0);
    document.visibilityState = "visible";
    document.dispatchEvent(new Event("visibilitychange"));
    await flush();
    assert.equal(state.redirects.length, 1);
});

test("checks on focus and prevents overlapping requests", async () => {
    const { state, window } = await start();
    let release;
    state.pending = new Promise(resolve => { release = resolve; });
    window.dispatchEvent(new Event("focus"));
    await state.tick();
    window.dispatchEvent(new Event("focus"));
    assert.equal(state.calls, 2);
    release();
    await flush();
    state.pending = null;
    await state.tick();
    assert.equal(state.calls, 3);
    assert.deepEqual(state.redirects, []);
});
