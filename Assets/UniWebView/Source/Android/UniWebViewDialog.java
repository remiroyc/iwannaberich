/**
 * Created by onevcat on 2013/12/02.
 * You can modify, rebuild and use this file if you purchased it.
 * But you can not redistribute it in any form.
 * Copyright and all rights reserved OneV's Den.
 */
package com.onevcat.uniwebview;

import android.annotation.SuppressLint;
import android.app.AlertDialog;
import android.app.Dialog;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Point;
import android.graphics.drawable.ColorDrawable;
import android.net.Uri;
import android.net.http.SslError;
import android.os.Build;
import android.text.Editable;
import android.util.Log;
import android.view.Display;
import android.view.Gravity;
import android.view.KeyEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.view.inputmethod.InputMethodManager;
import android.webkit.DownloadListener;
import android.webkit.GeolocationPermissions;
import android.webkit.JavascriptInterface;
import android.webkit.JsPromptResult;
import android.webkit.JsResult;
import android.webkit.SslErrorHandler;
import android.webkit.ValueCallback;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.EditText;
import android.widget.FrameLayout;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;

public class UniWebViewDialog extends Dialog {

    public ArrayList<String> schemes;

    private FrameLayout _content;
    private ProgressDialog _spinner;
    private UniWebView _uniWebView;
    private DialogListener _listener;
    private boolean _showSpinnerWhenLoading = true;
    private String _spinnerText = "Loading...";
    private boolean _isLoading;
    private boolean _loadingInterrupted;
    private int _top, _left, _bottom, _right;
    private AlertDialog _alertDialog;
    private String _currentUrl = "";
    private boolean _transparent;
    private boolean _backButtonEnable = true;
    private boolean _manualHide;
    private String _currentUserAgent;

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        Log.d(AndroidPlugin.LOG_TAG, "onKeyDown " + event);
        this._listener.onDialogKeyDown(this, keyCode);
        if (keyCode == KeyEvent.KEYCODE_BACK) {
            if (!_backButtonEnable) {
                return true;
            } else if (!goBack()) {
                this._listener.onDialogShouldCloseByBackButton(this);
            }
            return true;
        } else {
            return super.onKeyDown(keyCode, event);
        }
    }

    @SuppressLint("NewApi")
    public UniWebViewDialog(Context context, DialogListener listener) {
        super(context,android.R.style.Theme_Holo_NoActionBar);
        this._listener = listener;

        schemes = new ArrayList<String>();
        schemes.add("uniwebview");

        Window window = this.getWindow();
        window.setBackgroundDrawable(new ColorDrawable(android.graphics.Color.TRANSPARENT));
        window.addFlags(WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL);
        window.setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);

        if (Build.VERSION.SDK_INT < 16) {
            window.addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
        } else {
            HideSystemUI();
        }

        createContent();
        createWebView();
        createSpinner();

        addContentView(this._content,
                       new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT,ViewGroup.LayoutParams.MATCH_PARENT));
        this._content.addView(this._uniWebView);
        Log.d(AndroidPlugin.LOG_TAG, "Create a new UniWebView Dialog");
    }

    @SuppressLint("NewApi")
    public void HideSystemUI() {
        if (Build.VERSION.SDK_INT >= 16) {
            final View decorView = this.getWindow().getDecorView();
            final int uiOptions = View.SYSTEM_UI_FLAG_FULLSCREEN;
            decorView.setSystemUiVisibility(uiOptions);

            // Fix input method showing causes ui show issue.
            decorView.setOnSystemUiVisibilityChangeListener(new View.OnSystemUiVisibilityChangeListener() {
                @Override
                public void onSystemUiVisibilityChange(int i) {
                    decorView.setSystemUiVisibility(View.SYSTEM_UI_FLAG_FULLSCREEN);
                }
            });
        }
    }

    public void changeSize(int top, int left, int bottom, int right) {
        _top = top;
        _left = left;
        _bottom = bottom;
        _right = right;
        updateContentSize();
    }

    public void load(String url) {
        Log.d(AndroidPlugin.LOG_TAG, url);
        _uniWebView.loadUrl(url);
    }

    public void addJs(String js) {
        if (js == null) {
            Log.d(AndroidPlugin.LOG_TAG, "Trying to add a null js. Abort.");
            return;
        }

        String requestString = String.format("javascript:%s",js);
        load(requestString);
    }

    public void loadJS(String js) {
        if (js == null) {
            Log.d(AndroidPlugin.LOG_TAG, "Trying to eval a null js. Abort.");
            return;
        }

        String jsReformat = js.trim();

        while (jsReformat.endsWith(";") && jsReformat.length() != 0) {
            jsReformat = jsReformat.substring(0, jsReformat.length()-1);
        }

        String requestString = String.format("javascript:android.onData(%s)", jsReformat);
        load(requestString);
    }

    public void loadHTMLString(String html, String baseURL) {
        _uniWebView.loadDataWithBaseURL(baseURL, html, "text/html", "UTF-8", null);
    }

    public void cleanCache() {
        _uniWebView.clearCache(true);
    }

    public boolean goBack() {
        if (_uniWebView.canGoBack()) {
            _uniWebView.goBack();
            return true;
        } else {
            return false;
        }
    }

    public boolean goForward() {
        if (_uniWebView.canGoForward()) {
            _uniWebView.goForward();
            return true;
        } else {
            return false;
        }
    }

    public void destroy() {
        _uniWebView.loadUrl("about:blank");
        UniWebViewManager.Instance().removeShowingWebViewDialog(this);
        this.dismiss();
    }

    protected void onStop() {
        this._listener.onDialogClose(this);
    }

    public void setShow(boolean show) {
        if (show) {
            this.show();
            if (this._showSpinnerWhenLoading && this._isLoading) {
                this._spinner.show();
            }
            UniWebViewManager.Instance().addShowingWebViewDialog(this);
            _manualHide = false;
        } else {
            InputMethodManager imm = (InputMethodManager)UnityPlayer.currentActivity.getSystemService(Context.INPUT_METHOD_SERVICE);
            imm.hideSoftInputFromWindow(_uniWebView.getWindowToken(), 0);
            this._spinner.hide();
            this.hide();
            _manualHide = true;
        }
    }

    public void updateContentSize() {
        Window window = this.getWindow();
        Display display = window.getWindowManager().getDefaultDisplay();
        int width;
        int height;

        if (android.os.Build.VERSION.SDK_INT >= 13) {
            Point size = new Point();
            display.getSize(size);
            width = size.x;
            height = size.y;
        } else {
            width = display.getWidth();
            height = display.getHeight();
        }

        window.setLayout(width - _left - _right, height - _top - _bottom);

        WindowManager.LayoutParams layoutParam = window.getAttributes();
        layoutParam.gravity = Gravity.TOP | Gravity.LEFT;
        layoutParam.x = _left;
        layoutParam.y = _top;

        window.setAttributes(layoutParam);
    }

    public void setSpinnerShowWhenLoading(boolean showSpinnerWhenLoading) {
        this._showSpinnerWhenLoading = showSpinnerWhenLoading;
    }

    public void setSpinnerText(String text) {
        if (text != null) {
            this._spinnerText = text;
        } else {
            this._spinnerText = "";
        }
        this._spinner.setMessage(text);
    }

    private void createContent() {
        this._content = new FrameLayout(getContext());
        this._content.setVisibility(View.VISIBLE);
    }

    private void createSpinner() {
        this._spinner = new ProgressDialog(getContext());
        this._spinner.setCanceledOnTouchOutside(true);
        this._spinner.requestWindowFeature(Window.FEATURE_NO_TITLE);
        this._spinner.setMessage(this._spinnerText);
    }

    private void createWebView() {
        _uniWebView = new UniWebView(getContext());

        WebViewClient webClient = new WebViewClient() {
            @Override
            public void onPageStarted(WebView view, String url, Bitmap favicon) {
                Log.d(AndroidPlugin.LOG_TAG, "Start Loading URL: " + url);
                super.onPageStarted(view, url, favicon);
                if (UniWebViewDialog.this._showSpinnerWhenLoading && UniWebViewDialog.this.isShowing()) {
                    UniWebViewDialog.this._spinner.show();
                }
                UniWebViewDialog.this._isLoading = true;
                UniWebViewDialog.this._listener.onPageStarted(UniWebViewDialog.this,url);
            }

            @Override
            public void onPageFinished(WebView view, String url) {
                UniWebViewDialog.this._spinner.hide();
                _currentUrl = url;
                _currentUserAgent = _uniWebView.getSettings().getUserAgentString();
                UniWebViewDialog.this._listener.onPageFinished(UniWebViewDialog.this, url);
                UniWebViewDialog.this._isLoading = false;
                _uniWebView.updateTransparent(_transparent);
            }

            @Override
            public void onReceivedError (WebView view, int errorCode, String description, String failingUrl) {
                UniWebViewDialog.this._spinner.hide();
                _currentUrl = failingUrl;
                _currentUserAgent = _uniWebView.getSettings().getUserAgentString();
                UniWebViewDialog.this._listener.onReceivedError(UniWebViewDialog.this,errorCode,description,failingUrl);
                UniWebViewDialog.this._isLoading = false;
            }

            @Override
            public boolean shouldOverrideUrlLoading(WebView view, String url) {
                Log.d(AndroidPlugin.LOG_TAG,"shouldOverrideUrlLoading: " + url);
                return UniWebViewDialog.this._listener.shouldOverrideUrlLoading(UniWebViewDialog.this, url);
            }

            @Override
            public void onReceivedSslError (WebView view, SslErrorHandler handler, SslError error) {
                Log.d(AndroidPlugin.LOG_TAG,"onReceivedSslError: " + error.toString());
                handler.proceed();
            }
        };

        _uniWebView.setWebViewClient(webClient);

        UniWebChromeClient chromeClient = new UniWebChromeClient(this._content) {
            //The undocumented magic method override
            //IDE will swear at you if you try to put @Override here
            //For Android 3.0+
            public void openFileChooser(ValueCallback<Uri> uploadMsg) {
                AndroidPlugin.setUploadMessage(uploadMsg);
                Intent i = new Intent(Intent.ACTION_GET_CONTENT);
                i.addCategory(Intent.CATEGORY_OPENABLE);
                i.setType("image/*");
                AndroidPlugin.getActivity().startActivityForResult(Intent.createChooser(i, "File Chooser"), AndroidPlugin.FILECHOOSER_RESULTCODE);
            }

            // For Android 3.0+
            public void openFileChooser( ValueCallback uploadMsg, String acceptType ) {
                AndroidPlugin.setUploadMessage(uploadMsg);
                Intent i = new Intent(Intent.ACTION_GET_CONTENT);
                i.addCategory(Intent.CATEGORY_OPENABLE);
                i.setType("*/*");
                AndroidPlugin.getActivity().startActivityForResult(
                        Intent.createChooser(i, "File Browser"),
                        AndroidPlugin.FILECHOOSER_RESULTCODE);
            }

            //For Android 4.1
            public void openFileChooser(ValueCallback<Uri> uploadMsg, String acceptType, String capture){
                AndroidPlugin.setUploadMessage(uploadMsg);
                Intent i = new Intent(Intent.ACTION_GET_CONTENT);
                i.addCategory(Intent.CATEGORY_OPENABLE);
                i.setType("image/*");
                AndroidPlugin.getActivity().startActivityForResult(Intent.createChooser(i, "File Chooser"), AndroidPlugin.FILECHOOSER_RESULTCODE);
            }

            @Override
            public boolean onJsAlert(WebView view, String url, String message, final JsResult result) {
                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(UniWebViewDialog.this.getContext());
                _alertDialog = alertDialogBuilder
                        .setTitle(url)
                        .setMessage(message)
                        .setCancelable(false)
                        .setIcon(android.R.drawable.ic_dialog_alert)
                        .setPositiveButton(android.R.string.ok, new OnClickListener() {
                            public void onClick(DialogInterface dialog, int id) {
                                dialog.dismiss();
                                result.confirm();
                                _alertDialog = null;
                            }
                        }).show();
                return true;
            }

            @Override
            public boolean onJsConfirm(WebView view, String url, String message, final JsResult result) {
                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(UniWebViewDialog.this.getContext());
                _alertDialog = alertDialogBuilder
                        .setTitle(url)
                        .setMessage(message)
                        .setIcon(android.R.drawable.ic_dialog_info)
                        .setCancelable(false)
                        .setPositiveButton(android.R.string.yes, new OnClickListener() {
                            public void onClick(DialogInterface dialog, int whichButton) {
                                dialog.dismiss();
                                result.confirm();
                                _alertDialog = null;
                            }
                        })
                        .setNegativeButton(android.R.string.no, new OnClickListener() {
                            public void onClick(DialogInterface dialog, int i) {
                                dialog.dismiss();
                                result.cancel();
                                _alertDialog = null;
                            }
                        }).show();
                return true;
            }

            @Override
            public boolean onJsPrompt (WebView view, String url, String message, String defaultValue, final JsPromptResult result) {
                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(UniWebViewDialog.this.getContext());
                alertDialogBuilder
                        .setTitle(url)
                        .setMessage(message)
                        .setIcon(android.R.drawable.ic_dialog_info)
                        .setCancelable(false);

                final EditText input = new EditText(UniWebViewDialog.this.getContext());
                input.setSingleLine();
                alertDialogBuilder.setView(input);

                alertDialogBuilder.setPositiveButton(android.R.string.yes, new DialogInterface.OnClickListener() {
                    public void onClick(DialogInterface dialog, int whichButton) {
                        Editable editable = input.getText();
                        String value = "";
                        if (editable != null) {
                            value = editable.toString();
                        }
                        dialog.dismiss();
                        result.confirm(value);
                        _alertDialog = null;
                    }
                });

                alertDialogBuilder.setNegativeButton(android.R.string.no, new DialogInterface.OnClickListener() {
                    public void onClick(DialogInterface dialog, int whichButton) {
                        dialog.dismiss();
                        result.cancel();
                        _alertDialog = null;
                    }
                });
                _alertDialog = alertDialogBuilder.show();

                return true;
            }

            public void onGeolocationPermissionsShowPrompt(String origin, GeolocationPermissions.Callback callback) {
                // callback.invoke(String origin, boolean allow, boolean remember);
                callback.invoke(origin, true, false);
            }
        };

        _uniWebView.setWebChromeClient(chromeClient);

        _uniWebView.setDownloadListener(new DownloadListener() {
            public void onDownloadStart(String url, String userAgent,
                                        String contentDisposition, String mimetype,
                                        long contentLength) {
                Intent i = new Intent(Intent.ACTION_VIEW);
                i.setData(Uri.parse(url));
                AndroidPlugin.getActivity().startActivity(i);
            }
        });

        this._uniWebView.setVisibility(View.VISIBLE);

        _uniWebView.addJavascriptInterface(this, "android");

        setBounces(false);
    }

    @JavascriptInterface
    public void onData(String value) {
        Log.d(AndroidPlugin.LOG_TAG, "receive a call back from js: " + value);
        this._listener.onJavaScriptFinished(this,value);
    }

    public void goBackGround() {
        if (_isLoading) {
            _loadingInterrupted = true;
            this._uniWebView.stopLoading();
        }
        if (this._alertDialog != null) {
            this._alertDialog.hide();
        }
        this.hide();
    }

    public void goForeGround() {
        if (!_manualHide) {
            if (_loadingInterrupted) {
                this._uniWebView.reload();
                _loadingInterrupted = false;
            }
            this.show();
            if (this._alertDialog != null) {
                this._alertDialog.show();
            }
        }
    }

    public void setTransparent(boolean transparent) {
        _transparent = transparent;
        _uniWebView.updateTransparent(_transparent);
    }

    public String getUrl() {
        return _currentUrl;
    }

    public void setBackButtonEnable(boolean enable) {
        _backButtonEnable = enable;
    }

    public void setBounces(boolean enable) {
        if (android.os.Build.VERSION.SDK_INT <= 8) {
            Log.d(AndroidPlugin.LOG_TAG, "WebView over scroll effect supports after API 9");
        } else {
            if (enable) {
                _uniWebView.setOverScrollMode(View.OVER_SCROLL_ALWAYS);
            } else {
                _uniWebView.setOverScrollMode(View.OVER_SCROLL_NEVER);
            }
        }
    }

    public void setZoomEnable(boolean enable) {
        _uniWebView.getSettings().setBuiltInZoomControls(enable);
    }

    public void reload() {
        _uniWebView.reload();
    }

    public void addUrlScheme(String scheme) {
        if (!schemes.contains(scheme)) {
            schemes.add(scheme);
        }
    }

    public void removeUrlScheme(String scheme) {
        if (schemes.contains(scheme)) {
            schemes.remove(scheme);
        }
    }

    public void stop() {
        _uniWebView.stopLoading();
    }

    public void useWideViewPort(boolean use) {
        _uniWebView.getSettings().setUseWideViewPort(use);
    }

    public String getUserAgent() {
        return _currentUserAgent;
    }

    public static abstract interface DialogListener {
        public abstract void onPageFinished(UniWebViewDialog dialog, String url);
        public abstract void onPageStarted(UniWebViewDialog dialog, String url);
        public abstract void onReceivedError(UniWebViewDialog dialog, int errorCode, String description, String failingUrl);
        public abstract boolean shouldOverrideUrlLoading(UniWebViewDialog dialog, String url);
        public abstract void onDialogShouldCloseByBackButton(UniWebViewDialog dialog);
        public abstract void onDialogKeyDown(UniWebViewDialog dialog, int keyCode);
        public abstract void onDialogClose(UniWebViewDialog dialog);
        public abstract void onJavaScriptFinished(UniWebViewDialog dialog, String result);
    }

}
