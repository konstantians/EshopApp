function setUpExternalTable(itemsX, tableId, searchInputId, searchButtonIconIndicatorContainerId, searchButtonIconIndicatorId, hiddenInputId) {
    let tableX = new DataTable("#" + tableId, {
        paging: true,
        pageLength: 10,
        lengthChange: false,
        searching: false,
        info: true,
    });

    hideOrShowPagination(tableX, tableId + "_wrapper");

    let searchInputX = document.getElementById(searchInputId);
    let searchTimeOutX;

    let searchModeX = document.getElementById(searchButtonIconIndicatorContainerId).querySelector('a[data-search-mode-X]')?.getAttribute("data-search-mode-X");
    let handlers = [];

    document.getElementById(searchButtonIconIndicatorContainerId).querySelectorAll('a[data-search-mode-X]').forEach(link => {
        let handler = function () {
            if (searchModeX === link.getAttribute('data-search-mode-X')) {
                return;
            }

            searchInputX.value = "";
            searchModeX = link.getAttribute('data-search-mode-X');

            // update icon
            let searchButtonIconIndicatorX = document.getElementById(searchButtonIconIndicatorId);
            Array.from(searchButtonIconIndicatorX.classList)
                .filter(iconClass => iconClass.startsWith('fa-') && iconClass !== 'fa-solid')
                .forEach(iconClass => searchButtonIconIndicatorX.classList.remove(iconClass));
            searchButtonIconIndicatorX.classList.add(link.dataset.searchModeIconX);

            itemsX.forEach(item => item.isFilteredBySearch = false);
            renderTableX(tableX, itemsX);
            hideOrShowPagination(tableX, tableId + "_wrapper");
        };

        link.addEventListener('click', handler);

        // store the element + handler so you can trigger or remove it later
        handlers.push({ element: link, handler });
    });

    searchInputX.addEventListener('input', function () {
        clearTimeout(searchTimeOutX);
        searchTimeOutX = setTimeout(() => {
            let query = searchInputX.value.trim().toLowerCase();
            if (query === "") {
                itemsX.forEach(item => item.isFilteredBySearch = false);
            } else {
                itemsX.forEach(item => {
                    const match = item[searchModeX]?.toLowerCase().includes(query);
                    item.isFilteredBySearch = !match;
                });
            }

            renderTableX(tableX, itemsX);
            hideOrShowPagination(tableX, tableId + "_wrapper");
        }, 500);
    });

    document.getElementById(tableId).addEventListener('change', (event) => {
        let hiddenInputX = document.getElementById(hiddenInputId);

        if (!event.target.classList.contains('external-table-checkbox')) {
            return;
        }

        const checkboxValue = event.target.value;
        const isChecked = event.target.checked;

        let checkedItemsX = [];
        itemsX.forEach(item => {
            if (item.id === checkboxValue) {
                item.checked = isChecked;
            }

            if (item.checked) {
                checkedItemsX.push(item.id);
            }
        });

        hiddenInputX.value = checkedItemsX.join(',');
    });

    return { table: tableX, handlers: handlers };
}

function hideOrShowPagination(tableX, tableWrapperId) {
    let containers = document.getElementById(tableWrapperId).querySelectorAll(".dt-layout-row");
    let paginationContainer = containers[containers.length - 1];
    let totalRows = tableX.rows().count();
    if (Math.ceil(totalRows / tableX.page.len()) <= 1) {
        paginationContainer.classList.add("d-none");
    }
    else {
        paginationContainer.classList.remove("d-none");
    }
}

function renderTableX(tableX, itemsX, resetTable = false) {
    tableX.clear();

    let counter = 0;
    for (const item of itemsX) {
        if (item.isFilteredBySearch) {
            counter++;
            continue;
        };

        let isChecked = item.checked ? 'checked' : '';
        if (resetTable) {
            isChecked = item.initiallyChecked ? 'checked' : '';
        }

        tableX.row.add(returnTrRow(item, isChecked));

        counter++;
    }

    tableX.draw();
}

function resetExternaltable(tableId, tableX, itemsX, handlers) {
    itemsX.forEach(item => item.isFilteredBySearch = false);
    handlers[0].handler();
    renderTableX(tableX, itemsX, true);
    hideOrShowPagination(tableX, tableId + "_wrapper");
    tableX.page('first').draw('page');
}