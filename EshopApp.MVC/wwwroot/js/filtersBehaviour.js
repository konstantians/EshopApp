let filtersSection = document.getElementById('filtersSection');
const filterToggleButton = document.getElementById('filterToggleButton');
let isOpen = false;
let previousInnerWidth = window.innerWidth;

if (filterToggleButton) {
    filterToggleButton.addEventListener('click', () => {
        isOpen = !isOpen;

        filtersSection.style.transform = isOpen ? 'translateX(0)' : 'translateX(-100%)';

        if (isOpen && window.innerWidth >= 768)
            filterToggleButton.style.left = '50%';
        else if (isOpen && window.innerWidth < 768)
            filterToggleButton.style.left = '75%';
        else if (!isOpen)
            filterToggleButton.style.left = '0';
    });
}

document.querySelectorAll('.toggle-check').forEach(li => {
    li.addEventListener('click', function (event) {
        if (li.classList.contains('li-disabled')) {
            return;
        }

        const checkbox = li.querySelector('input[type="checkbox"]');
        //Don't toggle if the checkbox itself was clicked
        if (event.target !== checkbox) {
            checkbox.checked = !checkbox.checked;
        }
    });
});

function updateFiltersLayout() {
    if (!filtersSection || !filterToggleButton)
        return;

    //fixes an edge case bug where everything goes to hell at exactly 1200 pixels
    if (window.innerWidth === 1200) {
        filtersSection.style.width = '33.3333%';
    }
    else {
        filtersSection.style.width = '';
    }

    if (window.innerWidth < 1650) {
        filtersSection.classList.remove('col-3');
        filtersSection.classList.add('col-4');
    }
    else if (window.innerWidth >= 1650) {
        filtersSection.classList.add('col-3');
        filtersSection.classList.remove('col-4');
    }

    if (window.innerWidth < 1200) {
        filtersSection.classList.remove('col-3');
        filtersSection.classList.remove('col-4');

        if (previousInnerWidth >= 1200) {
            isOpen = false;
            filtersSection.style.transform = 'translateX(-100%)';
        }

        if (!filtersSection.classList.contains('addTransitionLaterToMobileStyles')) {
            setTimeout(() => {
                filtersSection.classList.add('addTransitionLaterToMobileStyles');
            }, 500);
        }

        if (previousInnerWidth >= 768 && window.innerWidth < 768 && isOpen) {
            filterToggleButton.style.transition = 'none';
            filterToggleButton.style.left = '75%';

            setTimeout(() => {
                filterToggleButton.style.transition = 'left 0.4s ease';
            }, 500);
        }
        else if (previousInnerWidth < 768 && window.innerWidth >= 768 && isOpen) {
            filterToggleButton.style.transition = 'none';
            filterToggleButton.style.left = '50%';

            setTimeout(() => {
                filterToggleButton.style.transition = 'left 0.4s ease';
            }, 500);
        }
    }
    else {
        filtersSection.classList.remove('addTransitionLaterToMobileStyles');
        filtersSection.style.transform = 'translateX(0)';
        filterToggleButton.style.left = '0';
        isOpen = false; // reset toggle state
    }

    previousInnerWidth = window.innerWidth;
}

window.addEventListener("load", updateFiltersLayout);
window.addEventListener("resize", updateFiltersLayout);

let dropdown = document.getElementById('pagesDropdown');
if (dropdown) {
    dropdown.addEventListener('click', function () {
        var dropdownMenu = dropdown.nextElementSibling;
        if (dropdownMenu && dropdownMenu.classList.contains('show') && window.innerWidth < 576) {
            dropdownMenu.style.left = '-0.05px';
        }
        else {
            dropdownMenu.style.left = '';
        }
    });
}

document.querySelectorAll('.option-dropdown').forEach(dropdown => {
    const button = dropdown.querySelector('button');
    const items = dropdown.querySelectorAll('.dropdown-item');

    items.forEach(item => {
        item.addEventListener('click', e => {
            e.preventDefault(); // prevent the "#" link from jumping
            button.innerText = item.innerText;
        });
    });
});

/********* Pagination Logic *********/

/*1 page nothing appears (total li 0)
2 pages 2 pages then all arrow button appear (total li 6)
3 pages 3 pages then all arrow button appear  (total li 7)
4 pages 4 pages then all arrow button appear  (total li 8)
5 pages 5 pages then all arrow button appear   (total li 9)
6 pages all 5 pages appear and the '...' li and all the arrow buttons appear (total li 10)
... from this point the logic remains the same
*/

let paginationLisCount = document.querySelectorAll('.pagination .page-item');
let paginationUl = document.getElementById('paginationUl');

let goToFirstPaginationLi = document.getElementById('goToFirstPaginationLi');
let goToPreviousPaginationLi = document.getElementById('goToPreviousPaginationLi');
let goToNextPaginationLi = document.getElementById('goToNextPaginationLi');
let goToLastPaginationLi = document.getElementById('goToLastPaginationLi');
let totalItems = paginationUl.getAttribute('data-total-items');
let itemsPerPage = 18;
let totalPages = Math.ceil(totalItems / 18);
let currentPage = 1;

document.querySelectorAll('.pagination .page-item.page-li').forEach(function (pageLi) {
    pageLi.addEventListener('click', function (e) {
        handleClickOnPaginationLi(e.currentTarget);
        e.preventDefault();

        const activePageLink = document.querySelector('.pagination .page-item.active a');
        currentPage = parseInt(activePageLink?.textContent || "1");
        document.dispatchEvent(new CustomEvent('pagination:pageChange'));
    });
});

if (goToFirstPaginationLi) {
    goToFirstPaginationLi.addEventListener('click', function (e) {
        currentPage = 1;
        handleGoToFirstLi();
        e.preventDefault();
        document.dispatchEvent(new CustomEvent('pagination:pageChange'));
    });
}

if (goToLastPaginationLi) {
    goToLastPaginationLi.addEventListener('click', function (e) {
        currentPage = goToNextPaginationLi.previousElementSibling.querySelector('a').textContent.trim();
        handleGoToLastLi();
        e.preventDefault();
        document.dispatchEvent(new CustomEvent('pagination:pageChange'));
    });
}

if (goToPreviousPaginationLi) {
    goToPreviousPaginationLi.addEventListener('click', function (e) {
        if (currentPage != 1) {
            currentPage--;
        }

        handleGoToPreviousLi();
        e.preventDefault();

        const activePageLink = document.querySelector('.pagination .page-item.active a');
        const newPage = parseInt(activePageLink?.textContent || "1");

        document.dispatchEvent(new CustomEvent('pagination:pageChange'));
    });
}

if (goToNextPaginationLi) {
    goToNextPaginationLi.addEventListener('click', function (e) {
        if (currentPage != goToNextPaginationLi.previousElementSibling.querySelector('a').textContent.trim()) {
            currentPage++;
        }

        handleGoToNextLi();
        e.preventDefault();

        const activePageLink = document.querySelector('.pagination .page-item.active a');
        const newPage = parseInt(activePageLink?.textContent || "1");

        document.dispatchEvent(new CustomEvent('pagination:pageChange'));
    });
}

function handleGoToFirstLi() {
    let firstPageLi = goToPreviousPaginationLi.nextElementSibling;
    let firstPageLiValue = parseInt(firstPageLi.querySelector('a').textContent.trim());
    let lastPageLi = goToNextPaginationLi.previousElementSibling;
    let lastPageLiValue = parseInt(lastPageLi.querySelector('a').textContent.trim());

    //do nothing if firstPage is already the one active
    if (firstPageLiValue === 1 && firstPageLi.classList.contains('active')) {
        return;
    }

    //if we have 5 or less pages
    if (totalPages <= 5) {
        document.querySelectorAll('.pagination .page-item').forEach(item => item.classList.remove('active'));
        firstPageLi.classList.add('active'); 
        return;
    }

    //every other case
    //this solves the bug when there are less than 5 pages
    if (firstPageLiValue === 1 && !firstPageLi.classList.contains('active') && lastPageLiValue <= 5) {
        document.querySelectorAll('.pagination .page-item').forEach(item => item.classList.remove('active'));
        firstPageLi.classList.add('active'); 
        return;
    }

    document.querySelectorAll('.pagination .page-item').forEach(li => {
        li.classList.remove('active');

        if (li.previousElementSibling && li.previousElementSibling.classList.contains('disabled')) {
            return;
        }

        const currentLiAElement = li.querySelector('a');
        if (currentLiAElement) {
            const currentLiAElementValue = currentLiAElement.textContent.trim();

            if (!isNaN(currentLiAElementValue)) {
                currentLiAElement.textContent = currentLiAElementValue - (firstPageLiValue - 1);
            }
            /// the ... li that might be hidden
            else if (currentLiAElementValue === '…') {
                li.classList.remove('d-none');
            }
        }
    });
    firstPageLi.classList.add('active');
}

function handleGoToLastLi() {
    let lastPageLi = goToNextPaginationLi.previousElementSibling;

    //do nothing if lastPage is already the one active
    if (lastPageLi.classList.contains('active')) {
        return;
    }

    //if we have 5 or less pages
    if (totalPages <= 5) {
        document.querySelectorAll('.pagination .page-item').forEach(item => item.classList.remove('active'));
        lastPageLi.classList.add('active');
        return;
    }


    //every other case
    let lastPageLiValue = parseInt(lastPageLi.querySelector('a').textContent.trim());
    if (lastPageLiValue <= 5) {
        document.querySelectorAll('.pagination .page-item').forEach(item => item.classList.remove('active'));
        lastPageLi.classList.add('active');
    }

    let previousSibling = lastPageLi.previousElementSibling;
    let counter = 1;
    while (counter < 5) {
        previousSibling.classList.remove('active');
        const previousSiblingAElement = previousSibling.querySelector('a');
        if (previousSiblingAElement) {
            const previousSiblingAElementValue = previousSiblingAElement.textContent.trim();

            if (!isNaN(previousSiblingAElementValue)) {
                previousSiblingAElement.textContent = lastPageLiValue - counter;
                counter++;
            }
            /// the ... li that might be hidden
            else if (previousSiblingAElementValue === "…") {
                previousSibling.classList.add('d-none');
            }
        }

        previousSibling = previousSibling.previousElementSibling;
    }

    lastPageLi.classList.add('active');
}

function handleGoToPreviousLi() {
    let firstPageLi = goToPreviousPaginationLi.nextElementSibling;
    let lastPageLi = goToNextPaginationLi.previousElementSibling;
    let firstPageLiValue = parseInt(firstPageLi.querySelector('a').textContent.trim());
    let lastPageLiValue = parseInt(lastPageLi.querySelector('a').textContent.trim());
    //page one is active
    if (firstPageLiValue === 1 && firstPageLi.classList.contains('active')) {
        return;
    }

    //if we have 5 or less pages
    if (totalPages <= 5) {
        let activeLi = document.querySelector('.pagination .page-item.active');
        let previousVisibleSibling = activeLi.previousElementSibling;
        while (previousVisibleSibling.classList.contains('d-none'))
            previousVisibleSibling = previousVisibleSibling.previousElementSibling;

        activeLi.classList.remove('active');
        previousVisibleSibling.classList.add('active');

        return;
    }

    //every other case
    //page two is active and the previous page is one
    if (firstPageLiValue === 1 && firstPageLi.nextElementSibling.classList.contains('active')) {
        firstPageLi.classList.add('active');
        firstPageLi.nextElementSibling.classList.remove('active');
        return;
    }

    //we are at the last 5, but we are not in position 2(the active)
    if (firstPageLiValue + 4 == lastPageLiValue && !firstPageLi.nextElementSibling.classList.contains('active')) {
        let nextLiSibling = firstPageLi.nextElementSibling; //we start from second sibling
        while (true) {
            if (nextLiSibling.nextElementSibling.classList.contains('active')) {

                nextLiSibling.nextElementSibling.classList.remove('active');
                //if the last page is the active one edge case
                if (nextLiSibling.classList.contains('d-none')) {
                    nextLiSibling.previousElementSibling.classList.add('active');
                }
                else {
                    nextLiSibling.classList.add('active');
                }
                break;
            }

            nextLiSibling = nextLiSibling.nextElementSibling;
        }

        return;
    }

    //in the case we are second position, but ... is hidden(we are at looking at the last 5) add the ...
    if (firstPageLiValue + 4 == lastPageLiValue && firstPageLi.nextElementSibling.classList.contains('active')) {
        lastPageLi.previousElementSibling.classList.remove('d-none');
    }

    //every other case reduce all, but the last one by one
    document.querySelectorAll('.pagination .page-item').forEach(li => {
        if (li.previousElementSibling && li.previousElementSibling.classList.contains('disabled')) {
            return;
        }

        const currentLiAElement = li.querySelector('a');
        if (currentLiAElement) {
            const currentLiAElementValue = currentLiAElement.textContent.trim();

            if (!isNaN(currentLiAElementValue)) {
                currentLiAElement.textContent = currentLiAElementValue - 1;
            }
        }
    });

}

function handleGoToNextLi() {
    let firstPageLi = goToPreviousPaginationLi.nextElementSibling;
    let lastPageLi = goToNextPaginationLi.previousElementSibling;
    let firstPageLiValue = parseInt(firstPageLi.querySelector('a').textContent.trim());
    let lastPageLiValue = parseInt(lastPageLi.querySelector('a').textContent.trim());
    //we are at the last page
    if (lastPageLi.classList.contains('active')) {
        return;
    }

    //if we have 5 or less pages
    if (totalPages <= 5) {
        let activeLi = document.querySelector('.pagination .page-item.active');
        if (activeLi.nextElementSibling.classList.contains('d-none')) {
            activeLi.classList.remove('active');
            lastPageLi.classList.add('active');
        }
        else {
            activeLi.classList.remove('active');
            activeLi.nextElementSibling.classList.add('active');
        }
        return;
    }

    //every other case
    //page 1 is the one with the focus
    if (firstPageLiValue === 1 && firstPageLi.classList.contains('active')) {
        firstPageLi.classList.remove('active');
        firstPageLi.nextElementSibling.classList.add('active');
        return;
    }

    //we are at the last 4
    if (firstPageLiValue + 4 == lastPageLiValue) {
        let nextLiSibling = firstPageLi.nextElementSibling; //we start from second sibling
        while (true) {
            if (nextLiSibling.classList.contains('active')) {
                nextLiSibling.classList.remove('active');
                //edge case if we are going to the last element
                if (nextLiSibling.nextElementSibling.classList.contains('d-none')) {
                    lastPageLi.classList.add('active');
                }
                else {
                    nextLiSibling.nextElementSibling.classList.add('active');
                }
                break;
            }

            nextLiSibling = nextLiSibling.nextElementSibling;
        }

        return;
    }

    //We are getting to the last 5 which means that we need to add the d-none to the ... li
    if (firstPageLiValue + 5 == lastPageLiValue && firstPageLi.nextElementSibling.classList.contains('active')) {
        lastPageLi.previousElementSibling.classList.add('d-none');
    }

    //every other case add one to all, but the last one by one
    document.querySelectorAll('.pagination .page-item').forEach(li => {
        if (li.previousElementSibling && li.previousElementSibling.classList.contains('disabled')) {
            return;
        }

        const currentLiAElement = li.querySelector('a');
        if (currentLiAElement) {
            const currentLiAElementValue = currentLiAElement.textContent.trim();

            if (!isNaN(currentLiAElementValue)) {
                currentLiAElement.textContent = parseInt(currentLiAElementValue) + 1;
            }
        }
    });
}

function handleClickOnPaginationLi(clickedLi) {
    let clickedLiValue = parseInt(clickedLi.querySelector('a').textContent.trim());
    let firstPageLi = goToPreviousPaginationLi.nextElementSibling;
    let lastPageLi = goToNextPaginationLi.previousElementSibling;
    let firstPageLiValue = parseInt(firstPageLi.querySelector('a').textContent.trim());
    let lastPageLiValue = parseInt(lastPageLi.querySelector('a').textContent.trim());

    if (clickedLi.classList.contains('active')) {
        return;
    }
    else if (clickedLiValue == lastPageLiValue) {
        handleGoToLastLi();
        return;
    }
    else if (clickedLiValue + 4 > lastPageLiValue) {
        if (!lastPageLi.previousElementSibling.classList.contains('d-none')) {
            let counter = lastPageLiValue - 4;
            document.querySelectorAll('.pagination .page-item').forEach(li => {
                li.classList.remove('active');

                if (li.previousElementSibling && li.previousElementSibling.classList.contains('disabled')) {
                    return;
                }

                const currentLiAElement = li.querySelector('a');
                if (currentLiAElement) {
                    const currentLiAElementValue = currentLiAElement.textContent.trim();

                    if (!isNaN(currentLiAElementValue)) {
                        currentLiAElement.textContent = counter;

                        if (counter === clickedLiValue) {
                            li.classList.add('active');
                        }

                        counter++;
                    }
                }
            });

            lastPageLi.previousElementSibling.classList.add('d-none');
            return;
        }

        document.querySelectorAll('.pagination .page-item.page-li').forEach(li => li.classList.remove('active'));
        clickedLi.classList.add('active');
        return;
    }
    else {
        //this edge case can only happen if we are the beggining of the pagination or if there are less than 5-6 pages
        if (clickedLiValue == 1) {
            document.querySelectorAll('.pagination .page-item').forEach(item => item.classList.remove('active'));
            firstPageLi.classList.add('active');
            return;
        }

        let counter = -1;
        document.querySelectorAll('.pagination .page-item').forEach(li => {
            li.classList.remove('active');

            if (li.previousElementSibling && li.previousElementSibling.classList.contains('disabled')) {
                return;
            }

            const currentLiAElement = li.querySelector('a');
            if (currentLiAElement) {
                const currentLiAElementValue = currentLiAElement.textContent.trim();

                if (!isNaN(currentLiAElementValue)) {
                    currentLiAElement.textContent = clickedLiValue + counter;
                    counter++;
                }
            }
        });

        firstPageLi.nextElementSibling.classList.add('active');
        if (clickedLiValue + 4 <= lastPageLiValue && lastPageLiValue > 5) {
            lastPageLi.previousElementSibling.classList.remove('d-none');
        }
    }
}

//call this after filters are applied
function updateTotalItems(newTotalItems) {
    totalItems = parseInt(newTotalItems);
    totalPages = Math.ceil(totalItems / itemsPerPage);

    commonUpdatePaginationSectionProcedures();
    //here because this was called from filters logic the filter needs to do on its own the items update logic
}

function updateItemsPerPage(newItemsPerPage) {
    itemsPerPage = parseInt(newItemsPerPage);
    totalPages = Math.ceil(totalItems / itemsPerPage);

    commonUpdatePaginationSectionProcedures();
    document.dispatchEvent(new CustomEvent('itemsPerPage:changed'));
}

function commonUpdatePaginationSectionProcedures() {
    let firstLi = goToPreviousPaginationLi.nextElementSibling;
    let lastLi = goToNextPaginationLi.previousElementSibling;

    //if there is only one page then hide this section
    if (totalPages <= 1) {
        paginationUl.classList.add('d-none');
        return;
    }

    let currentLi = firstLi;
    for (let i = 1; i < 7; i++) {
        //the i!=6 prevents the last page from being hidden. It must never be hidden
        if (i == 6 || i + 1 <= totalPages) {
            currentLi.classList.remove('d-none');
        }
        else {
            currentLi.classList.add('d-none')
        }
        currentLi.classList.remove('active');
        currentLi = currentLi.nextElementSibling;
    }

    paginationUl.classList.remove('d-none');
    firstLi.classList.add('active');
    lastLi.querySelector('a').textContent = totalPages;
}
