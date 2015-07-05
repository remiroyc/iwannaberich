//
//	AndroidPlugin.java
//  Created by Wang Wei(@onevcat) on 2013-11-7.
//
package com.onevcat.uniwebview;

import android.app.Activity;
import android.os.Bundle;

public class UniWebViewCustomViewActivity extends Activity
{
	public static UniWebViewCustomViewActivity customViewActivity;
	public static UniWebChromeClient currentFullScreenClient;

	@Override
	public void onCreate(Bundle savedInstanceState) {
    	super.onCreate(savedInstanceState);
    	UniWebViewCustomViewActivity.customViewActivity = this;
    	currentFullScreenClient.ToggleFullScreen(this);
	}

	@Override
	public void onBackPressed() {
        if (android.os.Build.VERSION.SDK_INT <= 11) {
            currentFullScreenClient.onHideCustomView();
        }
		this.finish();
	}
}