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

function setUpDeleteModalsCustomValidationAndSubmissionProcedures(deleteModalsIdsCommonPart, deleteModalsformIdsCommonPart, deleteModalsInputIdsCommonPart, validationAttribute, validationAttributeErrorText) {
    //remove validation if the user decides to close the modal in case the open it again later
    let deleteModals = document.querySelectorAll(`[id^="${deleteModalsIdsCommonPart}"]`);
    deleteModals.forEach(deleteModal => {
        deleteModal.addEventListener('hidden.bs.modal', function () {
            let inputs = deleteModal.querySelectorAll(`[id^="${deleteModalsInputIdsCommonPart}"]`);
            inputs.forEach(input => {
                input.setAttribute('data-custom-validation', 'false');
                input.nextElementSibling.textContent = '';
                input.value = '';
            });
        });
    });

    //form submission logic
    document.querySelectorAll(`[id^="${deleteModalsformIdsCommonPart}"]`).forEach(form => {
        form.addEventListener('submit', function (e) {
            let modalDeleteInput = form.querySelector('input');
            let modalDeleteValidationSpan = form.querySelector('span');
            if (modalDeleteInput.getAttribute('data-custom-validation') !== 'true' || modalDeleteValidationSpan.textContent !== '') {
                if (modalDeleteInput.value === "") {
                    modalDeleteValidationSpan.textContent = "This field is required";
                }

                e.preventDefault();
            }
        });
    });

    //focusout input logic. It is similar to input logic
    let modalDeleteInputs = document.querySelectorAll(`[id^="${deleteModalsInputIdsCommonPart}"]`);
    modalDeleteInputs.forEach(modalDeleteInput => {
        modalDeleteInput.addEventListener("focusout", function () {
            modalDeleteInput.setAttribute('data-custom-validation', 'true');
            let correctValue = modalDeleteInput.getAttribute(validationAttribute);
            if (modalDeleteInput.value === "") {
                modalDeleteInput.nextElementSibling.textContent = "This field is required";
            }
            else if (modalDeleteInput.value !== correctValue) {
                modalDeleteInput.nextElementSibling.textContent = validationAttributeErrorText;
            }
            else {
                modalDeleteInput.nextElementSibling.textContent = '';
            }
        });
    });

    //input/user typing input logic. It is similar to focusout logic
    modalDeleteInputs.forEach(modalDeleteInput => {
        modalDeleteInput.addEventListener("input", function () {
            let correctValue = modalDeleteInput.getAttribute(validationAttribute);
            if (modalDeleteInput.value === "" && modalDeleteInput.getAttribute('data-custom-validation') === 'true') {
                modalDeleteInput.nextElementSibling.textContent = "This field is required";
            }
            else if (modalDeleteInput.value !== correctValue && modalDeleteInput.getAttribute('data-custom-validation') === 'true') {
                modalDeleteInput.nextElementSibling.textContent = validationAttributeErrorText;
            }
            else if (modalDeleteInput.value === correctValue && modalDeleteInput.getAttribute('data-custom-validation') === 'true') {
                modalDeleteInput.nextElementSibling.textContent = '';
            }
        });
    });
}