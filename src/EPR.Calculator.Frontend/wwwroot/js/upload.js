function checkstatus(schemeParameterValues) {
    console.log('DATA', schemeParameterValues);
    $.ajax({
        url: '/UploadFileProcessing?schemeParameterValues=' + JSON.stringify(schemeParameterValues),
        type: 'GET',
        // contentType: 'application/json',
        // data: JSON.stringify(schemeParameterValues),
        success: function (response) {
            window.location.href = '/ParameterConfirmation';
        },
        error: function (xhr, status, error) {
            window.location.href = '/UploadCSVError';
        }
    });
}