function setUpSortingDropdownListeners(items) {
    const handlers = [];
    const sortingDropdown = document.getElementById('sortingDropdown');
    if (!sortingDropdown) return handlers;

    sortingDropdown.querySelectorAll('li').forEach(li => {
        const handler = function () {
            let directionOrder = li.getAttribute('data-order-direction') || "ascending";
            let isNumeric = li.hasAttribute('data-numeric');
            sortByOrderAttribute(items, li.getAttribute('data-order'), directionOrder, isNumeric);
            renderTable(items);
        };
        li.addEventListener('click', handler);
        handlers.push({ li, handler });
    });

    return handlers;
}
function removeSortingDropdownListeners(handlers) {
    handlers.forEach(({ li, handler }) => li.removeEventListener('click', handler));
}

function sortByOrderAttribute(items, attribute, direction = 'ascending', isNumeric = false) {
    if (attribute.endsWith("Order")) {
        attribute = attribute.slice(0, -5); //removes the Order
    }

    if (attribute === 'default') {
        items.sort((itemA, itemB) => itemA.originalIndex - itemB.originalIndex);
        return;
    }

    const directionMultiplier = direction === 'ascending' ? 1 : -1;

    if (isNumeric) {
        items.sort((itemA, itemB) =>directionMultiplier * ((Number(itemA[attribute]) || 0) - (Number(itemB[attribute]) || 0)));
        return;
    }

    items.sort((a, b) =>
        directionMultiplier * ((a[attribute] || '').toString().localeCompare((b[attribute] || '').toString()))
    );
}

///////////////////////////////

function setUpCheckBoxFilterUl(ulId, ifCheckedLogicFunction, protectedByArray, isFilterActiveCounter, items, itemCountFunction) {
    document.getElementById(ulId).querySelectorAll('li').forEach(li => {
        li.addEventListener("click", function () {
            let filterCheckBox = li.querySelector('input');
            let liFilter = li.getAttribute('data-filter').toLowerCase().trim();
            if (filterCheckBox.checked) {
                if (typeof ifCheckedLogicFunction === 'function') {
                    ifCheckedLogicFunction(liFilter, li);
                }
            }
            else {
                items.forEach(item => {
                    const index = item[protectedByArray].indexOf(liFilter);
                    if (index > -1) {
                        item[protectedByArray].splice(index, 1);
                    }
                });
                isFilterActiveCounter.value--;
            }

            if (typeof itemCountFunction === 'function') {
                itemCountFunction();
            }
            renderTable(items);
        });
    });
}

function updateIntervalCheckBoxLis(firstPartOfTheUlId, ranges, textAfterIntervalValue) {

    let filtersUl = document.getElementById(firstPartOfTheUlId + "FiltersUl");
    if (!filtersUl) {
        return;
    }
    let lis = filtersUl.querySelectorAll("li");

    for (let li of lis) {
        let min = parseFloat(li.getAttribute("data-filter-interval-start"));
        let max = parseFloat(li.getAttribute("data-filter-interval-end"));

        // Find the matching range
        for (let [range, count] of ranges.entries()) {
            let [rMin, rMax] = range;
            if (rMin === min && rMax === max) {
                // Update label text
                let label = li.querySelector("label");
                let input = li.querySelector("input");
                label.textContent = `${rMin}${textAfterIntervalValue || ''} - ${rMax}${textAfterIntervalValue || ''} (${count})`;

                // Enable/disable li depending on count
                if (count > 0) {
                    li.classList.remove("disabled");
                    li.classList.add("hover-highlight");
                }
                else if (count === 0 && input && !input.checked) {
                    li.classList.remove("hover-highlight");
                    li.classList.add("disabled");
                }
            }
        }
    }
}

function updateValueCheckBoxLi(firstPartOfTheUlId, primitiveKeyValuePairs) {
    let filtersUl = document.getElementById(firstPartOfTheUlId + "FiltersUl");
    if (!filtersUl) {
        return;
    }
    let lis = filtersUl.querySelectorAll("li");

    for (let li of lis) {
        let filterKey = li.getAttribute("data-filter");

        // Find the matching range
        for (let [key, count] of primitiveKeyValuePairs) {
            if (filterKey === key) {
                // Update label text
                let label = li.querySelector("label");
                label.textContent = `${key} (${count})`;

                // Enable/disable li depending on count
                if (count > 0) {
                    li.classList.remove("disabled");
                    li.classList.add("hover-highlight");
                } else {
                    li.classList.remove("hover-highlight");
                    li.classList.add("disabled");
                }
            }
        }
    }
}

//////////////////////////////

let searchInput = document.getElementById('searchInput');
let searchButtonIconIndicator = document.getElementById('searchButtonIconIndicator');
let searchMode = document.querySelector('a[data-search-mode]')?.getAttribute("data-search-mode");

function setUpSearchInputMode(items, itemCountFunction) {
    const handlers = [];

    document.querySelectorAll('a[data-search-mode]').forEach(link => {
        const handler = function() {
            if (searchMode === link.getAttribute('data-search-mode')) {
                return;
            }

            searchInput.value = "";
            searchMode = link.getAttribute('data-search-mode');

            // update icon
            Array.from(searchButtonIconIndicator.classList)
                .filter(iconClass => iconClass.startsWith('fa-') && iconClass !== 'fa-solid')
                .forEach(iconClass => searchButtonIconIndicator.classList.remove(iconClass));
            searchButtonIconIndicator.classList.add(link.dataset.searchModeIcon);

            items.forEach(item => item.isFilteredBySearch = false);
            if (typeof itemCountFunction === 'function') {
                itemCountFunction();
            }
            renderTable(items);
        };

        link.addEventListener('click', handler);
        handlers.push({ link, handler });
    });

    return handlers;
}

function removeSearchInputModeListeners(handlers) {
    handlers.forEach(({ link, handler }) => link.removeEventListener('click', handler));
}

function setUpSearchInput(items, itemCountFunction) {
    let searchInput = document.getElementById('searchInput');
    if (!searchInput) return null;

    let searchTimeOut;

    const handler = function () {
        clearTimeout(searchTimeOut);
        searchTimeOut = setTimeout(() => {
            let query = searchInput.value.trim().toLowerCase();
            if (query === "") {
                items.forEach(item => item.isFilteredBySearch = false);
            } else {
                items.forEach(item => {
                    const match = item[searchMode]?.toLowerCase().includes(query);
                    item.isFilteredBySearch = !match;
                });
            }

            if (typeof itemCountFunction === 'function') {
                itemCountFunction();
            }
            renderTable(items);
        }, 500);
    };

    searchInput.addEventListener('input', handler);

    return { element: searchInput, handler };
}

function removeSearchInputListener(listenerObj) {
    if (listenerObj) {
        listenerObj.element.removeEventListener('input', listenerObj.handler);
    }
}

///////////////////////////////

function renderTable(items) {
    if (!tbody || !currentPage || !itemsPerPage || !addTrToTableBody) {
        console.log("not all needed global variables are defined");
        return;
    }

    tbody.innerHTML = '';

    const start = (currentPage - 1) * itemsPerPage;
    let counter = 0;
    let renderedCount = 0;

    for (const item of items) {
        if (renderedCount >= itemsPerPage) {
            break;
        };

        if (item.isFilteredBySearch) {
            continue;
        }

        if (typeof checkIfItemFilteredByCheckboxFilters === 'function' && checkIfItemFilteredByCheckboxFilters(item)) {
            continue;
        }

        if (counter < start) {
            counter++;
            continue;
        }

        if (typeof addTrToTableBody === 'function') {
            addTrToTableBody(item);
        }

        renderedCount++;
        counter++;
    }
}