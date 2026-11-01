document.addEventListener("DOMContentLoaded", function () {
    // Event delegation for all .btn-open-modal buttons
    document.addEventListener("click", function (e) {
        const button = e.target.closest(".btn-open-modal");
        if (!button) return;

        e.preventDefault();
        
        const modal = document.querySelector(".custom-modal") || document.getElementById("carDetailsModal") || document.getElementById("categoryDetailsModal");
        if (!modal) return;
        
        const modalContent = modal.querySelector("#modal-body-content");
        const closeBtn = modal.querySelector(".custom-modal-close");
        
        let endpoint = button.getAttribute("data-url");
        
        if (!endpoint) {
            const carId = button.getAttribute("data-car-id");
            const categoryId = button.getAttribute("data-category-id");
            const actionType = button.getAttribute("data-action-type");
            
            if (carId) {
                if (actionType === 'edit') endpoint = `/Cars/Edit/${carId}`;
                else if (actionType === 'delete') endpoint = `/Cars/Delete/${carId}`;
                else endpoint = `/Cars/Details/${carId}`;
            } else if (categoryId) {
                if (actionType === 'edit') endpoint = `/Categories/Edit/${categoryId}`;
                else if (actionType === 'delete') endpoint = `/Categories/Delete/${categoryId}`;
                else if (actionType === 'create') endpoint = `/Categories/Create`;
                else endpoint = `/Categories/Details/${categoryId}`;
            }
        }

        if (!endpoint) return;

        modal.style.display = "block";
        modalContent.innerHTML = '<div class="loading-spinner">Incarcare date...</div>';

        fetch(endpoint, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error("Eroare la obtinerea datelor.");
            }
            return response.text();
        })
        .then(html => {
            modalContent.innerHTML = html;
            
            if (window.$ && window.$.validator) {
                const form = modalContent.querySelector("form");
                if (form) {
                    $.validator.unobtrusive.parse(form);
                }
            }
            
            const innerCloseBtn = modalContent.querySelector('.custom-modal-close-btn');
            if (innerCloseBtn) {
                innerCloseBtn.addEventListener('click', function() {
                    modal.style.display = "none";
                });
            }
        })
        .catch(error => {
            console.error("Eroare AJAX:", error);
            modalContent.innerHTML = '<p class="text-danger" style="padding: 20px; text-align: center;">Eroare la incarcare.</p>';
        });
        
        if (closeBtn) {
            closeBtn.onclick = function() {
                modal.style.display = "none";
            }
        }
        
        // This handles clicking outside the modal
        window.onclick = function(event) {
            if (event.target === modal) {
                modal.style.display = "none";
            }
        }
    });

    // Also delegate form submission inside modals
    document.addEventListener('submit', function (event) {
        const form = event.target;
        // Check if the form is inside a modal
        if (form.closest('.custom-modal-content')) {
            event.preventDefault();
            const formData = new FormData(form);

            fetch(form.action, {
                method: form.method || 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .then(response => {
                // Dacă serverul face redirect, fetch-ul îl va urmări implicit. 
                // Pentru API-uri, ar trebui să returneze JSON.
                if(response.redirected) {
                    window.location.href = response.url;
                    return null;
                }
                
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.indexOf("application/json") !== -1) {
                    return response.json();
                } else {
                    return response.text();
                }
            })
            .then(data => {
                if (data === null) return; // a fost redirect
                
                if (typeof data === 'object') {
                    if (data.success) {
                        location.reload();
                    } else {
                        alert(data.message || 'A aparut o eroare.');
                    }
                } else {
                    // Dacă e HTML (probabil eroare de validare pe model)
                    const modalContent = form.closest('#modal-body-content');
                    if (modalContent) {
                        modalContent.innerHTML = data;
                        if (window.$ && window.$.validator) {
                            $.validator.unobtrusive.parse(modalContent.querySelector('form'));
                        }
                    }
                }
            })
            .catch(error => {
                console.error('Form submission error:', error);
                alert('A aparut o eroare la trimiterea formularului.');
            });
        }
    });
});
