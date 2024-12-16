function downloadFile(url, errorAction) {
    $.ajax({
        url: url,
        method: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            const url = window.URL.createObjectURL(data);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = 'filename.csv'; // Replace with your file name and extension
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
        },
        error: function (error) {
            window.location.href = errorAction;
        }
    });    
}