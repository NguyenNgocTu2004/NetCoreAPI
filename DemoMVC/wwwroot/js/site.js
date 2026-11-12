(() => {
    const modalElement = document.getElementById("crud-modal");
    if (!modalElement) {
        return;
    }

    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
    const titleElement = document.getElementById("crud-modal-title");
    const contentElement = document.getElementById("crud-modal-content");
    const modalFeedback = document.getElementById("crud-modal-feedback");
    const pageFeedback = document.getElementById("crud-alert");

    function showFeedback(element, message, isError) {
        element.textContent = message;
        element.classList.remove("d-none", "alert-success", "alert-danger");
        element.classList.add(isError ? "alert-danger" : "alert-success");
    }

    function hideFeedback(element) {
        element.textContent = "";
        element.classList.add("d-none");
        element.classList.remove("alert-success", "alert-danger");
    }

    function prepareValidation(container) {
        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
            window.jQuery(container).find("form").each(function () {
                window.jQuery.validator.unobtrusive.parse(this);
            });
        }
    }

    async function readError(response) {
        try {
            const result = await response.json();
            if (result && result.message) {
                return result.message;
            }
        } catch {
            // Non-JSON responses use the status-based message below.
        }

        if (response.status === 404) {
            return "Bản ghi không còn tồn tại.";
        }
        if (response.status >= 500) {
            return "Máy chủ gặp lỗi khi xử lý yêu cầu. Vui lòng thử lại sau.";
        }
        return "Yêu cầu không thành công. Vui lòng kiểm tra dữ liệu và thử lại.";
    }

    async function refreshList() {
        const container = document.querySelector("[data-list-url]");
        if (!container) {
            return;
        }

        const url = new URL(container.dataset.listUrl, window.location.origin);
        const searchInput = document.querySelector('input[name="searchString"]');
        if (searchInput && searchInput.value.trim()) {
            url.searchParams.set("searchString", searchInput.value.trim());
        }

        const response = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });
        if (!response.ok) {
            throw new Error(await readError(response));
        }

        container.innerHTML = await response.text();
    }

    document.addEventListener("click", async (event) => {
        const link = event.target.closest("a[data-crud-modal]");
        if (!link || modalElement.dataset.loading === "true") {
            return;
        }

        event.preventDefault();
        modalElement.dataset.loading = "true";
        titleElement.textContent = link.dataset.modalTitle || link.textContent.trim();
        contentElement.innerHTML = '<div class="text-center py-4" role="status">Đang tải...</div>';
        hideFeedback(modalFeedback);
        modal.show();

        try {
            const response = await fetch(link.href, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            if (!response.ok) {
                throw new Error(await readError(response));
            }

            contentElement.innerHTML = await response.text();
            prepareValidation(contentElement);
        } catch (error) {
            showFeedback(
                modalFeedback,
                error instanceof Error ? error.message : "Không thể kết nối đến máy chủ.",
                true
            );
            contentElement.replaceChildren();
        } finally {
            modalElement.dataset.loading = "false";
        }
    });

    document.addEventListener("submit", async (event) => {
        const form = event.target.closest("#crud-modal-content form");
        if (!form) {
            return;
        }

        event.preventDefault();
        if (form.dataset.submitting === "true") {
            return;
        }

        const $form = window.jQuery ? window.jQuery(form) : null;
        if ($form && $form.data("validator") && !$form.valid()) {
            return;
        }

        form.dataset.submitting = "true";
        form.setAttribute("aria-busy", "true");
        const submitButtons = Array.from(form.querySelectorAll('button[type="submit"], input[type="submit"]'));
        const originalMarkup = submitButtons.map((button) => button.innerHTML);
        submitButtons.forEach((button) => {
            button.disabled = true;
        });
        if (submitButtons.length > 0) {
            submitButtons[0].innerHTML =
                '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Đang xử lý...';
        }
        hideFeedback(modalFeedback);

        try {
            const response = await fetch(form.action, {
                method: form.method || "POST",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json"
                }
            });

            if (response.status === 422) {
                contentElement.innerHTML = await response.text();
                prepareValidation(contentElement);
                return;
            }
            if (!response.ok) {
                throw new Error(await readError(response));
            }

            const result = await response.json();
            modal.hide();
            showFeedback(pageFeedback, result.message || "Thao tác thành công.", false);

            try {
                await refreshList();
            } catch (error) {
                showFeedback(
                    pageFeedback,
                    `${result.message || "Thao tác thành công."} Không thể tải lại danh sách; vui lòng làm mới trang.`,
                    true
                );
            }
        } catch (error) {
            showFeedback(
                modalFeedback,
                error instanceof Error ? error.message : "Không thể kết nối đến máy chủ.",
                true
            );
        } finally {
            form.dataset.submitting = "false";
            form.removeAttribute("aria-busy");
            submitButtons.forEach((button, index) => {
                button.disabled = false;
                button.innerHTML = originalMarkup[index];
            });
        }
    });
})();
