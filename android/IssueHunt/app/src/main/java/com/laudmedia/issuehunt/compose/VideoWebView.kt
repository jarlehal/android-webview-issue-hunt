package com.laudmedia.issuehunt.compose

import android.annotation.SuppressLint
import android.net.Uri
import android.os.Build
import android.util.Log
import android.view.ViewGroup
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebResourceResponse
import android.webkit.WebSettings
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.net.toUri
import com.laudmedia.issuehunt.WebInterface
import com.laudmedia.issuehunt.ui.theme.IssueHuntTheme

@SuppressLint("SetJavaScriptEnabled")
@Composable
fun VideoWebView() {
    val videoUrl = "https://staging-awv-issue.laud-media.com"

    val errorPage = """
        <?xml version="1.0" encoding="UTF-8" ?>
        <html>
            <head>
                <style>
                    body {
                        background-color: #222222;
                        color: #aaaaaa;
                    }
                    .error-container {
                        padding: 2rem;
                    }
                </style>
            </head>
            <script>
                function runOnLoad() {
                    setTimeout( function() {
                        window.location = '${videoUrl}'
                    }, 2000);
                }
            </script>
            
            <body onLoad="runOnLoad();">
                <div class="error-container">                
                    <h2>No Network..</h2>
                    <h3>Please make sure this device is connected to Internet</h3>
                </div>
            </body>
        </html>
    """.trimIndent()
    val errorPageBase64 = android.util.Base64.encodeToString(errorPage.toByteArray(charset = Charsets.UTF_8), android.util.Base64.DEFAULT)

    val triggerOnThese = arrayOf(videoUrl.toUri(), "${videoUrl}/_framework/blazor.webassembly.js".toUri())

    fun triggerNetworkErrorOn(url: Uri?):Boolean {
        return triggerOnThese.contains(url)
    }

    val wvClient = object : WebViewClient() {
        override fun onReceivedError(view: WebView?, request: WebResourceRequest?, error: WebResourceError?) {
            Log.e("VideoWebView", "error: ${error?.errorCode} ${error?.description} - ${request?.url}")
            if (error?.errorCode == -2 && triggerNetworkErrorOn(request?.url)) {
                view?.loadData(errorPageBase64, "text/html; charset=utf-8", "base64")
            }
            super.onReceivedError(view, request, error)
        }

        override fun onReceivedHttpError(view: WebView?, request: WebResourceRequest?, errorResponse: WebResourceResponse?) {
            Log.e("VideoWebView", "error: ${errorResponse?.statusCode} ${errorResponse?.reasonPhrase}")

            super.onReceivedHttpError(view, request, errorResponse)
        }
    }

    AndroidView(
        factory = {
            WebView(it).apply {
                addJavascriptInterface(WebInterface(this.context), "Android")
                layoutParams = ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT,
                    ViewGroup.LayoutParams.MATCH_PARENT
                )
                settings.javaScriptEnabled = true
                settings.databaseEnabled = true
                settings.domStorageEnabled = true
                settings.cacheMode = WebSettings.LOAD_NO_CACHE;
                webViewClient = wvClient
            }
        },
        update = {
            it.clearCache(true)
            it.loadUrl(videoUrl)
        },
        onReset = {
            Log.d("VideoWebView","OnReset")
        },
        onRelease = {
            it.loadUrl("about:blank")
            Log.d("VideoWebView","OnRelease")
        }
    )
}

@Preview(showBackground = true)
@Composable
fun DefaultPreview() {
    IssueHuntTheme {
        VideoWebView()
    }
}