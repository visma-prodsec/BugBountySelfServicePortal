function showModal(id) {
    var element = $("#" + id);
    element.addClass("in");
    element.css("display", "flex");
}
function showCover() {
    $("#cover").show();
}
function hideCover() {
    $("#cover").hide();
}
function copyToClipboard(text) {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val(text).select();
    document.execCommand("copy");
    $temp.remove();
}