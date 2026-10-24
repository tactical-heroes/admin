const monitor = document.querySelector("script[data-session-url]");
let checking = false;
let refreshing = false;

async function checkSession() {
    if (document.visibilityState !== "visible" || checking || refreshing) {
        return;
    }

    checking = true;
    try {
        const response = await fetch(new URL(monitor.dataset.sessionUrl, document.baseURI), {
            credentials: "same-origin",
            cache: "no-store",
            redirect: "error",
            signal: AbortSignal.timeout(10000)
        });
        const version = response.headers.get("X-Session-Permissions-Version");
        if (response.status === 401 || (response.ok && version && version !== monitor.dataset.sessionVersion)) {
            refreshing = true;
            const refreshUrl = new URL(monitor.dataset.refreshUrl, document.baseURI);
            refreshUrl.searchParams.set("returnUrl", location.pathname + location.search + location.hash);
            location.replace(refreshUrl.href);
        }
    } catch {
        // A temporary network failure does not invalidate the session.
    } finally {
        checking = false;
    }
}

setInterval(checkSession, 30000);
document.addEventListener("visibilitychange", checkSession);
window.addEventListener("focus", checkSession);
void checkSession();
