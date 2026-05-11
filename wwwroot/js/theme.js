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