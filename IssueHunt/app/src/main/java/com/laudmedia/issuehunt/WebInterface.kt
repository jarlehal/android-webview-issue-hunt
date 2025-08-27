package com.laudmedia.issuehunt

import android.content.Context
import android.webkit.JavascriptInterface

class WebInterface(private val context: Context) {

    @JavascriptInterface
    fun closeApp() {
        when (context) {
            is MainActivity -> context.finish()
        }
    }

}