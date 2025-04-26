function showAlert(alertId, title, message, titleSmall, messageSmall, fadeOutDelay = 5000, hideDelay = 15000, type = "error") {
    const alertDiv = document.getElementById(alertId);
    if (window.innerWidth >= 1200) {
        alertDiv.querySelector('h4').textContent = title;
        alertDiv.querySelector('p').textContent = message;
    }
    else {
        alertDiv.querySelector('h4').textContent = titleSmall;
        alertDiv.querySelector('p').textContent = messageSmall;
    }

    let alertIcon = alertDiv.querySelector('i');
    if (type === "error") {
        alertIcon.classList.add('fa-circle-xmark');
        alertIcon.classList.remove('fa-circle-check');
    }
    else if (type === "success") {
        alertIcon.classList.remove('fa-circle-xmark');
        alertIcon.classList.add('fa-circle-check');
    }

    alertDiv.classList.remove('d-none');
    alertDiv.style.opacity = 1; //in case I want the alert to reappear without a page refresh

    setTimeout(() => { alertDiv.style.opacity = 0; }, fadeOutDelay);
    setTimeout(() => { alertDiv.classList.add('d-none') }, hideDelay); // Auto-hide after 15 secondss
}

function hideAlert(button) {
    let alertDiv = button.parentElement;
    alertDiv.classList.add("d-none");
}