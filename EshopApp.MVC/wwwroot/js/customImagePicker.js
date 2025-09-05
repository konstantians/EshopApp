function setImagePicker(imagePickerInputId, imagePickerModalId, imagePickerGalleryId, galleryToggleIconId, shouldChangeImagePickerInput = true, isModal = true) {
    const imagePickerInput = document.getElementById(imagePickerInputId);
    const imagePickerModal = document.getElementById(imagePickerModalId);
    const imagePickerGallery = document.getElementById(imagePickerGalleryId);
    const galleryToggleIcon = document.getElementById(galleryToggleIconId);
    const imageSearch = imagePickerModal.querySelector('input[id^="imageSearch"]');
    const imageUpload = imagePickerModal.querySelector('input[id^="imageUpload"]');
    const imageGrid = imagePickerModal.querySelector('div[id^="imageGrid"]');
    const saveImagesButton = imagePickerModal.querySelector('button[id^="saveImagesButton"]');
    const cancelSaveImagesButton = imagePickerModal.querySelector('button[id^="cancelSaveImagesButton"]');

    let imagesThatAreConfirmedSelected = [];

    const modal = new bootstrap.Modal(imagePickerModal, { focus: false });
    if (isModal) {
        imagePickerInput.addEventListener('click', () => {
            imagePickerModal.querySelectorAll('.image-selectable.selected').forEach(selectedImage => {
                if (selectedImage.classList.contains('selected')) {
                    imagesThatAreConfirmedSelected.push(selectedImage.dataset.id);
                }
            });
            modal.show();
        });
    }

    if (galleryToggleIcon && imagePickerGallery) {
        galleryToggleIcon.addEventListener("click", function () {
            this.classList.toggle("fa-eye");
            this.classList.toggle("fa-eye-slash");
            let imagePickerGalleryContainer = imagePickerGallery.parentElement;
            if (imagePickerGalleryContainer.classList.contains('d-none')) {
                imagePickerGalleryContainer.classList.remove('d-none');
                this.classList.remove("fa-eye");
                this.classList.add("fa-eye-slash");
            }
            else {
                imagePickerGalleryContainer.classList.add('d-none');
                this.classList.add("fa-eye");
                this.classList.remove("fa-eye-slash");
            }
        });
    }

    // Search filter
    imageSearch.addEventListener('input', function () {
        const query = this.value.toLowerCase();
        imagePickerModal.querySelectorAll('#imageGrid .image-selectable').forEach(div => {
            const name = div.dataset.name.toLowerCase();
            div.parentElement.style.display = name.includes(query) ? '' : 'none';
        });
    });

    // Upload new image (preview inside grid)
    imageUpload.addEventListener('change', async function (e) {
        const file = e.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (ev) {
            const newCol = document.createElement('div');
            newCol.className = 'col-6 col-sm-4 col-lg-2';
            newCol.innerHTML = `
                <div class="image-selectable text-center p-1 border" data-id="new" data-name="${file.name}">
                    <img src="${ev.target.result}" height="135" style="width:100%" />
                    <div class="text-truncate p-1" style="background-color:white;" title="${file.name}">
                        ${file.name}
                    </div>
                </div>
            `;
            imageGrid.prepend(newCol);
        };
        reader.readAsDataURL(file);

        const formData = new FormData();
        formData.append('imageFile', file);

        try {
            const response = await fetch("/ImageManagement/UploadImage", {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorData = await response.json();
                let errorMessage = errorData.errorMessage || 'Upload failed';
                if (response.status === 500) {
                    errorMessage = "Upload failed due to unexpected server error. Please contact the administrator if the error persists.";
                }
                else if (response.status === 503) {
                    errorMessage = "Upload failed due to one or more services being currently unavailable. Please contact the administrator if the error persists.";
                }
                else if (errorMessage === "DuplicateEntityName") {
                    errorMessage = "Upload failed, because there is another picture with the same name already uploaded to the server.";
                }

                throw new Error(errorMessage);
            }

            const result = await response.json();

            //replace the id with the correct one and add the listener
            const newDiv = imagePickerModal.querySelector('.image-selectable[data-id="new"]');
            if (newDiv) {
                newDiv.dataset.id = result.ImageId;
                newDiv.addEventListener('click', () => {
                    if (newDiv.classList.contains('selected')) {
                        newDiv.classList.remove('selected');
                    }
                    else {
                        newDiv.classList.add('selected');
                    }
                });
            }
        } catch (errorMessage) {
            console.error(errorMessage);
            alert(errorMessage);
            //remove the image if the upload failed
            const newDiv = imagePickerModal.querySelector('.image-selectable[data-id="new"]');
            if (newDiv) newDiv.parentElement.remove();
        }
    });

    imagePickerModal.querySelectorAll('.image-selectable').forEach(div => {
        div.addEventListener('click', () => {
            if (div.classList.contains('selected')) {
                div.classList.remove('selected');
            }
            else {
                div.classList.add('selected');
            }
        });
    });

    saveImagesButton.addEventListener('click', () => {
        const selectedImages = imagePickerModal.querySelectorAll('.image-selectable.selected');
        const selectedIds = [];
        imagesThatAreConfirmedSelected = [];

        imagePickerGallery.innerHTML = "";
        if (imagePickerGallery && selectedImages.length == 0) {
            imagePickerGallery.previousElementSibling.classList.remove('d-none');
            //let alert = createNoImagesAlert();
            //imagePickerGallery.appendChild(alert);

            if (isModal) {
                modal.hide();
            }
            else {
                imagePickerModal.classList.add('d-none');
            }

            return;
        }

        imagePickerGallery.previousElementSibling.classList.add('d-none');
        selectedImages.forEach(selectedImage => {
            if (selectedImage.classList.contains('selected')) {
                selectedIds.push(selectedImage.dataset.id);
                imagesThatAreConfirmedSelected.push(selectedImage.dataset.id);
            }

            if (imagePickerGallery) {
                const wrapper = document.createElement("div");
                dataColStructure = imagePickerGallery.dataset.colStructure;
                wrapper.className = dataColStructure ?? "col-6 col-sm-4 col-lg-2";
                const clone = selectedImage.cloneNode(true);
                clone.classList.remove('image-selectable', 'selected');
                let img = clone.querySelector('img');
                img.style.width = '100%';
                if (imagePickerGallery.hasAttribute("data-dont-create-image-modals")) {
                    wrapper.appendChild(clone);
                }
                else {
                    img.style.cursor = 'pointer';
                    let uniqueGuid = 'xxxx-xxxx-xxxx'.replace(/[x]/g, () =>
                        Math.floor(Math.random() * 16).toString(16)
                    );
                    img.setAttribute('data-bs-target', `#imageModal-${uniqueGuid}`);
                    img.addEventListener('click', function () {
                        openImage(this);
                    });

                    let newImageModal = createImageModal(`imageModal-${uniqueGuid}`);

                    wrapper.appendChild(clone);
                    wrapper.appendChild(newImageModal);
                }
                
                imagePickerGallery.appendChild(wrapper);
            }
        });

        if (selectedImages && selectedImages.length > 0) {
            const hiddenInput = document.getElementById(imageGrid.dataset.saveToInputWithId);
            hiddenInput.value = selectedIds.join(',');

            if (shouldChangeImagePickerInput) {
                const selectedImageNames = Array.from(selectedImages).map(div => div.dataset.name);
                imagePickerInput.value = selectedImageNames.join(', ');
            }
        }

        if (isModal) {
            modal.hide();
        }
        else {
            imagePickerModal.classList.add('d-none');
        }
    });

    cancelSaveImagesButton.addEventListener('click', () => {
        const selectedImages = imagePickerModal.querySelectorAll('.image-selectable');
        selectedImages.forEach(selectedImage => {
            if (!isModal) {
                imagePickerModal.classList.add('d-none');
            }

            if (imagesThatAreConfirmedSelected.includes(selectedImage.dataset.id)) {
                selectedImage.classList.add('selected');
            }
            else {
                selectedImage.classList.remove('selected');
            }
        });
    })

    function imagePickerProceduresForResponsiveness() {
        uploadFileButtonText = imagePickerModal.querySelector('span[id^="uploadFileButtonText"]');
        uploadFileButtonIcon = imagePickerModal.querySelector('i[id^="uploadFileButtonIcon"]');
        if (window.innerWidth < 576) {
            uploadFileButtonText.textContent = "";
            uploadFileButtonIcon.classList.remove('me-1');
        } else if (window.innerWidth < 992) {
            uploadFileButtonText.textContent = " Προσθήκη";
            uploadFileButtonIcon.classList.add('me-1');
        } else {
            uploadFileButtonText.textContent = " Προσθήκη στο δίσκο";
            uploadFileButtonIcon.classList.add('me-1');
        }
    }

    window.addEventListener("load", imagePickerProceduresForResponsiveness);
    window.addEventListener("resize", imagePickerProceduresForResponsiveness);
}

function openImage(img) {
    const modalSelector = img.getAttribute('data-bs-target');
    const imageModal = document.querySelector(modalSelector);
    const modal = new bootstrap.Modal(imageModal);

    const modalImage = imageModal.querySelector("img");
    modalImage.src = img.src;

    modal.show();

    modalImage.onload = () => {
        const dialog = imageModal.firstElementChild;

        // Calculate width: image natural width or 95% viewport, whichever is smaller
        const imgWidth = modalImage.naturalWidth;
        const maxWidth = window.innerWidth * 0.95;

        dialog.style.width = Math.min(imgWidth, maxWidth) + 'px';
    };

    function resizeModalImage() {
        if (window.innerWidth < 576) {
            imageModal.firstElementChild.classList.remove('modal-dialog-centered');
            modalImage.style.maxWidth = "370px";
            modalImage.style.maxHeight = "370px";
        }
        else {
            imageModal.firstElementChild.classList.add('modal-dialog-centered');
            modalImage.style.maxWidth = "500px";
            modalImage.style.maxHeight = "500px";
        }
    }

    resizeModalImage();
    window.addEventListener("load", resizeModalImage);
    window.addEventListener("resize", resizeModalImage);
}

function createImageModal(modalId) {
    // Create modal container
    const modal = document.createElement('div');
    modal.className = 'modal fade custom-center-modal-for-image';
    modal.id = modalId;
    modal.tabIndex = -1;
    modal.setAttribute('aria-hidden', 'true');

    // Modal dialog
    const dialog = document.createElement('div');
    dialog.className = 'modal-dialog modal-dialog-centered';
    dialog.style.margin = 'auto';

    // Modal content
    const content = document.createElement('div');
    content.className = 'modal-content border-0 shadow-none p-0';

    // Modal header
    const header = document.createElement('div');
    header.className = 'modal-header border-2 p-2 mb-1';

    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'btn-close';
    closeButton.setAttribute('data-bs-dismiss', 'modal');
    closeButton.setAttribute('aria-label', 'Close');

    header.appendChild(closeButton);

    // Modal body
    const body = document.createElement('div');
    body.className = 'modal-body d-flex justify-content-center p-0';

    const img = document.createElement('img');
    img.src = '';
    img.style.maxHeight = '370px';
    img.style.maxWidth = '370px';
    img.style.width = 'auto';
    img.style.height = 'auto';

    body.appendChild(img);

    // Assemble modal
    content.appendChild(header);
    content.appendChild(body);
    dialog.appendChild(content);
    modal.appendChild(dialog);

    return modal;
}

function createNoImagesAlert() {
    // Create alert div
    const alert = document.createElement("div");
    alert.className = "alert d-flex align-items-center";
    const root = document.documentElement;
    const accentColor = getComputedStyle(root).getPropertyValue('--accent-orange').trim();
    alert.style.background = accentColor;
    alert.style.opacity = "0.9";
    alert.setAttribute("role", "alert");

    // Create icon (Font Awesome)
    const icon = document.createElement("i");
    icon.className = "fa-solid fa-triangle-exclamation me-2";

    // Create text
    const text = document.createElement("div");
    text.textContent = "Δεν υπάρχουν επιλεγμένες εικόνες για την παραλλαγή.";

    // Put everything together
    alert.appendChild(icon);
    alert.appendChild(text);

    return alert;
}