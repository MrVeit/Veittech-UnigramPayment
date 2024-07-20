const eventListener = {

    // Class definition

    $listener: {
        onAppSizeChanged: function(minimizedCallbackPtr, restoredCallbackPtr) {
            Telegram.WebApp.onEvent('viewportChanged', function() {
                if (Telegram.WebApp.viewportStableHeight === 0) {
                    dynCall('v', minimizedCallbackPtr, []);
                } 
                else {
                    dynCall('v', restoredCallbackPtr, []);
                }
            });
        }
    },

    // External C# calls

    OnAppSizeChanged: function(minimizedCallbackPtr, restoredCallbackPtr) {
        listener.onAppSizeChanged(minimizedCallbackPtr, restoredCallbackPtr);
    }
};

autoAddDeps(eventListener, '$listener');
mergeInto(LibraryManager.library, eventListener);