// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function checkstatus() {
    $.ajax({
        url: 'ProcessData',
        type: 'POST',
        success: function (response) {
            window.location.href = response.newUrl;
        },
        error: function () {
            $window.location.href = response.newUrl;
        }
    })
}
$(function () {
    checkstatus();
});