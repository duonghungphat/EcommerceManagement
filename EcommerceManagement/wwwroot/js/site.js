document.addEventListener("DOMContentLoaded", function () {
    const successAlerts = document.querySelectorAll(".alert-success");

    successAlerts.forEach(function (alert) {
        setTimeout(function () {
            alert.classList.remove("show");

            setTimeout(function () {
                alert.remove();
            }, 300);
        }, 3000);
    });
});