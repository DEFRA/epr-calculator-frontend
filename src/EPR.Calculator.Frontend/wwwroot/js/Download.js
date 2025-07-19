function downloadFile(url, errorAction, event, timeout, token, isBillingFile = false, isDraftBillingFile = false) {    
    isBillingFile = String(isBillingFile).toLowerCase() === "true";
    isDraftBillingFile = String(isDraftBillingFile).toLowerCase() === "true";

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
                // Get the Content-Disposition header
                const contentDisposition = xhr.getResponseHeader('Content-Disposition');

                // Regular expression to match the filename and filename* parameters
                const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;

                let filename = '';
                let matches = filenameRegex.exec(contentDisposition);
                if (matches?.[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }

                // If the file is a billing file, adjust the filename accordingly
                if (isBillingFile) {
                    // Remove any existing extension for clarity
                    const baseName = filename.replace(/(\.[^/.]+)?$/, '');
                    if (isDraftBillingFile) {
                        filename = `${baseName}_DRAFT.csv`;
                    } else {
                        filename = `${baseName}_AUTHORISED.csv`;
                    }
                }

                // Create a download link
                const url = window.URL.createObjectURL(data);
                const a = document.createElement('a');
                a.href = url;
                a.download = filename || 'downloaded_file'; // Use extracted filename or a default name
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
            }
            catch (error) {
                window.location.href = errorAction;
            }
        },
        error: function (error) {
            window.location.href = errorAction;
        }
    });
}