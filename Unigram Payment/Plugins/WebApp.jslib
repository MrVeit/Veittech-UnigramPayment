const webAppLibrary = {

    // Class definition

    $webApp: {
        isTelegramApp: function() {
            return typeof Telegram !== 'undefined' && Telegram.WebApp !== null;
        },

        isVersionAtLeast: function(version) {
            if (!webApp.isTelegramApp()) {
                return false;
            }

            return Telegram.WebApp.isVersionAtLeast(version);
        },

        isExpanded: function() {
            if (!webApp.isTelegramApp()) {
                return false;
            }

            return Telegram.WebApp.isExpanded;
        },

        getVersion: function() {
            if (!webApp.isTelegramApp()) {
                return "";
            }

            return Telegram.WebApp.version;
        },

        expand: function() {
            if (!webApp.isTelegramApp()) {
                return;
            }

            Telegram.WebApp.expand();
        },

        close: function() {
            if (!webApp.isTelegramApp()) {
                return;
            }

            Telegram.WebApp.close();
        },

        openExternalLink: function(url, tryInstantView) {
            if (!webApp.isTelegramApp()) {
                return;
            }

            let options = tryInstantView ? { try_instant_view: true } : {};

            Telegram.WebApp.openLink(url, options);
        },

        openTelegramLink: function(url) {
            if (!webApp.isTelegramApp()) {
                return;
            }

            Telegram.WebApp.openTelegramLink(url);
        },

        openInvoice: function(link, successCallbackPtr, errorCallbackPtr) {
            if (!webApp.isTelegramApp()) {
                return;
            }

            var invoiceLink = UTF8ToString(link);

            Telegram.WebApp.openInvoice(invoiceLink, function(status) {
                Telegram.WebApp.onEvent('invoiceClosed', function(event) {
                    var statusPtr = allocate(intArrayFromString(event.status), ALLOC_NORMAL);

                    if (event.status === "paid") {
                        var paymentJson = JSON.stringify(event);
                        var paymentPtr = allocate(intArrayFromString(paymentJson), ALLOC_NORMAL);

                        dynCall('vii', successCallbackPtr, [statusPtr, paymentPtr]);

                        _free(paymentPtr);
                    }
                    else {
                        dynCall('vi', errorCallbackPtr, [statusPtr]);
                    }

                    _free(statusPtr);
                });
            });
        }
    },

    // External C# calls

    IsTelegramApp: function() {
        return webApp.isTelegramApp();
    },

    IsSupportedVersionActive: function(version) {
        return webApp.isVersionAtLeast(version);
    },

    IsExpanded: function() {
        return webApp.isExpanded();
    },

    GetCurrentVersion: function() {
        return webApp.getVersion();
    },

    Expand: function() {
        webApp.expand();
    },

    Close: function() {
        webApp.close();
    },

    OpenInvoice: function(link, successCallbackPtr, errorCallbackPtr) {
        webApp.openInvoice(link, successCallbackPtr, errorCallbackPtr);
    },

    OpenExternalLink: function(url, tryInstantView) {
        webApp.openExternalLink(url, tryInstantView);
    },

    OpenTelegramLink: function(url) {
        webApp.openTelegramLink(url);
    }
}

autoAddDeps(webAppLibrary, '$webApp');
mergeInto(LibraryManager.library, webAppLibrary);