window.laudMediaAndroid = {
    closeApp: () => {
        try {
            Android.closeApp()
        } catch (e) {
            window.close()
        }
    }
};
