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
                // Get the Content-Disposition header
                const contentDisposition = xhr.getResponseHeader('Content-Disposition');               

                if (isEmpty(fileName)) {

                    // Regular expression to match the filename and filename* parameters
                    const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;

                    let matches = filenameRegex.exec(contentDisposition);
                    if (matches?.[1]) {
                        fileName = matches[1].replace(/['"]/g, '');
                    }
                }

                // Create a download link
                const url = window.URL.createObjectURL(data);
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName || 'downloaded_file'; // Use extracted filename or a default name
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

console.log(isEmpty("cat")); // false
console.log(isEmpty(1)); // false
console.log(isEmpty([])); // false
console.log(isEmpty({})); // false
console.log(isEmpty(false)); // false
console.log(isEmpty(0)); // false
console.log(isEmpty(-0)); // false
console.log(isEmpty(NaN)); // false

console.log(isEmpty("")); // true
console.log(isEmpty("    ")); // true
console.log(isEmpty(null)); // true
console.log(isEmpty(undefined)); // true
