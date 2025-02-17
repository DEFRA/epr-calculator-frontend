(function () {
    function preventFormReSubmission(formElement) {
        if (formElement.dataset.submitted === "true") {
            return false;
        }
        formElement.dataset.submitted = "true";
        return true;
    }
    window.preventFormReSubmission = preventFormReSubmission;
})();