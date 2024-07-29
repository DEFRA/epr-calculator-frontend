// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function checkstatus() {
    $.ajax({
        url:'',
        type: 'POST',       
        dataType: 'json',
        success: function (response, xhr) {          
            callController(xhr.statusText, response)
        },
        error: function (response,xhr) {
            callController(xhr.statusText, response)
        }
    })
}
function callController(type, data) {
    var url = type === "success" ? "/ParameterConfirmation" : " /UploadCSVError";
    $.ajax({
            url: url,
        type: 'POST',
        contentType:"application/json",
        data: JSON.stringify(data),
        success: function () {           
                window.location.href = url;
            },
            error: function (xhr, status, error) {
                console.log(status, error);
            }
        })
}

$(function () {
    checkstatus();
});