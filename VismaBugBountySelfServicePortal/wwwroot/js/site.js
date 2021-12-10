function showModal(id) {
    var element = window.$("#" + id);
    element.addClass("in");
    element.css("display", "flex");
}
function showCover() {
    window.$("#cover").show();
}
function hideCover() {
    window.$("#cover").hide();
}
function copyToClipboard(text) {
    var $temp = window.$("<input>");
    window.$("body").append($temp);
    $temp.val(text).select();
    document.execCommand("copy");
    $temp.remove();
}