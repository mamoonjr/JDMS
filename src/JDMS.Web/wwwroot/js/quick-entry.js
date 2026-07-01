(function () {

    const cfg = window.quickEntryConfig || {};

    let lineIndex = 1;

    let lookupTimer = null;



    function getDeliveryFeeFromInput() {

        return Math.max(0, parseFloat($('#deliveryFeeInput')?.value) || 0);

    }



    const $ = (sel, ctx) => (ctx || document).querySelector(sel);

    const $$ = (sel, ctx) => [...(ctx || document).querySelectorAll(sel)];



    function formatMoney(n) {

        return (Number(n) || 0).toFixed(2);

    }



    function getToken() {

        return $('input[name="__RequestVerificationToken"]')?.value || '';

    }



    function recalc() {

        let subtotal = 0;

        $$('#linesBody .line-row').forEach(row => {

            const qty = parseFloat($('.qty-input', row)?.value) || 0;

            const price = parseFloat($('.price-input', row)?.value) || 0;

            const lineTotal = qty * price;

            const cell = $('.line-total', row);

            if (cell) cell.textContent = formatMoney(lineTotal);

            subtotal += lineTotal;

        });



        const deliveryFee = getDeliveryFeeFromInput();

        const discount = parseFloat($('#discount')?.value) || 0;

        const grand = subtotal + deliveryFee - discount;



        $('#subtotalDisplay').textContent = formatMoney(subtotal);

        $('#deliverySummary').textContent = formatMoney(deliveryFee);

        $('#discountDisplay').textContent = formatMoney(discount);

        $('#grandTotalDisplay').textContent = formatMoney(Math.max(0, grand)) + ' د.أ';

    }



    function reindexLines() {

        $$('#linesBody .line-row').forEach((row, i) => {

            $('.product-select', row)?.setAttribute('name', `Lines[${i}].ProductId`);

            $('.qty-input', row)?.setAttribute('name', `Lines[${i}].Quantity`);

            $('.price-input', row)?.setAttribute('name', `Lines[${i}].UnitPrice`);

        });

        lineIndex = $$('#linesBody .line-row').length;

    }



    function bindLineEvents(row) {

        $('.product-select', row)?.addEventListener('change', function () {

            const price = this.selectedOptions[0]?.dataset?.price || 0;

            $('.price-input', row).value = price;

            recalc();

        });

        $('.qty-input', row)?.addEventListener('input', recalc);

        $('.price-input', row)?.addEventListener('input', recalc);

        $('.remove-line', row)?.addEventListener('click', function () {

            if ($$('#linesBody .line-row').length > 1) {

                row.remove();

                reindexLines();

                recalc();

            }

        });

    }



    async function loadAreas(governorateId, selectedAreaId) {

        const areaSelect = $('#areaId');

        areaSelect.innerHTML = '<option value="">-- جاري التحميل --</option>';

        areaSelect.disabled = true;

        if (!governorateId) {

            areaSelect.innerHTML = '<option value="">-- اختر المحافظة --</option>';

            return;

        }

        const res = await fetch(`${cfg.areasUrl}?governorateId=${governorateId}`);

        const areas = await res.json();

        areaSelect.innerHTML = '<option value="">-- اختر المنطقة --</option>';

        areas.forEach(a => {

            const name = a.nameAr ?? a.NameAr ?? '';

            const opt = document.createElement('option');

            opt.value = a.id ?? a.Id;

            opt.textContent = name;

            areaSelect.appendChild(opt);

        });

        areaSelect.disabled = false;

        if (selectedAreaId) {

            areaSelect.value = String(selectedAreaId);

        }

    }



    async function lookupCustomer(phone) {

        const status = $('#phoneStatus');

        if (!phone || phone.length < 7) {

            status.textContent = '';

            status.className = 'text-muted';

            return;

        }

        status.textContent = 'جاري البحث...';

        status.className = 'text-info';

        try {

            const res = await fetch(`${cfg.lookupUrl}?phone=${encodeURIComponent(phone)}`);

            const data = await res.json();

            if (data.found) {

                $('#existingCustomerId').value = data.customerId;

                $('#fullName').value = data.fullName || '';

                $('#secondaryPhone').value = data.secondaryPhone || '';

                if (data.governorateId) {

                    $('#governorateId').value = data.governorateId;

                    await loadAreas(data.governorateId, data.areaId);

                }

                $('#neighborhood').value = data.neighborhood || '';

                $('#buildingNumber').value = data.buildingNumber || '';

                $('#street').value = data.street || '';

                status.textContent = '✓ عميل موجود - تم تحميل البيانات';

                status.className = 'text-success';

            } else {

                $('#existingCustomerId').value = '';

                status.textContent = 'عميل جديد';

                status.className = 'text-warning';

            }

        } catch {

            status.textContent = 'تعذر البحث';

            status.className = 'text-danger';

        }

    }



    function resetForm() {

        $('#quickEntryForm').reset();

        $('#existingCustomerId').value = '';

        $('#phoneStatus').textContent = '';

        $('#areaId').innerHTML = '<option value="">-- اختر المحافظة --</option>';

        $('#areaId').disabled = true;

        const tbody = $('#linesBody');

        tbody.innerHTML = tbody.querySelector('.line-row').outerHTML;

        lineIndex = 1;

        bindLineEvents($('.line-row'));

        $('#submitAlert').classList.add('d-none');

        recalc();

    }



    function collectFormData() {

        const lines = [];

        $$('#linesBody .line-row').forEach(row => {

            const productId = parseInt($('.product-select', row)?.value) || 0;

            const quantity = parseInt($('.qty-input', row)?.value) || 0;

            const unitPrice = parseFloat($('.price-input', row)?.value) || 0;

            if (productId > 0 && quantity > 0) {

                lines.push({ productId, quantity, unitPrice });

            }

        });

        return {

            existingCustomerId: $('#existingCustomerId').value ? parseInt($('#existingCustomerId').value) : null,

            fullName: $('#fullName').value.trim(),

            phoneNumber: $('#phoneNumber').value.trim(),

            secondaryPhone: $('#secondaryPhone').value.trim() || null,

            governorateId: parseInt($('#governorateId').value) || 0,

            areaId: parseInt($('#areaId').value) || 0,

            neighborhood: $('#neighborhood').value.trim(),

            buildingNumber: $('#buildingNumber').value.trim(),

            street: $('#street').value.trim() || null,

            deliveryFee: getDeliveryFeeFromInput(),

            discount: parseFloat($('#discount').value) || 0,

            notes: ($('#orderNotes')?.value || '').trim().slice(0, 500) || null,

            lines

        };

    }



    document.addEventListener('DOMContentLoaded', function () {

        $$('#linesBody .line-row').forEach(bindLineEvents);



        $('#phoneNumber')?.addEventListener('input', function () {

            clearTimeout(lookupTimer);

            lookupTimer = setTimeout(() => lookupCustomer(this.value.trim()), 500);

        });

        $('#phoneNumber')?.addEventListener('blur', function () {

            lookupCustomer(this.value.trim());

        });



        $('#governorateId')?.addEventListener('change', function () {

            loadAreas(this.value);

        });



        $('#deliveryFeeInput')?.addEventListener('input', recalc);



        $('#discount')?.addEventListener('input', recalc);



        $('#addLineBtn')?.addEventListener('click', function () {

            const first = $('#linesBody .line-row');

            const clone = first.cloneNode(true);

            clone.querySelector('.product-select').value = '';

            clone.querySelector('.qty-input').value = '1';

            clone.querySelector('.price-input').value = '';

            clone.querySelector('.line-total').textContent = '0.00';

            $('#linesBody').appendChild(clone);

            reindexLines();

            bindLineEvents(clone);

        });



        $('#resetBtn')?.addEventListener('click', resetForm);



        $('#quickEntryForm')?.addEventListener('submit', async function (e) {

            e.preventDefault();

            const btn = $('#saveBtn');

            const spinner = btn.querySelector('.spinner-border');

            const alert = $('#submitAlert');

            btn.disabled = true;

            spinner?.classList.remove('d-none');



            try {

                const body = collectFormData();

                const res = await fetch(cfg.submitUrl, {

                    method: 'POST',

                    headers: { 'Content-Type': 'application/json' },

                    body: JSON.stringify(body)

                });

                const result = await res.json();

                alert.classList.remove('d-none', 'alert-success', 'alert-danger');

                if (result.success) {

                    alert.classList.add('alert-success');

                    alert.innerHTML = `${result.message} - رقم الطلب: <strong>${result.orderNumber}</strong>

                        <a href="${cfg.printUrl}?orderId=${result.orderId}" target="_blank" class="alert-link ms-2">طباعة الفاتورة</a>

                        <button type="button" class="btn btn-sm btn-outline-success ms-2" id="newOrderBtn">طلب جديد</button>`;

                    $('#newOrderBtn')?.addEventListener('click', resetForm);

                } else {

                    alert.classList.add('alert-danger');

                    alert.textContent = result.message || 'فشل الحفظ';

                }

            } catch (err) {

                alert.classList.remove('d-none');

                alert.classList.add('alert-danger');

                alert.textContent = 'حدث خطأ في الاتصال';

            } finally {

                btn.disabled = false;

                spinner?.classList.add('d-none');

            }

        });



        $('#quickEntryForm')?.addEventListener('keydown', function (e) {

            if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA' && e.target.type !== 'submit') {

                const focusable = [...this.querySelectorAll('input,select,button:not([type=submit])')]

                    .filter(el => !el.disabled && el.offsetParent !== null);

                const idx = focusable.indexOf(e.target);

                if (idx >= 0 && idx < focusable.length - 1) {

                    e.preventDefault();

                    focusable[idx + 1].focus();

                }

            }

        });



        recalc();

    });

})();


