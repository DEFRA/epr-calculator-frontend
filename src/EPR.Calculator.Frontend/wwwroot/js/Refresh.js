function processAndUploadData(parameterData, url, sucessUrl, errorAction, fileName) {
    const urlWithFileName = `${url}?fileName=${encodeURIComponent(fileName)}`;
    $.ajax({
        url: urlWithFileName,
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
    let url = `/${action}`;
    $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify({ data }),
        contentType: "application/json",
        success: function (response) {
            window.location.href = response.redirectUrl;
        },
        error: function (xhr, status, error) {
            window.location.href = '/StandardError';
        }
    })
}