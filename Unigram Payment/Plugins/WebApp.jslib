const webAppLibrary = {

    // Class definition

    $webApp: {
        getAllocString: function(data)
        {
            let ptr;

            if (typeof allocate === 'undefined')
            {
                console.log(`[UNIGRAM PAYMENT] Detected Unity version 2023+`);

                const length = lengthBytesUTF8(data) + 1;

                ptr = _malloc(length);

                stringToUTF8(data, ptr, length);

                return ptr;
            }

            return allocate(intArrayFromString(data), 'i8', ALLOC_NORMAL);
        },

        isTelegramApp: function()
        {
            return typeof Telegram !== 'undefined' && Telegram.WebApp !== null;
        },

        getUnsafeInitData: function()
        {
            if (!webApp.isTelegramApp())
            {
                console.error("[UNIGRAM PAYMENT] Failed to claim unsafe init data");

                return webApp.getAllocString("");
            }

            var initDataUnsafe = Telegram.WebApp.initDataUnsafe;

            if (initDataUnsafe == null)
            {
                console.error("[UNIGRAM PAYMENT] Failed to claim "+
                    "unsafe init data, because is null");

                return webApp.getAllocString("");
            }

            if (initDataUnsafe.user == null)
            {
                console.error("[UNIGRAM PAYMENT] Failed to parse "+
                    "user data in unsafe init data");

                return webApp.getAllocString("");
            }

            let filteredData = 
            {
                id: initDataUnsafe.user.id || null,
                first_name: initDataUnsafe.user.first_name || null,
                last_name: initDataUnsafe.user.last_name || null,
                username: initDataUnsafe.user.username || null,
                start_param: initDataUnsafe.start_param || null,
            };

            var jsonString = JSON.stringify(filteredData);

            console.log(`[UNIGRAM PAYMENT] Successfully claimed unsafe init data: ${jsonString}`);

            return webApp.getAllocString(jsonString);
        },

        isVersionAtLeast: function(version)
        {
            if (!webApp.isTelegramApp())
            {
                return false;
            }

            return Telegram.WebApp.isVersionAtLeast(version);
        },

        isExpanded: function()
        {
            if (!webApp.isTelegramApp())
            {
                return false;
            }

            return Telegram.WebApp.isExpanded;
        },

        getVersion: function()
        {
            if (!webApp.isTelegramApp())
            {
                return webApp.getAllocString("");
            }

            return Telegram.WebApp.version;
        },

        expand: function()
        {
            if (!webApp.isTelegramApp())
            {
                return;
            }

            Telegram.WebApp.expand();
        },

        close: function()
        {
            if (!webApp.isTelegramApp())
            {
                return;
            }

            Telegram.WebApp.close();
        },

        openExternalLink: function(url, tryInstantView)
        {
            if (!webApp.isTelegramApp())
            {
                return;
            }

            let options = tryInstantView ? { try_instant_view: true } : {};

            Telegram.WebApp.openLink(url, options);
        },

        openTelegramLink: function(url)
        {
            if (!webApp.isTelegramApp())
            {
                return;
            }

            Telegram.WebApp.openTelegramLink(url);
        },

        openInvoice: function(link, successCallbackPtr, errorCallbackPtr)
        {
            if (!webApp.isTelegramApp())
            {
                return;
            }

            var invoiceLink = UTF8ToString(link);

            Telegram.WebApp.openInvoice(invoiceLink, function(status)
            {
                Telegram.WebApp.onEvent('invoiceClosed', function(event)
                {
                    var statusPtr = webApp.getAllocString(event.status);

                    if (event.status === "paid")
                    {
                        var paymentJson = JSON.stringify(event);
                        var paymentPtr = webApp.getAllocString(paymentJson);

                        dynCall('vii', successCallbackPtr, [statusPtr, paymentPtr]);

                        _free(paymentPtr);

                        return;
                    }
                    
                    dynCall('vi', errorCallbackPtr, [statusPtr]);

                    _free(statusPtr);
                });
            });
        }
    },

    // External C# calls

    IsTelegramApp: function()
    {
        return webApp.isTelegramApp();
    },

    GetUnsafeInitData: function()
    {
        return webApp.getUnsafeInitData();
    },

    IsSupportedVersionActive: function(version)
    {
        return webApp.isVersionAtLeast(version);
    },

    IsExpanded: function()
    {
        return webApp.isExpanded();
    },

    GetCurrentVersion: function()
    {
        return webApp.getVersion();
    },

    Expand: function()
    {
        webApp.expand();
    },

    Close: function()
    {
        webApp.close();
    },

    OpenInvoice: function(link, successCallbackPtr, errorCallbackPtr)
    {
        webApp.openInvoice(link, successCallbackPtr, errorCallbackPtr);
    },

    OpenExternalLink: function(url, tryInstantView)
    {
        webApp.openExternalLink(url, tryInstantView);
    },

    OpenTelegramLink: function(url)
    {
        webApp.openTelegramLink(url);
    }
}

autoAddDeps(webAppLibrary, '$webApp');
mergeInto(LibraryManager.library, webAppLibrary);