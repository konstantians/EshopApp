function setUpCustomSelect(dropdownButtonId, dropdownLabelId = 'none', dropdownHiddenInputId = 'none', hiddenValueFunction){
    let dropdownButton = document.getElementById(dropdownButtonId);

    if (dropdownLabelId !== 'none') {
        document.getElementById(dropdownLabelId).addEventListener('click', function () {
            dropdownButton.focus();
        });
    }

    const dropdownItems = dropdownButton.nextElementSibling.querySelectorAll('.dropdown-item');
    const hiddenInput = dropdownHiddenInputId !== 'none' ? document.getElementById(dropdownHiddenInputId) : null;

    dropdownItems.forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            if (typeof hiddenValueFunction === 'function') {
                dropdownButton.textContent = hiddenValueFunction(item);
            } else {
                dropdownButton.textContent = item.textContent;
            }
            if (hiddenInput) {
                hiddenInput.value = this.dataset.value;
            }
        });
    });
}

function setDropdownValue(dropdownButtonId, dropdownHiddenInputId, value) {

    const dropdownButton = document.getElementById(dropdownButtonId);
    const hiddenInput = document.getElementById(dropdownHiddenInputId);

    if (!dropdownButton) {
        return;
    }

    const dropdownItems = dropdownButton.nextElementSibling.querySelectorAll('.dropdown-item');
    const item = Array.from(dropdownItems).find(i => i.dataset.value === value);
    dropdownButton.textContent = item.textContent;

    if (!hiddenInput) {
        hiddenInput.value = item.dataset.value;
    }
}