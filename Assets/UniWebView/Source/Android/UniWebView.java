/**
 * Created by onevcat on 2013/12/02.
 * You can modify, rebuild and use this file if you purchased it.
 * But you can not redistribute it in any form.
 * Copyright and all rights reserved OneV's Den.
 */
 package com.onevcat.uniwebview;

import android.annotation.SuppressLint;
import android.content.Context;
import android.os.Build;
import android.view.ViewGroup;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.widget.FrameLayout;

@SuppressLint("SetJavaScriptEnabled")
public class UniWebView extends WebView{
    public static String customUserAgent;

    public UniWebView(Context context) {
        super(context);
        WebSettings webSettings = this.getSettings();
        webSettings.setJavaScriptEnabled(true);
        webSettings.setDatabaseEnabled(true);
        webSettings.setDomStorageEnabled(true);
        webSettings.setAllowFileAccess(true);
        webSettings.setGeolocationEnabled(true);

        if (UniWebView.customUserAgent != null && !UniWebView.customUserAgent.equals("")) {
            webSettings.setUserAgentString(UniWebView.customUserAgent);
        }

        if (Build.VERSION.SDK_INT >= 8) {
            webSettings.setPluginState(WebSettings.PluginState.ON);
        } else {
            webSettings.setPluginsEnabled(true);
        }

        if (Build.VERSION.SDK_INT >= 11) {
            webSettings.setDisplayZoomControls(false);
        }

        //The default value of these two is true before API level 16
        //And these two APIs are added in 16
        if (Build.VERSION.SDK_INT >= 16) {
            webSettings.setAllowFileAccessFromFileURLs(true);
            webSettings.setAllowUniversalAccessFromFileURLs(true);
        }

        setScrollBarStyle(WebView.SCROLLBARS_INSIDE_OVERLAY);
        setVerticalScrollbarOverlay(true);
        setLayoutParams(new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT,ViewGroup.LayoutParams.MATCH_PARENT));
    }

    public void updateTransparent(boolean transparent) {
        if (transparent) {
            this.setBackgroundColor(0x00000000);
            if (Build.VERSION.SDK_INT >= 11) {
                this.setLayerType(WebView.LAYER_TYPE_SOFTWARE, null);
            }
        } else {
            this.setBackgroundColor(0xFFFFFFFF);
            if (Build.VERSION.SDK_INT >= 11) {
                this.setLayerType(WebView.LAYER_TYPE_NONE, null);
            }
        }
    }
}
