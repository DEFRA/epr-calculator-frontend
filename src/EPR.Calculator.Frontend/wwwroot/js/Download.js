function downloadFile(url, fileName, errorAction, event, timeout, token) {
    event.preventDefault();    
    $.ajax({
        url: url,
        method: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        timeout: timeout,
        beforeSend: function (xhr) {
            let bearerToken = "Bearer " + token;
            xhr.setRequestHeader("Authorization", bearerToken);
        },
        success: function (data, status, xhr) {
            try {
                          
                var downloadfileName = getFileNameFromContentDispositionIfEmpty(fileName);                

                // Create a download link
                const url = window.URL.createObjectURL(data);
                const a = document.createElement('a');
                a.href = url;
                a.download = downloadfileName || 'downloaded_file'; // Use custom file name as param or extracted filename or a default name
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
            }
            catch (error)
            {
                window.location.href = errorAction;
            }            
        },
        error: function (error) {
            window.location.href = errorAction;
        }
    });  
}

function isEmpty(value) {
    return (value == null || (typeof value === "string" && value.trim().length === 0));
}

function getFileNameFromContentDispositionIfEmpty(fileName) {

    // if custom param name is empty or null then use extracted filename

    if (fileName == null || (typeof fileName === "string" && fileName.trim().length === 0)) {

        // Get the Content-Disposition header
        const contentDisposition = xhr.getResponseHeader('Content-Disposition');

        // Regular expression to match the filename and filename* parameters
        const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;

        let matches = filenameRegex.exec(contentDisposition);
        if (matches?.[1]) {
            fileName = matches[1].replace(/['"]/g, '');
        }
    }
    
    return fileName;   
}
