const webAppLibrary = {

    // Class definition

    $webApp: {
        isTelegramApp: function()
        {
            return typeof Telegram !== 'undefined' && Telegram.WebApp !== null;
        },

        getUnsafeInitData: function()
        {
            if (!webApp.isTelegramApp())
            {
                console.error("Failed to claim unsafe init data");

                return allocate(intArrayFromString(""), ALLOC_NORMAL);
            }

            var initDataUnsafe = Telegram.WebApp.initDataUnsafe;

            if (initDataUnsafe == null)
            {
                console.error("Failed to claim unsafe init data, because is null");

                return allocate(intArrayFromString(""), ALLOC_NORMAL);
            }

            if (initDataUnsafe.user == null)
            {
                console.error("Failed to parse user data in unsafe init data");

                return allocate(intArrayFromString(""), ALLOC_NORMAL);
            }
            
            console.log(JSON.stringify(initDataUnsafe));

            let filteredData = 
            {
                id: initDataUnsafe.user.id || null,
                first_name: initDataUnsafe.user.first_name || null,
                last_name: initDataUnsafe.user.last_name || null,
                username: initDataUnsafe.user.username || null,
                language_code: initDataUnsafe.user.language_code || null,
                start_param: initDataUnsafe.start_param || null,
                auth_date: initDataUnsafe.auth_date || null,
                hash: initDataUnsafe.hash || null
            };

            var jsonString = JSON.stringify(filteredData);

            console.log(`Successfully claimed unsafe init data: ${jsonString}`);

            return allocate(intArrayFromString(jsonString), ALLOC_NORMAL);
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
                return "";
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
                    var statusPtr = allocate(intArrayFromString(event.status), ALLOC_NORMAL);

                    if (event.status === "paid")
                    {
                        var paymentJson = JSON.stringify(event);
                        var paymentPtr = allocate(intArrayFromString(paymentJson), ALLOC_NORMAL);

                        dynCall('vii', successCallbackPtr, [statusPtr, paymentPtr]);

                        _free(paymentPtr);
                    }
                    else 
                    {
                        dynCall('vi', errorCallbackPtr, [statusPtr]);
                    }

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