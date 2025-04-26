function showPopUpModal(modalId, title, message, titleSmall, messageSmall, type = "error") {
    const modal = document.getElementById(modalId);
    if (window.innerWidth >= 992) {
        modal.querySelector('h4').textContent = title;
        modal.querySelector('p').textContent = message;
    }
    else {
        modal.querySelector('h4').textContent = titleSmall;
        modal.querySelector('p').textContent = messageSmall;
    }

    let alertIcon = modal.querySelector('i');
    if (type === "error") {
        alertIcon.classList.add('fa-xmark');
        alertIcon.classList.remove('fa-check');
    }
    else if (type === "success") {
        alertIcon.classList.remove('fa-xmark');
        alertIcon.classList.add('fa-check');
    }

    if (typeof resultModal.show === 'function') {
        console.warn('Found resultModal.show() being called somewhere incorrectly');
    }
    const modalInstance = bootstrap.Modal.getOrCreateInstance(modal);
    modalInstance.show();
}