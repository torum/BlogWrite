﻿using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using BlogWrite.Activation;
using BlogWrite.Contracts.Services;
using BlogWrite.Core.Contracts.Services;
using BlogWrite.Core.Services;
using BlogWrite.Helpers;
using BlogWrite.Models;
using BlogWrite.Notifications;
using BlogWrite.Services;
using BlogWrite.ViewModels;
using BlogWrite.Views;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;

namespace BlogWrite;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    private static readonly string _appName = "BlogWrite";//_resourceLoader.GetString("AppName");
    private static readonly string _appDeveloper = "torum";
    private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string AppDataFolder { get; } = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
    public static string AppConfigFilePath { get; } = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

    private static readonly Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    public static Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue => _currentDispatcherQueue;

    private static readonly ResourceLoader _resourceLoader = new();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsPage>();
            services.AddTransient<EntryDetailsViewModel>();
            services.AddTransient<EntryDetailsPage>();
            services.AddTransient<FeedAddViewModel>();
            services.AddTransient<FeedAddPage>();
            services.AddSingleton<FeedsViewModel>();
            services.AddSingleton<FeedsPage>();
            services.AddSingleton<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        Debug.Write("App_UnhandledException: " + e.Exception.ToString());
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        WinUIEx.WindowManager.PersistenceStorage = new FilePersistence(Path.Combine(AppDataFolder,"WinUIExPersistence.json"));

        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));


        await App.GetService<IActivationService>().ActivateAsync(args);
    }


    private class FilePersistence : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        private readonly string _file;

        public FilePersistence(string filename)
        {
            _file = filename;
            try
            {
                if (File.Exists(filename))
                {
                    var jo = System.Text.Json.Nodes.JsonObject.Parse(File.ReadAllText(filename)) as JsonObject;
                    foreach (var node in jo)
                    {
                        if (node.Value is JsonValue jvalue && jvalue.TryGetValue<string>(out string value))
                            _data[node.Key] = value;
                    }
                }
            }
            catch { }
        }
        private void Save()
        {
            JsonObject jo = new JsonObject();
            foreach (var item in _data)
            {
                if (item.Value is string s) // In this case we only need string support. TODO: Support other types
                    jo.Add(item.Key, s);
            }
            File.WriteAllText(_file, jo.ToJsonString());
        }
        public object this[string key] { get => _data[key]; set { _data[key] = value; Save(); } }

        public ICollection<string> Keys => _data.Keys;

        public ICollection<object> Values => _data.Values;

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _data.Add(key, value); Save();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _data.Add(item.Key, item.Value); Save();
        }

        public void Clear()
        {
            _data.Clear(); Save();
        }

        public bool Contains(KeyValuePair<string, object> item) => _data.Contains(item);

        public bool ContainsKey(string key) => _data.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException(); // TODO

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException(); // TODO

        public bool Remove(string key) => throw new NotImplementedException(); // TODO

        public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException(); // TODO

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => throw new NotImplementedException(); // TODO

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); // TODO
    }
}
