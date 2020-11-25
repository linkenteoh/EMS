// Escape regular expression
function escapeRegExp(string) {
    return string.replace(/[.*+\-?^${}()|[\]\\]/g, '\\$&');
}

// Initiate GET request to url provided
$('[data-get]').click(e => {
    e.preventDefault();
    let url = $(e.target).data('get');
    location = url || location;
});

// Auto-upper
$('[data-upper]').on('input', e => {
    let a = e.target.selectionStart;
    let b = e.target.selectionEnd;
    e.target.value = e.target.value.toUpperCase();
    e.target.setSelectionRange(a, b);
});

// Reset form
$('[type=reset]').click(e => {
    e.preventDefault();
    location = location;
});

// Initiate POST request to url provided
$('[data-post]').click(e => {
    e.preventDefault();
    let url = $(e.target).data('post');

    let f = $('<form>')[0];
    f.method = 'post';
    f.action = url || location;
    $(document.body).append(f);
    f.submit();
});

// Check all checkboxes
$('[data-check]').click(e => {
    e.preventDefault();
    let name = $(e.target).data('check');
    $(`[name=${name}]`).prop('checked', true);
});

// Uncheck all checkboxes
$('[data-uncheck]').click(e => {
    e.preventDefault();
    let name = $(e.target).data('uncheck');
    $(`[name=${name}]`).prop('checked', false);
});

// Make table row checkable (first checkbox)
$('[data-checkable]').click(e => {
    if ($(e.target).is('input,button,a')) return;

    let cb = $(e.currentTarget).find(':checkbox')[0];
    if (cb) {
        cb.checked = !cb.checked;
    }
});