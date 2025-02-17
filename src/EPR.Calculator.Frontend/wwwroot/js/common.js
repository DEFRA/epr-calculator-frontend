(function () {
    function preventFormReSubmission(event, formElement) {
        if (formElement.dataset.submitted) {
            event.preventDefault();
            window.history.back();
            return;
        }
        formElement.dataset.submitted = true;       
    }
    window.preventFormReSubmission = preventFormReSubmission;
})();