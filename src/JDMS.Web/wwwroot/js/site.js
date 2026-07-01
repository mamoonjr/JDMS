$(function () {
    if ($.fn.DataTable && $('.jdms-table').length) {
        $('.jdms-table').DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/ar.json',
                emptyTable: 'لا توجد بيانات للعرض'
            },
            pageLength: 25,
            order: []
        });
    }

    $('#themeToggle').on('click', function () {
        const html = document.documentElement;
        const isDark = html.getAttribute('data-bs-theme') === 'dark';
        const next = isDark ? 'light' : 'dark';
        html.setAttribute('data-bs-theme', next);
        document.cookie = 'jdms-theme=' + next + ';path=/;max-age=31536000';
        $(this).text(next === 'dark' ? 'الوضع الفاتح' : 'الوضع الداكن');
    });

    const theme = document.documentElement.getAttribute('data-bs-theme');
    if (theme === 'dark') $('#themeToggle').text('الوضع الفاتح');
});

function loadAreas(governorateId, areaSelect, selectedId) {
    if (!governorateId) return;
    $.get('/Governorates/GetAreas', { governorateId: governorateId }, function (data) {
        areaSelect.empty().append('<option value="">-- اختر المنطقة --</option>');
        data.forEach(function (a) {
            areaSelect.append($('<option>', { value: a.id, text: a.nameAr }));
        });
        if (selectedId) areaSelect.val(selectedId);
    });
}
