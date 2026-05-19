window.templateEditor = {
    normalizeId: function (selectorOrId) {
        if (!selectorOrId) {
            return "";
        }

        return selectorOrId.startsWith("#")
            ? selectorOrId.substring(1)
            : selectorOrId;
    },

    init: function (selectorOrId, content) {
        const selectorId = this.normalizeId(selectorOrId);
        const element = document.getElementById(selectorId);

        if (!element) {
            console.warn("Editor-Textarea wurde nicht gefunden:", selectorId);
            return;
        }

        if (typeof tinymce === "undefined") {
            console.warn("TinyMCE ist noch nicht geladen.");
            return;
        }

        const existingEditor = tinymce.get(selectorId);

        if (existingEditor) {
            if (content !== undefined) {
                existingEditor.setContent(content || "");
            }

            return;
        }

        element.value = content || element.value || "";

        tinymce.init({
            selector: "#" + selectorId,
            height: 400,
            menubar: false,
            plugins: "lists link table code",
            toolbar: "undo redo | bold italic underline | bullist numlist | link table | code",
            branding: false
        });
    },

    getContent: function (selectorOrId) {
        const selectorId = this.normalizeId(selectorOrId);

        if (typeof tinymce !== "undefined") {
            const editor = tinymce.get(selectorId);

            if (editor) {
                return editor.getContent();
            }
        }

        const element = document.getElementById(selectorId);
        return element ? element.value : "";
    },

    setContent: function (selectorOrId, content) {
        const selectorId = this.normalizeId(selectorOrId);

        if (typeof tinymce !== "undefined") {
            const editor = tinymce.get(selectorId);

            if (editor) {
                editor.setContent(content || "");
                return;
            }
        }

        const element = document.getElementById(selectorId);

        if (element) {
            element.value = content || "";
        }
    },

    destroy: function (selectorOrId) {
        const selectorId = this.normalizeId(selectorOrId);

        if (typeof tinymce === "undefined") {
            return;
        }

        const editor = tinymce.get(selectorId);

        if (editor) {
            editor.remove();
        }
    },

    remove: function (selectorOrId) {
        this.destroy(selectorOrId);
    }
};