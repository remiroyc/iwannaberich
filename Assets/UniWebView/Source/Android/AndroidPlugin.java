/**
 * Created by onevcat on 2013/10/20.
 * You can modify, rebuild and use this file if you purchased it.
 * But you can not redistribute it in any form.
 * Copyright and all rights reserved OneV's Den.
 */
package com.onevcat.uniwebview;

import android.app.Activity;
import android.content.Intent;
import android.content.res.Configuration;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.webkit.CookieManager;
import android.webkit.CookieSyncManager;
import android.webkit.ValueCallback;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerNativeActivity;

import java.util.ArrayList;

public class AndroidPlugin extends UnityPlayerNativeActivity
{
    public final static int FILECHOOSER_RESULTCODE = 1;
    protected static ValueCallback<Uri> _uploadMessages;
    protected static final String LOG_TAG = "UniWebView";

    public static Activity getActivity() {
        return UnityPlayer.currentActivity;
    }

    public static void setUploadMessage(ValueCallback<Uri> message) {
        _uploadMessages = message;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        CookieSyncManager.createInstance(AndroidPlugin.getActivity());
    }

    //It is a black magic in onPause and onResume
    //to make the unity view not disappear when return from background
    //Something about inactive activity which might be not reload when resume from bg.
    @Override
    protected void onPause() {
        super.onPause();
        ShowAllWebViewDialogs(false);

        CookieSyncManager manager = CookieSyncManager.getInstance();
        if (manager != null) {
            manager.stopSync();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        ShowAllWebViewDialogs(false);
        //If you are suffering a black unity scene problem when switch back to the game,
        //try to increase the number 200.
        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                ShowAllWebViewDialogs(true);
            }
        }, 200);

        CookieSyncManager manager = CookieSyncManager.getInstance();
        if (manager != null) {
            manager.startSync();
        }
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        Log.d(AndroidPlugin.LOG_TAG,"Rotation: " + newConfig.orientation);
        for (UniWebViewDialog dialog : UniWebViewManager.Instance().allDialogs()) {
            dialog.updateContentSize();
            dialog.HideSystemUI();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode,  Intent intent) {
        super.onActivityResult(requestCode, resultCode, intent);
        if(requestCode == FILECHOOSER_RESULTCODE)
        {
            if (null != _uploadMessages) {
                Uri result = intent == null || resultCode != Activity.RESULT_OK ? null : intent.getData();
                _uploadMessages.onReceiveValue(result);
                _uploadMessages = null;
            }
        }
    }

    public static void _UniWebViewInit(final String name, final int top, final int left, final int bottom, final int right) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewInit");
                UniWebViewDialog.DialogListener listener = new UniWebViewDialog.DialogListener() {
                    public void onPageFinished(UniWebViewDialog dialog, String url) {
                        Log.d(LOG_TAG, "page load finished: " + url);
                        UnityPlayer.UnitySendMessage(name, "LoadComplete", "");
                    }

                    public void onPageStarted(UniWebViewDialog dialog, String url) {
                        Log.d(LOG_TAG, "page load started: " + url);
                        UnityPlayer.UnitySendMessage(name, "LoadBegin", url);
                    }

                    public void onReceivedError(UniWebViewDialog dialog, int errorCode, String description, String failingUrl) {
                        Log.d(LOG_TAG, "page load error: " + failingUrl + " Error: " + description);
                        UnityPlayer.UnitySendMessage(name, "LoadComplete", description);
                    }

                    public boolean shouldOverrideUrlLoading(UniWebViewDialog dialog, String url) {
                        boolean shouldOverride = false;
                        if (url.startsWith("mailto:")) {
                            Intent intent = new Intent(Intent.ACTION_SENDTO, Uri.parse(url));
                            getActivity().startActivity(intent);
                            shouldOverride = true;
                        } else if (url.startsWith("tel:")) {
                            Intent intent = new Intent(Intent.ACTION_DIAL,
                                    Uri.parse(url));
                            getActivity().startActivity(intent);
                        } else {
                            boolean canResponseScheme = false;
                            for (String scheme : dialog.schemes ) {
                                if (url.startsWith(scheme + "://")) {
                                    canResponseScheme = true;
                                    break;
                                }
                            }
                            if (canResponseScheme) {
                                UnityPlayer.UnitySendMessage(name, "ReceivedMessage", url);
                                shouldOverride = true;
                            }
                        }
                        return shouldOverride;
                    }

                    public void onDialogShouldCloseByBackButton(UniWebViewDialog dialog) {
                        Log.d(LOG_TAG, "dialog should be closed");
                        UnityPlayer.UnitySendMessage(name, "WebViewDone", "");
                    }

                    public void onDialogKeyDown(UniWebViewDialog dialog, int keyCode) {
                        UnityPlayer.UnitySendMessage(name, "WebViewKeyDown", Integer.toString(keyCode));
                    }

                    public void onDialogClose(UniWebViewDialog dialog) {
                        UniWebViewManager.Instance().removeUniWebView(name);
                    }

                    public void onJavaScriptFinished(UniWebViewDialog dialog, String result) {
                        UnityPlayer.UnitySendMessage(name, "EvalJavaScriptFinished", result);
                    }
                };
                UniWebViewDialog dialog = new UniWebViewDialog(getActivity(), listener);
                dialog.changeSize(top, left, bottom, right);
                UniWebViewManager.Instance().setUniWebView(name, dialog);
            }
        });
	}

	public static void _UniWebViewLoad(final String name, final String url)
	{
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewLoad");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.load(url);
                }
            }
        });
	}

    public static void _UniWebViewReload(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewReload");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.reload();
                }
            }
        });
    }

    public static void _UniWebViewStop(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewStop");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.stop();
                }
            }
        });
    }

	public static void _UniWebViewChangeSize(final String name, final int top, final int left, final int bottom, final int right)
	{
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewChangeSize");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.changeSize(top, left, bottom, right);
                }
            }
        });
	}

    public static void _UniWebViewShow(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewShow");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setShow(true);
                }
            }
        });
    }

    public static void _UniWebViewDismiss(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewHide");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setShow(false);
                }
            }
        });
    }

	public static void _UniWebViewEvaluatingJavaScript(final String name, final String js) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewEvaluatingJavaScript");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.loadJS(js);
                }
            }
        });
	}

    public static void _UniWebViewAddJavaScript(final String name, final String js) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewAddJavaScript");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.addJs(js);
                }
            }
        });
    }

	public static void _UniWebViewCleanCache(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewCleanCache");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.cleanCache();
                }
            }
        });
	}

    public static void _UniWebViewCleanCookie(final String name, final String key) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewCleanCookie");

                CookieManager cm = CookieManager.getInstance();
                if (key == null || key.length() == 0) {
                    Log.d(LOG_TAG, "Cleaning all cookies");
                    cm.removeAllCookie();
                } else {
                    Log.d(LOG_TAG, "Setting an empty cookie for: " + key);
                    cm.setCookie(key,"");
                }

                CookieSyncManager manager = CookieSyncManager.getInstance();
                if (manager != null) {
                    manager.sync();
                }
            }
        });
    }

    public static void _UniWebViewDestroy(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewDestroy");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.destroy();
                }
            }
        });
    }

	public static void _UniWebViewTransparentBackground(final String name, final boolean transparent) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewTransparentBackground");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setTransparent(transparent);
                }
            }
        });
	}

    public static void _UniWebViewSetSpinnerShowWhenLoading(final String name, final boolean show) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewSetSpinnerShowWhenLoading: " + show);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setSpinnerShowWhenLoading(show);
                }
            }
        });
    }

    public static void _UniWebViewSetSpinnerText(final String name, final String text) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewSetSpinnerText: " + text);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setSpinnerText(text);
                }
            }
        });
    }

	public static void _UniWebViewGoBack(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewGoBack");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.goBack();
                }
            }
        });
	}

	public static void _UniWebViewGoForward(final String name) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewGoForward");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.goForward();
                }
            }
        });
	}

    public static void _UniWebViewLoadHTMLString(final String name, final String htmlString, final String baseURL) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewLoadHTMLString");
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.loadHTMLString(htmlString, baseURL);
                }
            }
        });
    }

    public static String _UniWebViewGetCurrentUrl(final String name) {
        UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
        if (dialog != null) {
            return dialog.getUrl();
        }
        return "";
    }

    public static void _UniWebViewSetBackButtonEnable(final String name, final boolean enable) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewSetBackButtonEnable:" + enable);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setBackButtonEnable(enable);
                }
            }
        });
    }

    public static void _UniWebViewSetBounces(final String name, final boolean enable) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewSetBounces:" + enable);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setBounces(enable);
                }
            }
        });
    }

    public static void _UniWebViewSetZoomEnable(final String name, final boolean enable) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewSetZoomEnable:" + enable);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.setZoomEnable(enable);
                }
            }
        });
    }

    public static void _UniWebViewAddUrlScheme(final String name, final String scheme) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewAddUrlScheme:" + scheme);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.addUrlScheme(scheme);
                }
            }
        });
    }

    public static void _UniWebViewRemoveUrlScheme(final String name, final String scheme) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewAddUrlScheme:" + scheme);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.removeUrlScheme(scheme);
                }
            }
        });
    }

    public static void _UniWebViewUseWideViewPort(final String name, final boolean use) {
        runSafelyOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d(LOG_TAG, "_UniWebViewUseWideViewPort:" + use);
                UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
                if (dialog != null) {
                    dialog.useWideViewPort(use);
                }
            }
        });
    }

    public static void _UniWebViewSetUserAgent(final String userAgent) {
        UniWebView.customUserAgent = userAgent;
    }

    public static String _UniWebViewGetUserAgent(final String name) {
        UniWebViewDialog dialog = UniWebViewManager.Instance().getUniWebViewDialog(name);
        if (dialog != null) {
            return dialog.getUserAgent();
        }
        return "";
    }

    protected static void runSafelyOnUiThread(final Runnable r) {
        getActivity().runOnUiThread(new Runnable() {
            public void run() {
                try {
                    r.run();
                } catch (Exception e) {
                    Log.d(LOG_TAG, "UniWebView should run on UI thread: " + e.getMessage());
                }
            }
        });
    }

    protected void ShowAllWebViewDialogs(boolean show) {
        ArrayList<UniWebViewDialog> webViewDialogs = UniWebViewManager.Instance().getShowingWebViewDialogs();
        for (UniWebViewDialog webViewDialog : webViewDialogs) {
            if (show) {
                Log.d(LOG_TAG, webViewDialog + "goForeGround");
                webViewDialog.goForeGround();
                webViewDialog.HideSystemUI();
            } else {
                Log.d(LOG_TAG, webViewDialog + "goBackGround");
                webViewDialog.goBackGround();
                webViewDialog.HideSystemUI();
            }
        }
    }
}