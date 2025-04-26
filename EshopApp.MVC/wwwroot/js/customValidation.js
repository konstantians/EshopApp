function setUpValidationListeners() {
    document.querySelectorAll("input, select, textarea").forEach(field => {
        field.dataset.validated = "false";

        field.addEventListener("focusout", function () {
            field.dataset.validated = "true";
            //if there are more validation errors and the checkpasswordmatch had errors, it is going to be overriden as intended
            validateField(field);
        });

        field.addEventListener("input", function () {
            if (field.dataset.validated === "true") {
                clearError(field);
                validateField(field);
            }
        });
    });
}

function setUpFormSubmissionValidationListener(formId) {
    document.getElementById(formId).onsubmit = function (event) {
        let isValid = true;

        //pick the inputs, selects and textareas of this form
        this.querySelectorAll("input, select, textarea").forEach(field => {
            if (field.type !== "password" && typeof field.value === "string") {
                field.value = field.value.trim();
            }

            if (!validateField(field)) {
                isValid = false;
            }
        });

        if (!isValid) {
            event.preventDefault();
        }
    };
}

function validateField(field) {
    //required validation
    if (field.hasAttribute('data-val-required') && field.value.trim() === "") {
        showValidationErrror(field, field.getAttribute("data-val-required"));
        return false;
    }
    //if it is empty but not required then it passed validation
    else if (!field.hasAttribute('data-val-required') && field.value.trim() === "") {
        return true;
    }

    //check password regex(not dynamic as of now)
    if ("type" in field && field.type === "password") {
        password = field.value;
        if (password.length < 8) {
            showValidationErrror(field, "Password must be at least 8 characters long.");
            return false;
        }
        if (password.length > 128) {
            showValidationErrror(field, "Password can not exceed 128 characters.");
            return false;
        }
        if (!/[a-z]/.test(password)) {
            showValidationErrror(field, "Password must contain at least one lowercase letter.");
            return false;
        }
        if (!/[A-Z]/.test(password)) {
            showValidationErrror(field, "Password must contain at least one uppercase letter.");
            return false;
        }
        if (!/\d/.test(password)) {
            showValidationErrror(field, "Password must contain at least one digit.");
            return false;
        }
        if (!/[\W_]/.test(password)) {
            showValidationErrror(field, "Password must contain at least one special character.");
            return false;
        }
    }

    //regex validation if data-val-regex-pattern exists
    if (field.hasAttribute("data-val-regex-pattern")) {
        const regexPattern = new RegExp(field.getAttribute("data-val-regex-pattern"));
        if (!regexPattern.test(field.value.trim())) {
            showValidationErrror(field, field.getAttribute("data-val-regex"));
            return false;
        }
    }

    //Browser's built-in validation, works well for email
    if (!field.checkValidity()) {
        showValidationErrror(field);
        return false;
    }

    if (!checkPasswordMatch(field)) {
        return false;
    }

    return true;
}

function checkPasswordMatch(field) {
    if (field.type !== "password") return true; // Checks to see if it a password field

    const passwordField = document.querySelector('input[type="password"][data-compare-password="1"]');
    const repeatPasswordField = document.querySelector('input[type="password"][data-compare-password="2"]');

    if (!passwordField || !repeatPasswordField) return true;

    const passwordFieldNameAttribute = passwordField.getAttribute('name');
    const passwordErrorSpan = document.querySelector(`span[data-valmsg-for="${passwordFieldNameAttribute}"]`);
    const repeatPasswordFieldNameAttribute = repeatPasswordField.getAttribute('name');
    const repeatPasswordErrorSpan = document.querySelector(`span[data-valmsg-for="${repeatPasswordFieldNameAttribute}"]`);

    // Delay validation until user unfocuses repeat field at least once
    if (repeatPasswordField.dataset.validated === "false") return true;

    if (passwordField.value !== repeatPasswordField.value) {
        repeatPasswordErrorSpan.textContent = "Passwords do not match.";
        repeatPasswordErrorSpan.classList.add("text-danger");
        passwordErrorSpan.textContent = "Passwords do not match.";
        passwordErrorSpan.classList.add("text-danger");
        return false;
    }

    clearError(repeatPasswordField);
    clearError(passwordField);
    return true;
}

function showValidationErrror(field, message) {
    const fieldNameAttribute = field.getAttribute('name');
    const errorSpan = document.querySelector(`span[data-valmsg-for="${fieldNameAttribute}"]`);
    if (errorSpan) {
        errorSpan.textContent = message ?? field.validationMessage;
        errorSpan.classList.add("text-danger");
    }
}

function clearError(field) {
    const fieldNameAttribute = field.getAttribute('name');
    const errorSpan = document.querySelector(`span[data-valmsg-for="${fieldNameAttribute}"]`);
    if (errorSpan) {
        errorSpan.textContent = "";
        errorSpan.classList.remove("text-danger");
    }
}
