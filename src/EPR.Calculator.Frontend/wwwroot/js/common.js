    function preventFormReSubmission(event, formElement) {
        if (formElement.dataset.submitted) {
            event.preventDefault();
            return;
        }
        formElement.dataset.submitted = true;       
    }
