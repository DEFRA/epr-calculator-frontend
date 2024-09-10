[ExcludeFromCodeCoverage]
function processAndUploadData(parameterData, url, sucessUrl, errorAction) {
    $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(parameterData),
        processData: false,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            window.location.href = sucessUrl;
        },
        error: function (xhr) {
            callController(errorAction, xhr.responseText);
        }
    });
}

function callController(action, data) {
    var url = "/" + action;
    $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: "application/json",
        success: function () {
            window.location.href = url;
        },
        error: function (xhr, status, error) {
            window.location.href = '/StandardError';
        }
    })
}