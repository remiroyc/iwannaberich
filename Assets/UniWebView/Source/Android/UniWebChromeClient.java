//
//	AndroidPlugin.java
//  Created by Wang Wei(@onevcat) on 2013-11-7.
//
package com.onevcat.uniwebview;

import com.unity3d.player.UnityPlayer;

import android.view.View;
import android.webkit.WebChromeClient;
import android.widget.FrameLayout;
import android.content.Intent;

public class UniWebChromeClient extends WebChromeClient {
    FrameLayout.LayoutParams LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MATCH_PARENT,
                                                                             FrameLayout.LayoutParams.MATCH_PARENT);
    private View _customView;
	private FrameLayout _uniWebViewLayout;
	private FrameLayout _customViewContainer;
	private WebChromeClient.CustomViewCallback _customViewCallback;

    public UniWebChromeClient(FrameLayout oriLayout ) {
        _uniWebViewLayout = oriLayout;
    }
    
    @Override
    public void onShowCustomView(View view, CustomViewCallback callback) {
        // if a view already exists then immediately terminate the new one
        _customView = view;
        _customViewCallback = callback;

		UniWebViewCustomViewActivity.currentFullScreenClient = this;
		Intent intent = new Intent(UnityPlayer.currentActivity, UniWebViewCustomViewActivity.class);
        UnityPlayer.currentActivity.startActivity(intent);
    }
    
    public void ToggleFullScreen(UniWebViewCustomViewActivity activity)
    {
        _customViewContainer = new FrameLayout(activity);
        _uniWebViewLayout.setVisibility(View.GONE);
        _customViewContainer.setLayoutParams(LayoutParameters);
        _customView.setLayoutParams(LayoutParameters);
        _customViewContainer.addView(_customView);
        _customViewContainer.setVisibility(View.VISIBLE);
        activity.setContentView(_customViewContainer);
    }
    
    @Override
    public void onHideCustomView() {
        if (_customView != null) {
            _customView.setVisibility(View.GONE);
            _customViewContainer.removeView(_customView);
            _customView = null;
            _customViewContainer.setVisibility(View.GONE);
            _customViewCallback.onCustomViewHidden();
            _uniWebViewLayout.setVisibility(View.VISIBLE);
        }
    }
}