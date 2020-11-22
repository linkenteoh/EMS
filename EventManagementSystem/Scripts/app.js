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

// TODO: Initiate POST request to url provided
$('[data-post]').click(e => {
    e.preventDefault();
    let url = $(e.target).data('post');
    // let f = document.createElement('form'); // js
    let f = $('<form>')[0];// jquery
    f.method = 'post';
    f.action = url || location;
    $(document.body).append(f);
    f.submit();
    /*<form method action></form>*/

});

// TODO: Check all
$('[data-check]').click(e => {
    e.preventDefault();
    let name = $(e.target).data("check"); //ids
    $(`[name=${name}]`).prop('checked', true);
    // <input type="checkbox" checked="checked" />
});

// TODO: Uncheck all
$('[data-uncheck]').click(e => {
    e.preventDefault();
    let name = $(e.target).data("uncheck"); //ids
    $(`[name=${name}]`).prop('checked', false);
    // <input type="checkbox" checked="checked" />
});
$('[data-checkable]').click(e => {
    if ($(e.target).is('input, button, a')) return;
    let cb = $(e.currentTarget).find(':checkbox')[0]
    if (cb != null) {
        cb.checked = !cb.checked;
    }

});