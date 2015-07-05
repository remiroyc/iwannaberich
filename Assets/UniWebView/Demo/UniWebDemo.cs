//
//	UniWebDemo.cs
//  Created by Wang Wei(@onevcat) on 2013-10-20.
//
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is a demo script to show how to use UniWebView.
/// You can follow the step 1 to 10 and get started with the basic use of UniWebView.
/// </summary>
public class UniWebDemo : MonoBehaviour {
	
	public GameObject cubePrefab;
	public TextMesh tipTextMesh;

//Just let it compile on platforms beside of iOS and Android
//If you are just targeting for iOS and Android, you can ignore this
#if UNITY_IOS || UNITY_ANDROID

	//1. First of all, we need a reference to hold an instance of UniWebView
	private UniWebView _webView;

	private string _errorMessage;
	private GameObject _cube;
	private Vector3 _moveVector;

	void Start() {
		_cube = GameObject.Instantiate(cubePrefab) as GameObject;
		_cube.GetComponent<UniWebViewCube>().webViewDemo = this;
		_moveVector = Vector3.zero;
	}

	void Update() {
		if (_cube != null) {
			if (_cube.transform.position.y < -5) {
				Destroy(_cube);
				_cube = null;
			} else {
				_cube.transform.Translate(_moveVector * Time.deltaTime);
			}
		}
	}

	void OnGUI() {
		if (GUI.Button(new Rect(0, Screen.height - 150, 150, 80),"Open")) {
			//2. You can add a UniWebView either in Unity Editor or by code.
			//Here we check if there is already a UniWebView component. If not, add one.
			_webView = GetComponent<UniWebView>();
			if (_webView == null) {
				_webView = gameObject.AddComponent<UniWebView>();
				_webView.OnReceivedMessage += OnReceivedMessage;
				_webView.OnLoadComplete += OnLoadComplete;
				_webView.OnWebViewShouldClose += OnWebViewShouldClose;
				_webView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;

				_webView.InsetsForScreenOreitation += InsetsForScreenOreitation;
			}

			//3. You can set the insets of this webview by assigning an insets value simply
			//   like this:
			/*
					int bottomInset = (int)(UniWebViewHelper.screenHeight * 0.5f);
					_webView.insets = new UniWebViewEdgeInsets(5,5,bottomInset,5);
			*/

			// Or you can also use the `InsetsForScreenOreitation` delegate to specify different
			// insets for portrait or landscape screen. If your webpage should resize on both portrait
			// and landscape, please use the delegate way. See the `InsetsForScreenOreitation` method
			// in this file for more.

			// Now, set the url you want to load.
			_webView.url = "http://uniwebview.onevcat.com/demo/index1-1.html";

			//You can read a local html file, by putting the file into /Assets/StreamingAssets folder
			//And use the url like these
			//If you are using "Split Application Binary" for Android, see the FAQ section of manual for more.
			/*
			#if UNITY_EDITOR
			_webView.url = Application.streamingAssetsPath + "/index.html";
			#elif UNITY_IOS
			_webView.url = Application.streamingAssetsPath + "/index.html";
			#elif UNITY_ANDROID
			_webView.url = "file:///android_asset/index.html";
			#endif
			*/

			// You can set the spinner visibility and text of the webview.
			// This line can change the text of spinner to "Wait..." (default is  "Loading...")
			//_webView.SetSpinnerLabelText("Wait...");
			// This line will tell UniWebView to not show the spinner as well as the text when loading.
			//_webView.SetShowSpinnerWhenLoading(false);

			//4.Now, you can load the webview and waiting for OnLoadComplete event now.
			_webView.Load();

			_errorMessage = null;

			//You can also load some HTML string instead from a url or local file.
			//When loading from the HTML string, the _webView.url will take no effect.
			//_webView.LoadHTMLString("<body>I am a html string</body>",null);

			//If you want the webview show immediately, instead of the OnLoadComplete event, call Show()
			//A blank webview will appear first, then load the web page content in it
			//_webView.Show();
		}

		if (_webView != null && GUI.Button(new Rect(150,
		                                            Screen.height - 150,
		                                            150,
		                                            80),"Back")) {
			_webView.GoBack();
		}

		//Tool bar is hidden by default. You can use it to navigate or close the webview.
		//The tool bar is only avaliable in iOS system. You can use "back button" of Android to go back.
		if (_webView != null && GUI.Button(new Rect(300,
		                                            Screen.height - 150,
		                                            150,
		                                            80),"ToolBar")) {
			if (_webView.toolBarShow) {
				_webView.HideToolBar(true);
			} else {
				_webView.ShowToolBar(true);
			}
		}

		if (_errorMessage != null) {
			GUI.Label(new Rect(0, 0, Screen.width, 80), _errorMessage);
		}
	}

	//5. When the webView complete loading the url sucessfully, you can show it.
	//   You can also set the autoShowWhenLoadComplete of UniWebView to show it automatically when it loads finished.
	void OnLoadComplete(UniWebView webView, bool success, string errorMessage) {
		if (success) {
			webView.Show();
		} else {
			Debug.Log("Something wrong in webview loading: " + errorMessage);
			_errorMessage = errorMessage;
		}
	}

	//6. The webview can talk to Unity by a url with scheme of "uniwebview". See the webpage for more
	//   Every time a url with this scheme clicked, OnReceivedMessage of webview event get raised.
	void OnReceivedMessage(UniWebView webView, UniWebViewMessage message) {
		Debug.Log(message.rawMessage);
		//7. You can get the information out from the url path and query in the UniWebViewMessage
		//For example, a url of "uniwebview://move?direction=up&distance=1" in the web page will 
		//be parsed to a UniWebViewMessage object with:
		//				message.scheme => "uniwebview"
		//              message.path => "move"
		//              message.args["direction"] => "up"
		//              message.args["distance"] => "1"
		// "uniwebview" scheme is sending message to Unity by default.
		// If you want to use your customized url schemes and make them sending message to UniWebView,
		// use webView.AddUrlScheme("your_scheme") and webView.RemoveUrlScheme("your_scheme")
		if (string.Equals(message.path,"move")) {
			Vector3 direction = Vector3.zero;
			if (string.Equals(message.args["direction"],"up")) {
				direction = new Vector3(0,0,1);
			} else if (string.Equals(message.args["direction"],"down")) {
				direction = new Vector3(0,0,-1);
			} else if (string.Equals(message.args["direction"],"left")) {
				direction = new Vector3(-1,0,0);
			} else if (string.Equals(message.args["direction"],"right")) {
				direction = new Vector3(1,0,0);
			}

			int distance = 0;
			if (int.TryParse(message.args["distance"], out distance)) {
				direction *= distance;
			}

			_moveVector = direction;

		} else if (string.Equals(message.path, "add")) {
			if (_cube != null) {
				Destroy(_cube);
			}
			_cube = GameObject.Instantiate(cubePrefab) as GameObject;
			_cube.GetComponent<UniWebViewCube>().webViewDemo = this;
			_moveVector = Vector3.zero;
		} else if (string.Equals(message.path, "close")) {
			//8. When you done your work with the webview, 
			//you can hide it, destory it and do some clean work.
			webView.Hide();
			Destroy(webView);
			webView.OnReceivedMessage -= OnReceivedMessage;
			webView.OnLoadComplete -= OnLoadComplete;
			webView.OnWebViewShouldClose -= OnWebViewShouldClose;
			webView.OnEvalJavaScriptFinished -= OnEvalJavaScriptFinished;
			webView.InsetsForScreenOreitation -= InsetsForScreenOreitation;
			_webView = null;
		}
	}

	//9. By using EvaluatingJavaScript method, you can talk to webview from Unity.
	//It can evel a javascript or run a js method in the web page.
	//(In the demo, it will be called when the cube hits the sphere)
	public void ShowAlertInWebview(float time, bool first) {
		_moveVector = Vector3.zero;
		if (first) {
			//Eval the js and wait for the OnEvalJavaScriptFinished event to be raised.
			//The sample(float time) is written in the js in webpage, in which we pop 
			//up an alert and return a demo string.
			//When the js excute finished, OnEvalJavaScriptFinished will be raised.
			_webView.EvaluatingJavaScript("sample(" + time +")");
		}
	}

	//In this demo, we set the text to the return value from js.
	void OnEvalJavaScriptFinished(UniWebView webView, string result) {
		Debug.Log("js result: " + result);
		tipTextMesh.text = "<color=#000000>" + result + "</color>";
	}

	//10. If the user close the webview by tap back button (Android) or toolbar Done button (iOS), 
	//    we should set your reference to null to release it. 
	//    Then we can return true here to tell the webview to dismiss.
	bool OnWebViewShouldClose(UniWebView webView) {
		if (webView == _webView) {
			_webView = null;
			return true;
		}
		return false;
	}

	// This method will be called when the screen orientation changed. Here we returned UniWebViewEdgeInsets(5,5,bottomInset,5)
	// for both situation. Although they seem to be the same, screenHeight was changed, leading a difference between the result.
	// eg. on iPhone 5, bottomInset is 284 (568 * 0.5) in portrait mode while it is 160 (320 * 0.5) in landscape.
	UniWebViewEdgeInsets InsetsForScreenOreitation(UniWebView webView, UniWebViewOrientation orientation) {
		int bottomInset = (int)(UniWebViewHelper.screenHeight * 0.5f);

		if (orientation == UniWebViewOrientation.Portrait) {
			return new UniWebViewEdgeInsets(5,5,bottomInset,5);
		} else {
			return new UniWebViewEdgeInsets(5,5,bottomInset,5);
		}
	}
#endif //End of #if UNITY_IOS || UNITY_ANDROID
}
