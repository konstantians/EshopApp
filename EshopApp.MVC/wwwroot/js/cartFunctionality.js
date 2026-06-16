async function initiateCart() {
    //check if the user is in the checkout
    const currentPage = window.location.pathname.toLowerCase();
    if(currentPage.includes("viewcart") || currentPage.includes("customeraccounttypeselection") || currentPage.includes("customerinformation") || currentPage.includes("orderinformation") || currentPage.includes("orderfinalization")){
        document.getElementById("cartSection").classList.add('d-none');
        document.getElementById("cartIconContainerSmall").classList.add('d-none');
        document.getElementById("userIconContainerSmall").classList.replace('me-1', 'me-4');
    };

    //continue if not
    if (!localStorage.getItem('cart')) {
        try {
            const response = await fetch('/cart/initializeUserCart');

            if (!response.ok) {
                // probably send the user to the custom 500 page
                throw new Error(`HTTP error: ${response.status}`);
            }

            const data = await response.json();
            if (data.authenticated === false && data.hadAccessToken) {
                window.location.href = '/account/signInAndSignUp';
                return;
            }
            else if(data.authenticated === false){
                document.getElementById("cartIconContainer").classList.remove('dropdown-hoverable');
                document.getElementById("cartIconContainerSmall").classList.remove('dropdown-hoverable');
                return;
            }

            // Save cart if present
            if (data.cart) {
                localStorage.setItem('cart', JSON.stringify(data.cart));
                renderUserCart(data.cart);
            }
        }
        catch (err) {
            console.error('Cart initialization failed:', err);
        }
    }
    else {
        let cart = localStorage.getItem('cart');
        cart = JSON.parse(cart);
        if (cart.cartItems.length === 0) {
            document.getElementById("cartIconContainer").classList.remove('dropdown-hoverable');
            document.getElementById("cartIconContainerSmall").classList.remove('dropdown-hoverable');
        }
        else{
            renderUserCart(cart);
        }
    }
}
function renderUserCart(cart){
    const dynamicImagesUrl = window.appConfig.dynamicImagesUrl; //'@Url.Content("~/DynamicImages/")'
    let cartItemsContainerSmall = document.getElementById("cartItemsContainerSmall");
    let cartItemsContainer = document.getElementById("cartItemsContainer");

    let cartBadgeSmall = document.getElementById("cartBadgeSmall");
    cartBadgeSmall.textContent = cart.cartItems.length;
    let cartBadge = document.getElementById("cartBadge");
    cartBadge.textContent = cart.cartItems.length;
    if(cart.cartItems.length === 0){
        document.getElementById("cartIconContainer").classList.remove('dropdown-hoverable');
        document.getElementById("cartIconContainerSmall").classList.remove('dropdown-hoverable');
    }
    //if we have 0 items and an item is added without the else the container will never become hoverable
    else{
        document.getElementById("cartIconContainer").classList.add('dropdown-hoverable');
        document.getElementById("cartIconContainerSmall").classList.add('dropdown-hoverable');
    }
            
    let totalCartPrice = 0;
    cart.cartItems.forEach(item => {
        const variant = item.variant;
        if (!variant) return;

        // Choose thumbnail image if exists
        let imgUrl = window.appConfig.noProductImageUrl;
        const images = variant.variantImages ?? [];
        const thumbnail = images.find(img => img?.isThumbNail === true);
        const fallback = images[0];
        const selectedImage = thumbnail || fallback;

        if (selectedImage?.image?.imagePath) {
            imgUrl = dynamicImagesUrl + selectedImage.image.imagePath;
        }

        const row = document.createElement('div');
        row.className = 'row mb-2';

        // Image column
        const colImg = document.createElement('div');
        colImg.className = 'col-4';
        const aImg = document.createElement('a');
        aImg.href = '/Home/ViewItem/' + variant.id;
        const img = document.createElement('img');
        img.src = imgUrl;
        img.className = 'img-fluid';
        img.style.objectFit = 'fill';
        img.style.width = '100%';
        img.style.height = '60px';
        img.style.border = '1px solid #ccc';
        aImg.appendChild(img);
        colImg.appendChild(aImg);

        // Details column
        const colDetails = document.createElement('div');
        colDetails.className = 'col-8 d-flex flex-column justify-content-between';

        const aName = document.createElement('a');
        aName.href = '#';
        aName.className = 'text-secondary text-decoration-none hover-black';
        aName.textContent = `${item.quantity} x ${variant.product?.name || 'Unknown product'}`;
        cartBadge.textContent = Number(cartBadge.textContent) + item.quantity - 1;
        cartBadgeSmall.textContent = Number(cartBadgeSmall.textContent) + item.quantity - 1;

        const priceRow = document.createElement('div');
        priceRow.className = 'd-flex justify-content-between align-items-center';

        let realPrice = Number(variant.price);
        if (variant.discount) {
            realPrice = variant.price - (variant.price * variant.discount.percentage / 100);
        }

        const priceDiv = document.createElement('div');
        priceDiv.className = 'text-info text-smaller';
        priceDiv.innerHTML = realPrice.toFixed(2) + " €";
        totalCartPrice += item.quantity * Number(realPrice.toFixed(2));

        const removeLink = document.createElement('a');
        removeLink.href = '#';
        const removeIcon = document.createElement('i');
        removeIcon.className = 'fa-regular fa-x text-secondary hover-black';
        removeIcon.addEventListener('click', (e) => {
            e.preventDefault();
            removeFromCart(item.id); 
        });

        removeLink.appendChild(removeIcon);
        priceRow.appendChild(priceDiv);
        priceRow.appendChild(removeLink);

        colDetails.appendChild(aName);
        colDetails.appendChild(priceRow);

        row.appendChild(colImg);
        row.appendChild(colDetails);

        cartItemsContainer.appendChild(row);
        cartItemsContainerSmall.appendChild(row.cloneNode(true));
    });

    document.getElementById("finalPriceLink").textContent = totalCartPrice.toFixed(2) + " €";
}

async function addItemToCart(variantId, sku, quantity, productId, productName, price, imagePath, shouldAnimate = false) {
    try {
        const response = await fetch('/cart/addItemToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                variantId: variantId,
                quantity: quantity
            })
        });

        if (!response.ok) {
            const data = await response.json();
            if (data.errorMessage) {
                resultModal.setAttribute("popUpValue", "insufficientVariantQuantity");
                showPopUpModal('resultModal', "Αποτυχία Αλλαγής Ποσότητας", "Η ποσότητα παραλλαγής προϊόντος που θέλετε να προσθέσετε στο καλάθι σας δεν είναι διαθέσιμη.",
                    "Αποτυχία", "Η ποσότητα παραλλαγής προϊόντος που θέλετε να προσθέσετε στο καλάθι σας δεν είναι διαθέσιμη.");
            }
            return false;
        }

        const data = await response.json();
        if (data.authenticated === false) {
            addToCartLocal(variantId, sku, quantity, productId, productName, price, imagePath);
            if (shouldAnimate) {
                animateToCart();
            }
            return true;
        }

        if (shouldAnimate) {
            animateToCart();
        }

        localStorage.removeItem('cart');
        document.getElementById("cartItemsContainer").innerHTML = "";
        document.getElementById("cartItemsContainerSmall").innerHTML = "";

        initiateCart();
        return true;
    } catch (err) {
        console.error('Request failed:', err);
        return false;
    }
}

function addToCartLocal(variantId, sku, quantity, productId, productName, price, imagePath) {
    let cart = JSON.parse(localStorage.getItem("cart")) || { cartItems: [] };

    // Check if this variant is already in the cart
    const existingItem = cart.cartItems.find(item => item.variant.id === variantId);

    if (existingItem) {
        existingItem.quantity += Number(quantity);
    }
    //This part will only work in a specific page
    else {
        // Add new item
        cart.cartItems.push({
            variant: {
                product: {
                    id: productId,
                    name: productName
                },
                id: variantId,
                sku: sku,
                price: Number(price),
                variantImages: [
                    {
                        isThumbNail: true,
                        image: {
                            imagePath: imagePath
                        }
                    }
                ]
            },
            quantity: Number(quantity),
            id: crypto.randomUUID(),
        });
    }

    // Save back to localStorage
    localStorage.setItem("cart", JSON.stringify(cart));
    document.getElementById("cartItemsContainer").innerHTML = "";
    document.getElementById("cartItemsContainerSmall").innerHTML = "";

    // Render cart
    renderUserCart(cart);
}

async function updateCartItem(cartItemId, quantity) {
    try {
        const response = await fetch('/cart/updateCartItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                cartItemId: cartItemId,
                quantity: quantity
            })
        });

        if (!response.ok) {
            const data = await response.json();
            if (data.errorMessage === "InsufficientStockForVariant") {
                resultModal.setAttribute("popUpValue", "insufficientVariantQuantity");
                showPopUpModal('resultModal', "Αποτυχία Αλλαγής Ποσότητας", "Η ποσότητα παραλλαγής προϊόντος που θέλετε να προσθέσετε στο καλάθι σας δεν είναι διαθέσιμη.",
                    "Αποτυχία", "Η ποσότητα παραλλαγής προϊόντος που θέλετε να προσθέσετε στο καλάθι σας δεν είναι διαθέσιμη.");
            }
            return false;
        }

        const data = await response.json();
        if (data.authenticated === false) {
            updateCartItemLocal(cartItemId, quantity);
            return true;
        }

        localStorage.removeItem('cart');
        document.getElementById("cartItemsContainer").innerHTML = "";
        document.getElementById("cartItemsContainerSmall").innerHTML = "";

        initiateCart();
        return true;

    } catch (err) {
        console.error('Request failed:', err);
    }
}

function updateCartItemLocal(cartItemId, quantity) {
    //TODO here probably we should do a get variant instead of this crap... To also check for quantity.
    let cart = JSON.parse(localStorage.getItem("cart")) || { cartItems: [] };

    // Check if this variant is already in the cart
    const existingItem = cart.cartItems.find(item => item.id === cartItemId);
    existingItem.quantity = Number(quantity);

    // Save back to localStorage
    localStorage.setItem("cart", JSON.stringify(cart));
    document.getElementById("cartItemsContainer").innerHTML = "";
    document.getElementById("cartItemsContainerSmall").innerHTML = "";

    // Render cart
    renderUserCart(cart);
}

async function removeFromCart(cartItemId) {
    if (!cartItemId) return false;

    try {
        const response = await fetch('/cart/removeItemFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(cartItemId)
        });

        //200 means nothing happened because the user was not authenticated 
        //204 means the user is authenticated and the item was removed correctly
        if (response.status === 200 || response.status === 204) {
            let cart = localStorage.getItem("cart");

            if (!cart) {
                return true;
            }

            cart = JSON.parse(cart);

            if (cart.cartItems) {
                cart.cartItems = cart.cartItems.filter(item => item.id !== cartItemId);
            }

            if (!cart.cartItems || cart.cartItems.length === 0) {
                localStorage.removeItem("cart");
                document.getElementById("cartItemsContainer").innerHTML = "";
                document.getElementById("cartItemsContainerSmall").innerHTML = "";
                document.getElementById("cartIconContainer").classList.remove('dropdown-hoverable');
                document.getElementById("cartIconContainerSmall").classList.remove('dropdown-hoverable');
                document.getElementById("cartBadgeSmall").textContent = "0";
                document.getElementById("cartBadge").textContent = "0";
                document.getElementById("finalPriceLink").textContent = "0.00 €";
            }
            else {
                localStorage.setItem("cart", JSON.stringify(cart));
            }

            document.getElementById("cartItemsContainer").innerHTML = "";
            document.getElementById("cartItemsContainerSmall").innerHTML = "";

            initiateCart();

            return true;
        }

        const data = await response.json();
        console(data?.errorMessage || "Failed to remove item from cart");

        return false;
    }
    catch (err) {
        console.error('Request failed:', err);
        alert("Could not remove item from cart. Try again later.");
        return false;
    }
}