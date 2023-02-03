﻿using System.Diagnostics.CodeAnalysis;

using FeedDesk.Contracts.Services;

using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace FeedDesk.Services;

public class WebViewService : IWebViewService
{
    private WebView2? _webView;

    public Uri? Source => _webView?.Source;


    [MemberNotNullWhen(true, nameof(_webView))]
    public void NavigateToString(string str)
    {
        _webView?.NavigateToString(str);
    }


    [MemberNotNullWhen(true, nameof(_webView))]
    public bool CanGoBack => _webView != null && _webView.CanGoBack;

    [MemberNotNullWhen(true, nameof(_webView))]
    public bool CanGoForward => _webView != null && _webView.CanGoForward;

    public event EventHandler<CoreWebView2WebErrorStatus>? NavigationCompleted;
    public event EventHandler<CoreWebView2InitializedEventArgs>? CoreWebView2Initialized;



    public WebViewService()
    {
    }

    [MemberNotNull(nameof(_webView))]
    public async void Initialize(WebView2 webView)
    {
        _webView = webView;

        // Test
        await _webView.EnsureCoreWebView2Async();
        // _webView.EnsureCoreWebView2Async();
        _webView.NavigationCompleted += OnWebViewNavigationCompleted;
        _webView.CoreWebView2Initialized += OnCoreWebView2Initialized;
    }

    public void GoBack() => _webView?.GoBack();

    public void GoForward() => _webView?.GoForward();

    public void Reload() => _webView?.Reload();

    public void UnregisterEvents()
    {
        if (_webView != null)
        {
            _webView.NavigationCompleted -= OnWebViewNavigationCompleted;
        }
    }

    private void OnWebViewNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args) => NavigationCompleted?.Invoke(this, args.WebErrorStatus);


    private void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args) => CoreWebView2Initialized?.Invoke(sender, args);

}