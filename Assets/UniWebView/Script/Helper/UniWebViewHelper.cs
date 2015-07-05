using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Supply some helper utility method for UniWebView
/// </summary>
public class UniWebViewHelper{
	/// <summary>
	/// Is we are running the on retina iOS device.
	/// </summary>
	/// <returns><c>true</c>, if running on retina retina iOS device <c>false</c> otherwise.</returns>
	/// <description>
	/// This method is depreated in version 1.7.0. Use UniWebViewHelper.screenHeight and UniWebViewHelper.screenWidth to get screen size in point directly.
	/// </description>
	[Obsolete("RunningOnRetinaIOS() is deprecated in ver1.7.0. Use ScreenHeight() and ScreenWidth() and set inset based on that instead.",true)]
	public static bool RunningOnRetinaIOS() {
		#if UNITY_IPHONE
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return (iPhone.generation != iPhoneGeneration.iPad1Gen &&
			        iPhone.generation != iPhoneGeneration.iPad2Gen &&
			        iPhone.generation != iPhoneGeneration.iPadMini1Gen &&
			        iPhone.generation != iPhoneGeneration.iPhone &&
			        iPhone.generation != iPhoneGeneration.iPhone3G &&
			        iPhone.generation != iPhoneGeneration.iPhone3GS &&
			        iPhone.generation != iPhoneGeneration.iPodTouch1Gen &&
			        iPhone.generation != iPhoneGeneration.iPodTouch2Gen &&
			        iPhone.generation != iPhoneGeneration.iPodTouch3Gen);
		}
		#endif
		return false;
	}

	/// <summary>
	/// Get the height of the screen.
	/// </summary>
	/// <value>
	/// The height of screen.
	/// </value>
	/// <description>
	/// In iOS devices, it will always return the screen height in "point", 
	/// instead of "pixel". It would be useful to use this value to calculate webview size.
	/// On other platforms, it will just return Unity's Screen.height.
	/// For example, a portrait iPhone 5 will return 568 and a landscape one 320. You should 
	/// always use this value to do screen-size-based insets calculation.
	/// </description>
	public static int screenHeight {
		get {
			#if UNITY_IOS && !UNITY_EDITOR
			return UniWebViewPlugin.ScreenHeight();
			#endif
			return Screen.height;
		}
	}

	/// <summary>
	/// Get the height of the screen.
	/// </summary>
	/// <value>
	/// The height of screen.
	/// </value>
	/// <description>
	/// In iOS devices, it will always return the screen width in "point", 
	/// instead of "pixel". It would be useful to use this value to calculate webview size.
	/// On other platforms, it will just return Unity's Screen.height.
	/// For example, a portrait iPhone 5 will return 320 and a landscape one 568. You should 
	/// always use this value to do screen-size-based insets calculation.
	/// </description>
	public static int screenWidth {
		get {
			#if UNITY_IOS && !UNITY_EDITOR
			return UniWebViewPlugin.ScreenWidth();
			#endif
			return Screen.width;
		}
	}
}
