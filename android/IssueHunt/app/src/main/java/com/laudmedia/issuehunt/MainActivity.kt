package com.laudmedia.issuehunt

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import com.laudmedia.issuehunt.compose.VideoWebView
import com.laudmedia.issuehunt.ui.theme.IssueHuntTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            IssueHuntTheme {
                VideoWebView()
            }
        }
    }
}


@Preview(showBackground = true)
@Composable
fun GreetingPreview() {
    IssueHuntTheme {
        VideoWebView()
    }
}