$(document).ready(function () {

    $('#passwordInput').on('input', function () {
        let password = $(this).val();

        let hasLength = password.length >= 6;
        let hasUpper = /[A-Z]/.test(password);
        let hasLower = /[a-z]/.test(password);
        let hasDigit = /[0-9]/.test(password);

        $('#ruleLength')
            .toggleClass('text-success', hasLength)
            .toggleClass('text-danger', !hasLength);

        $('#ruleUpper')
            .toggleClass('text-success', hasUpper)
            .toggleClass('text-danger', !hasUpper);

        $('#ruleLower')
            .toggleClass('text-success', hasLower)
            .toggleClass('text-danger', !hasLower);

        $('#ruleDigit')
            .toggleClass('text-success', hasDigit)
            .toggleClass('text-danger', !hasDigit);
    });

    $('#confirmPasswordInput').on('input', function () {
        let password = $('#passwordInput').val();
        let confirmPassword = $(this).val();

        if (confirmPassword !== password) {
            $('#confirmPasswordError')
                .text('Пароли не совпадают');
        } else {
            $('#confirmPasswordError')
                .text('');
        }
    });

    $('#registerForm').submit(function (event) {
        let password = $('#passwordInput').val();
        let confirmPassword = $('#confirmPasswordInput').val();

        let hasLength = password.length >= 6;
        let hasUpper = /[A-Z]/.test(password);
        let hasLower = /[a-z]/.test(password);
        let hasDigit = /[0-9]/.test(password);

        if (!hasLength ||
            !hasUpper ||
            !hasLower ||
            !hasDigit) {

            event.preventDefault();
            return;
        }

        if (password !== confirmPassword) {
            event.preventDefault();

            $('#confirmPasswordError')
                .text('Пароли не совпадают');
        }
    });

});