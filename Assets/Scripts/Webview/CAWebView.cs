using UnityEngine;
using System.Collections;

public class CAWebView : MonoBehaviour
{

    private UniWebView _webView;

    void Start()
    {
        _webView = GetComponent<UniWebView>();
        if (_webView == null)
        {
            _webView = gameObject.AddComponent<UniWebView>();
            _webView.OnReceivedMessage += OnReceivedMessage;
            _webView.OnLoadComplete += OnLoadComplete;
            _webView.OnWebViewShouldClose += OnWebViewShouldClose;
            //_webView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;
            //_webView.InsetsForScreenOreitation += InsetsForScreenOreitation;
        }
    }

    private void OnLoadComplete(UniWebView webView, bool success, string errorMessage)
    {
        if (success)
        {
            webView.Show();
        }
        else
        {
            Debug.Log("Something wrong in webview loading: " + errorMessage);
            // _errorMessage = errorMessage;
        }
    }

    private void OnReceivedMessage(UniWebView webView, UniWebViewMessage message)
    {
        Debug.Log(message.rawMessage);
    }

    private bool OnWebViewShouldClose(UniWebView webView)
    {
        if (webView == _webView)
        {
            _webView = null;
            return true;
        }
        return false;
    }

    void Update()
    {

    }
}
