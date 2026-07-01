(function ($) {
    'use strict';

    const cfg = window.posConfig || {};
    const taxRate = cfg.taxRate ?? 0.16;

    let cart = [];
    let lastOrderId = null;
    let lookupTimer = null;
    let searchTimer = null;

    function money(n) {
        return (Math.round(n * 100) / 100).toFixed(2);
    }

    function showAlert(msg, ok) {
        const $a = $('#posAlert');
        $a.removeClass('d-none alert-success alert-danger')
            .addClass(ok ? 'alert-success' : 'alert-danger')
            .text(msg);
    }

    function hideAlert() {
        $('#posAlert').addClass('d-none');
    }

    function recalc() {
        const subtotal = cart.reduce((s, l) => s + l.unitPrice * l.quantity, 0);
        const delivery = parseFloat($('#deliveryFee').val()) || 0;
        const discount = parseFloat($('#discount').val()) || 0;
        const taxable = Math.max(0, subtotal - discount);
        const tax = Math.round(taxable * taxRate * 100) / 100;
        const grand = subtotal + delivery - discount + tax;

        $('#sumSubtotal').text(money(subtotal));
        $('#sumDelivery').text(money(delivery));
        $('#sumTax').text(money(tax));
        $('#sumGrand').text(money(grand));

        const received = parseFloat($('#amountReceived').val());
        if (!isNaN(received) && received > 0) {
            $('#changeDue').val(money(Math.max(0, received - grand)));
        } else {
            $('#changeDue').val('0.00');
        }
        return { subtotal, delivery, discount, tax, grand };
    }

    function renderCart() {
        const $body = $('#cartBody').empty();
        if (cart.length === 0) {
            $body.append('<tr><td colspan="5" class="text-center text-muted small">السلة فارغة</td></tr>');
        } else {
            cart.forEach((line, idx) => {
                const total = line.unitPrice * line.quantity;
                $body.append(
                    `<tr data-idx="${idx}">
                        <td class="text-truncate" style="max-width:120px" title="${escapeHtml(line.name)}">${escapeHtml(line.name)}</td>
                        <td class="text-center"><input type="number" min="1" class="form-control form-control-sm qty-input" value="${line.quantity}" data-idx="${idx}"/></td>
                        <td>${money(line.unitPrice)}</td>
                        <td>${money(total)}</td>
                        <td><button type="button" class="btn btn-link btn-sm text-danger p-0 btn-remove" data-idx="${idx}">✕</button></td>
                    </tr>`
                );
            });
        }
        recalc();
    }

    function escapeHtml(s) {
        return $('<div>').text(s).html();
    }

    function addToCart(product) {
        const existing = cart.find(c => c.productId === product.id);
        if (existing) {
            existing.quantity += 1;
        } else {
            cart.push({
                productId: product.id,
                name: product.name,
                unitPrice: product.price,
                quantity: 1
            });
        }
        renderCart();
    }

    function productFromCard($btn) {
        return {
            id: parseInt($btn.data('id'), 10),
            name: $btn.data('name'),
            price: parseFloat($btn.data('price')),
            sku: $btn.data('sku'),
            barcode: $btn.data('barcode')
        };
    }

    function renderProductGrid(products) {
        const $grid = $('#productGrid').empty();
        if (!products || products.length === 0) {
            $grid.append('<p class="text-muted small p-2">لا توجد منتجات</p>');
            return;
        }
        products.forEach(p => {
            const img = p.imageUrl || '/images/products/default.svg';
            $grid.append(
                `<button type="button" class="pos-product-card" data-id="${p.id}" data-name="${escapeHtml(p.name)}"
                    data-price="${p.unitPrice}" data-sku="${escapeHtml(p.sku || '')}" data-barcode="${escapeHtml(p.barcode || '')}">
                    <img src="${img}" alt="" loading="lazy" onerror="this.src='/images/products/default.svg'"/>
                    <span class="pos-product-name">${escapeHtml(p.name)}</span>
                    <span class="pos-product-price">${money(p.unitPrice)} د.أ</span>
                </button>`
            );
        });
    }

    function loadAreas(governorateId, selectedAreaId) {
        const $area = $('#areaId').prop('disabled', true).html('<option value="">—</option>');
        if (!governorateId) return;
        $.getJSON(cfg.areasUrl, { governorateId }, function (areas) {
            areas.forEach(a => {
                $area.append(`<option value="${a.id}" data-fee="${a.deliveryFee}">${escapeHtml(a.nameAr)}</option>`);
            });
            $area.prop('disabled', false);
            if (selectedAreaId) {
                $area.val(String(selectedAreaId)).trigger('change');
            }
        });
    }

    function lookupCustomer(phone) {
        const digits = (phone || '').replace(/\D/g, '');
        if (digits.length < 7) return;
        $.getJSON(cfg.lookupUrl, { phone: digits }, function (data) {
            if (data && data.found) {
                $('#existingCustomerId').val(data.customerId);
                $('#fullName').val(data.fullName || '');
                $('#secondaryPhone').val(data.secondaryPhone || '');
                $('#customerLookupHint').removeClass('d-none');
                if (data.governorateId) {
                    $('#governorateId').val(String(data.governorateId));
                    loadAreas(data.governorateId, data.areaId);
                }
                $('#neighborhood').val(data.neighborhood || '');
                $('#buildingNumber').val(data.buildingNumber || '');
                $('#street').val(data.street || '');
            } else {
                $('#existingCustomerId').val('');
                $('#customerLookupHint').addClass('d-none');
            }
        });
    }

    function collectPayload(createInvoice) {
        const totals = recalc();
        return {
            existingCustomerId: parseInt($('#existingCustomerId').val(), 10) || null,
            fullName: $('#fullName').val().trim(),
            phoneNumber: $('#phoneNumber').val().trim(),
            secondaryPhone: $('#secondaryPhone').val().trim() || null,
            governorateId: parseInt($('#governorateId').val(), 10) || 0,
            areaId: parseInt($('#areaId').val(), 10) || 0,
            neighborhood: $('#neighborhood').val().trim(),
            buildingNumber: $('#buildingNumber').val().trim(),
            street: $('#street').val().trim() || null,
            deliveryNotes: $('#deliveryNotes').val().trim() || null,
            deliveryFee: totals.delivery,
            discount: totals.discount,
            assignedDriverId: parseInt($('#assignedDriverId').val(), 10) || null,
            status: parseInt($('#orderStatus').val(), 10),
            paymentMethod: parseInt($('#paymentMethod').val(), 10),
            amountReceived: parseFloat($('#amountReceived').val()) || 0,
            changeDue: parseFloat($('#changeDue').val()) || 0,
            notes: $('#orderNotes').val().trim() || null,
            lines: cart.map(c => ({
                productId: c.productId,
                quantity: c.quantity,
                unitPrice: c.unitPrice
            })),
            createInvoice: !!createInvoice
        };
    }

    function validateOrder() {
        const name = $('#fullName').val().trim();
        const phone = ($('#phoneNumber').val() || '').replace(/\D/g, '');
        if (!name) {
            showAlert('اسم العميل مطلوب', false);
            $('#fullName').focus();
            return false;
        }
        if (phone.length < 7) {
            showAlert('رقم الهاتف مطلوب (7 أرقام على الأقل)', false);
            $('#phoneNumber').focus();
            return false;
        }
        if (cart.length === 0) {
            showAlert('أضف منتجاً واحداً على الأقل', false);
            $('#productSearch').focus();
            return false;
        }
        return true;
    }

    function resetForm() {
        hideAlert();
        lastOrderId = null;
        cart = [];
        renderCart();
        $('#existingCustomerId, #fullName, #phoneNumber, #secondaryPhone, #neighborhood, #buildingNumber, #street, #deliveryNotes, #orderNotes, #amountReceived').val('');
        $('#productSearch').val('');
        $('#deliveryFee, #discount').val('0');
        $('#changeDue').val('0.00');
        $('#governorateId').val('');
        $('#areaId').html('<option value="">—</option>').prop('disabled', true);
        $('#assignedDriverId').val('');
        $('#paymentMethod').val('0');
        $('#orderStatus').val('0');
        $('#customerLookupHint').addClass('d-none');
        $('#phoneNumber').focus();
    }

    function refreshStats() {
        $.getJSON(cfg.statsUrl, function (s) {
            if (!s) return;
            $('#statOrders').text(s.ordersToday);
            $('#statRevenue').text(money(s.revenueToday));
            $('#statPending').text(s.pendingDeliveries);
            $('#statDrivers').text(s.activeDrivers);
        });
    }

    function openPrint(orderId, thermal) {
        const base = thermal ? cfg.printThermalUrl : cfg.printUrl;
        const url = base + (base.indexOf('?') >= 0 ? '&' : '?') + 'orderId=' + orderId;
        window.open(url, '_blank');
    }

    function submit(createInvoice) {
        hideAlert();
        if (!validateOrder()) return;

        const payload = collectPayload(createInvoice);
        $.ajax({
            url: cfg.submitUrl,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    showAlert(res.message + ' — ' + res.orderNumber, true);
                    refreshStats();
                    if (createInvoice && res.orderId) {
                        openPrint(res.orderId, false);
                    }
                    resetForm();
                    $.getJSON(cfg.searchUrl, { q: '' }, renderProductGrid);
                } else {
                    showAlert(res.message || 'فشل الحفظ', false);
                }
            },
            error: function () {
                showAlert('خطأ في الاتصال بالخادم', false);
            }
        });
    }

    // Events
    $(document).on('click', '.pos-product-card', function () {
        addToCart(productFromCard($(this)));
    });

    $('#cartBody').on('click', '.btn-remove', function () {
        cart.splice(parseInt($(this).data('idx'), 10), 1);
        renderCart();
    });

    $('#cartBody').on('change', '.qty-input', function () {
        const idx = parseInt($(this).data('idx'), 10);
        const q = parseInt($(this).val(), 10);
        if (q > 0) cart[idx].quantity = q;
        else cart.splice(idx, 1);
        renderCart();
    });

    $('#phoneNumber').on('input', function () {
        clearTimeout(lookupTimer);
        const v = $(this).val();
        lookupTimer = setTimeout(() => lookupCustomer(v), 400);
    });

    $('#governorateId').on('change', function () {
        loadAreas(parseInt($(this).val(), 10));
    });

    $('#areaId').on('change', function () {
        const fee = $(this).find(':selected').data('fee');
        if (fee !== undefined && fee !== '') {
            $('#deliveryFee').val(money(parseFloat(fee)));
            recalc();
        } else if (cfg.areaFeeUrl) {
            const id = $(this).val();
            if (id) {
                $.getJSON(cfg.areaFeeUrl, { areaId: id }, function (r) {
                    if (r && r.deliveryFee != null) {
                        $('#deliveryFee').val(money(r.deliveryFee));
                        recalc();
                    }
                });
            }
        }
    });

    $('#productSearch').on('input', function () {
        clearTimeout(searchTimer);
        const q = $(this).val();
        searchTimer = setTimeout(function () {
            $.getJSON(cfg.searchUrl, { q }, renderProductGrid);
        }, 300);
    });

    $('#productSearch').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const q = $(this).val().trim();
            $.getJSON(cfg.searchUrl, { q }, function (products) {
                renderProductGrid(products);
                if (products.length === 1) {
                    addToCart({
                        id: products[0].id,
                        name: products[0].name,
                        price: products[0].unitPrice
                    });
                    $(this).val('');
                }
            }.bind(this));
        }
    });

    $('#deliveryFee, #discount, #amountReceived').on('input', recalc);

    $('#btnSave').on('click', () => submit(false));
    $('#btnSavePrint').on('click', () => submit(true));
    $('#btnSaveNew').on('click', () => submit(false));
    $('#btnCancel').on('click', () => {
        if (confirm('إلغاء الطلب الحالي؟')) resetForm();
    });

    $('#btnPrintA4').on('click', function () {
        if (lastOrderId) openPrint(lastOrderId, false);
        else showAlert('احفظ الطلب أولاً', false);
    });
    $('#btnPrintThermal').on('click', function () {
        if (lastOrderId) openPrint(lastOrderId, true);
        else showAlert('احفظ الطلب أولاً', false);
    });

    $(document).on('keydown', function (e) {
        if ($(e.target).is('textarea')) return;
        if (e.key === 'F2') { e.preventDefault(); $('#btnSave').click(); }
        if (e.key === 'F3') { e.preventDefault(); $('#btnSavePrint').click(); }
        if (e.key === 'F4') { e.preventDefault(); $('#btnSaveNew').click(); }
        if (e.key === 'Escape') { e.preventDefault(); $('#btnCancel').click(); }
    });

    renderCart();
    $('#phoneNumber').focus();
})(jQuery);
