//
//	UniWebViewManager.java
//  Created by Wang Wei(@onevcat) on 2013-11-30.
//
package com.onevcat.uniwebview;

import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;

public class UniWebViewManager
{
    private HashMap<String, UniWebViewDialog> _webViewDialogDic;
    private ArrayList<UniWebViewDialog> _showingWebViewDialogs;

    private static UniWebViewManager _instance = null;

    public UniWebViewManager() {
        _webViewDialogDic = new HashMap<String, UniWebViewDialog>();
        _showingWebViewDialogs = new ArrayList<UniWebViewDialog>();
    }

    public static UniWebViewManager Instance() {
        if (_instance == null) {
            _instance = new UniWebViewManager();
        }
        return _instance;
    }

    public UniWebViewDialog getUniWebViewDialog(String name) {
        if (name != null && name.length() != 0 && _webViewDialogDic.containsKey(name)) {
            return _webViewDialogDic.get(name);
        }
        return null;
    }

    public void removeUniWebView(String name) {
        if (_webViewDialogDic.containsKey(name)) {
            _webViewDialogDic.remove(name);
        }
    }

    public void setUniWebView(String name, UniWebViewDialog webViewDialog) {
        _webViewDialogDic.put(name, webViewDialog);
    }

    public Collection<UniWebViewDialog> allDialogs() {
        return _webViewDialogDic.values();
    }

    public void addShowingWebViewDialog(UniWebViewDialog webViewDialog) {
        if (!_showingWebViewDialogs.contains(webViewDialog)) {
            _showingWebViewDialogs.add(webViewDialog);
        }
    }

    public void removeShowingWebViewDialog(UniWebViewDialog webViewDialog) {
        _showingWebViewDialogs.remove(webViewDialog);
    }

    public ArrayList<UniWebViewDialog> getShowingWebViewDialogs() {
        return _showingWebViewDialogs;
    }
}