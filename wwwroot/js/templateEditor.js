window.templateEditor = {
    init: async function (selector, value) {
        if (!window.tinymce) {
            throw new Error("TinyMCE wurde nicht geladen.");
        }

        const element = document.querySelector(selector);

        if (!element) {
            throw new Error("Editor-Textarea wurde nicht gefunden.");
        }
        const id = selector.replace("#", "");

        if (tinymce.get(id)) {
            tinymce.get(id).setContent(value || "");
            return;
        }

        await tinymce.init({
            selector: selector,
            height: 500,
            menubar: false,
            plugins: "lists link table code autolink",
            toolbar:
                "undo redo | bold italic underline | " +
                "alignleft aligncenter alignright | " +
                "bullist numlist | link table | code",
            branding: false,
            promotion: false,
            content_style:
                "body { font-family:Inter,Arial,sans-serif; font-size:14px; }",
            setup: function (editor) {
                editor.on("init", function () {
                    editor.setContent(value || "");
                });
            }
        });
    },

    getContent: function (selector) {
        const editor = tinymce.get(selector.replace("#", ""));

        if (!editor) {
            return "";
        }

        return editor.getContent();
    },

    destroy: function (selector) {
        const editor = tinymce.get(selector.replace("#", ""));

        if (editor) {
            editor.remove();
        }
    }
};