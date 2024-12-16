function downloadFile(url, errorAction, event) {
    event.preventDefault();
    var xhr = $.ajax({
        url: url,
        method: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data, status, xhr) {
            const url = window.URL.createObjectURL(data);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = xhr.getResponseHeader('Content-Disposition'); // Replace with your file name and extension
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
        },
        error: function (error) {
            window.location.href = errorAction;
        }        
    });  
}