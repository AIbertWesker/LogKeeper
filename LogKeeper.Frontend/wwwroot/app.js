const state = {
    apiBase: "https://localhost:7290",
    cursorStack: [],
    currentCursor: null,
    lastPage: null
};

const elements = {
    apiBase: document.getElementById("apiBase"),
    applyApi: document.getElementById("applyApi"),
    level: document.getElementById("level"),
    application: document.getElementById("application"),
    correlationId: document.getElementById("correlationId"),
    clientIp: document.getElementById("clientIp"),
    messageContains: document.getElementById("messageContains"),
    from: document.getElementById("from"),
    to: document.getElementById("to"),
    pageSize: document.getElementById("pageSize"),
    applyFilters: document.getElementById("applyFilters"),
    refresh: document.getElementById("refresh"),
    prev: document.getElementById("prev"),
    next: document.getElementById("next"),
    logRows: document.getElementById("logRows"),
    details: document.getElementById("details"),
    detailsContent: document.getElementById("detailsContent"),
    stats: document.getElementById("stats")
};

const setApiBase = (value) => {
    state.apiBase = value.replace(/\/$/, "");
    elements.apiBase.value = state.apiBase;
};

const readFilters = () => {
    const filters = {
        level: elements.level.value.trim(),
        application: elements.application.value.trim(),
        correlationId: elements.correlationId.value.trim(),
        clientIp: elements.clientIp.value.trim(),
        messageContains: elements.messageContains.value.trim(),
        from: elements.from.value,
        to: elements.to.value
    };

    return Object.fromEntries(Object.entries(filters).filter(([, value]) => value));
};

const buildQuery = () => {
    const query = new URLSearchParams();
    const pageSize = Number(elements.pageSize.value || 50);
    query.set("pageSize", pageSize.toString());

    if (state.currentCursor) {
        query.set("cursorTimestamp", state.currentCursor);
    }

    const filters = readFilters();
    for (const [key, value] of Object.entries(filters)) {
        query.set(key, value);
    }

    return query.toString();
};

const fetchLogs = async () => {
    const query = buildQuery();
    const response = await fetch(`${state.apiBase}/logs?${query}`);
    if (!response.ok) {
        throw new Error(`API error: ${response.status}`);
    }

    return await response.json();
};

const renderRows = (items) => {
    elements.logRows.innerHTML = "";

    for (const item of items) {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${new Date(item.timestamp).toISOString()}</td>
            <td>${item.level}</td>
            <td>${item.application ?? "-"}</td>
            <td>${item.message}</td>
        `;

        row.addEventListener("click", () => showDetails(item));
        elements.logRows.appendChild(row);
    }
};

const showDetails = (item) => {
    elements.details.hidden = false;
    elements.detailsContent.textContent = JSON.stringify(item, null, 2);
};

const renderStats = (page) => {
    elements.stats.textContent = `Total: ${page.totalCount} | Page size: ${page.currentSize}`;
    elements.next.disabled = !page.hasMore;
    elements.prev.disabled = state.cursorStack.length === 0;
};

const loadPage = async () => {
    try {
        const page = await fetchLogs();
        state.lastPage = page;
        renderRows(page.items);
        renderStats(page);
    } catch (error) {
        elements.stats.textContent = error.message;
    }
};

const refresh = async () => {
    state.cursorStack = [];
    state.currentCursor = null;
    elements.details.hidden = true;
    await loadPage();
};

const nextPage = async () => {
    if (!state.lastPage?.nextCursorTimestamp) {
        return;
    }

    state.cursorStack.push(state.currentCursor);
    state.currentCursor = state.lastPage.nextCursorTimestamp;
    await loadPage();
};

const previousPage = async () => {
    if (state.cursorStack.length === 0) {
        return;
    }

    state.currentCursor = state.cursorStack.pop();
    await loadPage();
};

const attachEvents = () => {
    elements.applyApi.addEventListener("click", () => {
        setApiBase(elements.apiBase.value);
        refresh();
    });

    elements.applyFilters.addEventListener("click", refresh);
    elements.refresh.addEventListener("click", loadPage);
    elements.next.addEventListener("click", nextPage);
    elements.prev.addEventListener("click", previousPage);
};

const init = () => {
    setApiBase(state.apiBase);
    attachEvents();
    refresh();
};

init();
