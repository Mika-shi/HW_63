function loadMessages() {
    $.ajax({
        url: '/Chat/GetMessages',
        type: 'GET',
        success: function (messages) {
            let chat = $('#chatMessages');

            chat.empty();

            messages.forEach(function (message) {
                let messageClass = message.isMine
                    ? 'justify-content-end'
                    : 'justify-content-start';

                let avatar = '';

                if (message.avatarPath) {
                    avatar = `
                        <img src="${message.avatarPath}" style="width:45px;height:45px;object-fit:cover;border-radius:50%;">
                    `;
                } else {
                    let firstLetter = message.userName
                        ? message.userName.charAt(0).toUpperCase()
                        : '?';

                    avatar = `
                        <div class="d-flex align-items-center justify-content-center bg-secondary text-white"
                             style="width:45px;height:45px;border-radius:50%;font-weight:bold;">
                            ${firstLetter}
                        </div>
                    `;
                }

                let bubbleClass = message.isMine
                    ? 'bg-primary text-white'
                    : 'bg-light';

                let html = `
                    <div class="d-flex ${messageClass} mb-3">
                        <div class="d-flex align-items-end gap-2" style="max-width:75%;">

                            ${!message.isMine ? avatar : ''}

                            <div style="min-width:120px;">
                                <div class="mb-1">
                                    <a href="/Profile/Details/${message.userId}" class="text-decoration-none fw-bold">
                                        ${message.userName}
                                    </a>
                                </div>

                                <div class="rounded p-2 ${bubbleClass}">
                                    <div style="word-break:break-word;">
                                        ${message.text}
                                    </div>

                                    <div class="text-end mt-1">
                                        <small>${message.createdOn}</small>
                                    </div>
                                </div>
                            </div>

                            ${message.isMine ? avatar : ''}

                        </div>
                    </div>
                `;

                chat.append(html);
            });

            if (chat.length) {
                chat.scrollTop(chat[0].scrollHeight);
            }
        }
    });
}
$(document).ready(function () {

    $('.password-toggle').click(function () {
        let button = $(this);
        let targetId = button.data('target');
        let input = $('#' + targetId);
        let icon = button.find('i');

        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('bi-eye');
            icon.addClass('bi-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('bi-eye-slash');
            icon.addClass('bi-eye');
        }
    });

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
            $('#confirmPasswordError').text('Пароли не совпадают');
        } else {
            $('#confirmPasswordError').text('');
        }
    });

    $('#registerForm').submit(function (event) {
        let password = $('#passwordInput').val();
        let confirmPassword = $('#confirmPasswordInput').val();

        let hasLength = password.length >= 6;
        let hasUpper = /[A-Z]/.test(password);
        let hasLower = /[a-z]/.test(password);
        let hasDigit = /[0-9]/.test(password);

        if (!hasLength || !hasUpper || !hasLower || !hasDigit) {
            event.preventDefault();
            return;
        }

        if (password !== confirmPassword) {
            event.preventDefault();
            $('#confirmPasswordError').text('Пароли не совпадают');
        }
    });

    if ($('#chatMessages').length) {
        loadMessages();

        setInterval(function () {
            loadMessages();
        }, 5000);
    }

    $('#sendMessageButton').click(function () {
        let text = $('#messageText').val().trim();

        if (text.length === 0) {
            $('#messageError').text('Введите сообщение');
            return;
        }

        if (text.length > 150) {
            $('#messageError').text('Сообщение не должно превышать 150 символов');
            return;
        }

        $('#messageError').text('');

        $.ajax({
            url: '/Chat/SendMessage',
            type: 'POST',
            data: {
                text: text
            },
            success: function (result) {
                if (result.success) {
                    $('#messageText').val('');

                    loadMessages();
                } else {
                    $('#messageError').text(result.error);
                }
            }
        });
    });

    $('#messageText').on('input', function () {
        let length = $(this).val().length;

        $('#messageCounter').text(length + ' / 150');

        if (length >= 150) {
            $('#messageError').text('Достигнут лимит в 150 символов');
            $('#messageCounter').removeClass('text-muted').addClass('text-danger');
        } else {
            $('#messageError').text('');
            $('#messageCounter').removeClass('text-danger').addClass('text-muted');
        }
    });
});