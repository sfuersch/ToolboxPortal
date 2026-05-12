window.toggleTheme = function () {
    const isDark = document.body.classList.toggle("dark");

    localStorage.setItem("theme", isDark ? "dark" : "light");
};

window.loadTheme = function () {
    const theme = localStorage.getItem("theme");

    if (theme === "dark") {
        document.body.classList.add("dark");
    }
};
window.copyToClipboard = async function (text) {
    await navigator.clipboard.writeText(text);
};
window.toggleSidebar = function () {
    const shell = document.getElementById("portalShell");

    if (!shell) {
        return;
    }

    shell.classList.toggle("sidebar-collapsed");

    localStorage.setItem(
        "sidebar-collapsed",
        shell.classList.contains("sidebar-collapsed")
            ? "1"
            : "0"
    );
};

window.loadSidebarState = function () {
    const shell = document.getElementById("portalShell");

    if (!shell) {
        return;
    }

    if (localStorage.getItem("sidebar-collapsed") === "1") {
        shell.classList.add("sidebar-collapsed");
    }
};