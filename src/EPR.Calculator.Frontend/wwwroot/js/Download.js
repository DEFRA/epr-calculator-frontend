function downloadFile(url, errorAction, event) {
    event.preventDefault();
    $.ajax({
        url: url,
        method: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data, status, xhr) {
            // Get the Content-Disposition header
            var contentDisposition = xhr.getResponseHeader('Content-Disposition');

            // Regular expression to match the filename and filename* parameters
            const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;

            let filename = '';
            let matches = filenameRegex.exec(contentDisposition);
            if (matches != null && matches[1]) {
                filename = matches[1].replace(/['"]/g, '');
            } 

            // Create a download link
            var url = window.URL.createObjectURL(data);
            var a = document.createElement('a');
            a.href = url;
            a.download = filename || 'downloaded_file'; // Use extracted filename or a default name
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
        },
        error: function (error) {
            window.location.href = errorAction;
        }
    });  
}