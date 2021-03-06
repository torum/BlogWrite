﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Media;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using BlogWrite.Common;
using BlogWrite.Models;
using BlogWrite.Models.Clients;
using Microsoft.Data.Sqlite;
using System.Threading;

namespace BlogWrite.ViewModels
{
    /// TODO: 
    /// 
    /// [Service Auto Discovery]
    /// xmlrpc.php直叩きした時の判定。
    /// 
    /// [CRUD Services]
    /// InfoWindowでService情報も見れるようにする。
    /// ...
    /// 
    /// [Editor]
    /// ...
    /// 
    /// [General]
    /// i18n
    /// App Icon / App name .... FeedDesk?
    /// 
    /// [Feed Reader]
    /// Options (text configuration)
    /// Star/Pin/ReadItLator
    /// Wide layout - three vertical columns
    /// Search
    /// trap Open in new window event in WebView2 and redirect it in default browser.
    /// FavIcon
    /// Copy the URL to clipboard
    /// InfoWindowで、画像の取得と表示、をオフにできるようにする。
    /// 一定数以上または一定期間（１ヵ月）過ぎた古いFeedEntryはApp終了時かイニシャライズ時に削除する。
    /// InfoWindowで、個別にFeedの更新頻度を設定できるようにする。
    /// 
    /// [Settings window]
    /// 作る
    /// 

    /// Change History：
    /// v0.0.0.49 とりあえず、AtomPubのServiceDocumentとXML-RPCのRSDからの登録と保存・復帰。
    /// v0.0.0.48 表示する文字数とかフォントファミリーを少し弄った。
    /// v0.0.0.47 全画面InAppBrowserの方で、今開いているページをデフォブラウザで開く、ようにした。
    /// v0.0.0.46 3PaneモードIn app browserでの閉じるボタンを辞めた。全画面InAppBrowserの方はデフォブラウザで開くを辞めた。非選択アイテムでは開けないから。
    /// v0.0.0.45 D&Dしたら、フォルダの未読数を計算し直し。更新中はD&D出来ないように。
    /// v0.0.0.44 Download, Insert, Load and Display "Eye Catching" Image.
    /// v0.0.0.43 CommonStatus を追加して、IsArchivedだったら、Archiveボタンを非表示にするようにした。MagazineViewでタイトルの文字サイズを読みやすくした。
    /// v0.0.0.42 空のFolderの削除が出来て無かった。DeleteのLockも。
    /// v0.0.0.41 AutoDiscoveryのHTMLからRSDパース。登録はまだ。
    /// v0.0.0.40 SQLite へのDB accessが、すべてAsync化されて軽くなった。
    /// v0.0.0.39 ArchiveAll 作り直し。Database access を Asyncで With Lock.
    /// v0.0.0.38 Folderまとめ読みは、SQL一発で。 LIMIT 100
    /// v0.0.0.37 3paneのcontentpreviewbrowserの初期化が出来て無かった。
    /// v0.0.0.36 Magazine View styleの追加。View形式をFeedやサービスごとに覚えた。
    /// v0.0.0.35 Feed Folderまとめ表示を改善し、DebugTextがある場合はアイコンを黄色にするようにした。
    /// v0.0.0.34 Feed Folderまとめ表示。
    /// v0.0.0.33 OPML import and export.
    /// v0.0.0.32 NodeFeedをDeleteした時に、DBから記事を削除。AtomFeedNode/RssFeedNode, AtomFeedClient/RssFeedClientをまとめて一つにした。
    /// v0.0.0.31 DebugWrindowに書き出す内容をスリムにして、必要な通知はちゃんと出すようにした。古いAtomとかUriやDatetimeのFormatErrorとか。TreeViewのNodeFeedからArchive。
    /// v0.0.0.30 TabBottomのスタイル作り直し。
    /// v0.0.0.29 全更新の時刻判定が逆だった。datetime pase no exception handling.
    /// v0.0.0.28 Author情報の取得。DBのFlagをIsReadに変更し、AuthorとStatusの保存と表示。Statusの更新はまだ。
    /// v0.0.0.27 FeedDupeCheck fixed. SiteUrl再チェック。
    /// v0.0.0.26 DBエラーとHTTPエラーを分けた。とりあえず、Feed 既読管理（既読・未読Flag設定、未読のみ切り替え表示、未読件数表示）。
    /// v0.0.0.25 取得と保存と表示の流れで、ベースの形とエラー取り回し。
    /// v0.0.0.24 とりあえず、SQLiteの初期化からInsertとSelectまで。
    /// v0.0.0.23 とりあえず、古いAtom0.3のFeed読み込みにも対応した。Atom0.3 Apiの方は対応予定無し。
    /// v0.0.0.22 Renameing and Restructure of files and folder names.
    /// v0.0.0.21 とりあえず、AtomPubのServiceDocument解析とNodeの追加まで。カテゴリとか、保存時に消えてる可能性あり。
    /// v0.0.0.20 AutoDiscoveryで、Auth対応。認証情報入力ページを実装。AtomPub判定まで。
    /// v0.0.0.19 "Description" が存在してないRSSの場合、ContentTypeが設定されずHTMLと判定されず、ブラウザがVisibleにならないため In-app-browserで表示できてなかった。手動で表に表示するようにした。
    /// v0.0.0.18 gets and display Entry's Image in CardView.
    /// v0.0.0.17 Reset scroll position when Entries updated.
    /// v0.0.0.16 NodeFeedのParent設定洩れ。Feed Folderまとめ表示はSQLite化してからDBから読み込むようにする。
    /// v0.0.0.15 FeedのSiteUriとSiteTitleを取得と保存するようにした。情報を見るメニュとInfoWindowを作成。
    /// v0.0.0.14 WebView2のSmooth Scrollig をOffにする方法を見つけた。
    /// v0.0.0.13 WebView2のinstallationとversionを確認してダイアログを出すようにした。
    /// v0.0.0.12 WebView2の環境設定をするようにした。＞Binフォルダ内ではなく、Tempフォルダにブラウザデータを展開。
    /// v0.0.0.11 In App Browserを、CardViewとListViewで分けた。
    /// v0.0.0.10 とりあえず、カード表示を実装。タイトル、要約、日付の表示。
    /// v0.0.0.9 とりあえず、MainMenuのスタイル。Listviewのソート。
    /// v0.0.0.8 Listview と Cardview の切り替えTabControlを付けた。BrowserとDebugWindow の表示切替を実装。 Browserが非表示の際はデフォルトのブラウザを立ち上げる。
    /// v0.0.0.7 TreeviewItem's In-Place Renaming and Right Click Select.
    /// v0.0.0.6 とりあえず、SearviceDiscoveryでのRSS/Atomのパースと直登録。RSSのfeed の自前パース（Atomは既に済み）。
    /// v0.0.0.5 TreeViewのD&Dと、feed登録と更新時のエラーハンドリング改善。
    /// v0.0.0.4 とりあえず、TreeViewのD&D（Folder内に入れるのとInsertBefore）実装。
    /// v0.0.0.3 とりあえず、HTML取得、解析、RSS/AtomのFeed検出、登録、表示までの流れは出来た。
    /// v0.0.0.2 色々。
    /// v0.0.0.1 3年前の作りかけの状態を少なくとも最新の環境にあわせてアップデート。


    public class MainViewModel : ViewModelBase
    {
        // Application name
        const string _appName = "BlogWrite";

        // Application version
        const string _appVer = "0.0.0.49";
        public string AppVer
        {
            get
            {
                return _appVer;
            }
        }

        // Application config file folder
        const string _appDeveloper = "torum";

        // Application Window Title
        public string AppTitle
        {
            get
            {
                return _appName + " " + _appVer;
            }
        }

        #region == Properties ==

        #region == App Options ==

        private bool _isSaveLog;
        public bool IsSaveLog
        {
            get { return _isSaveLog; }
            set
            {
                if (_isSaveLog == value)
                    return;

                _isSaveLog = value;

                NotifyPropertyChanged(nameof(IsSaveLog));
            }
        }

        private bool _isDebugWindowEnabled;
        public bool IsDebugWindowEnabled

        {
            get { return _isDebugWindowEnabled; }
            set
            {
                if (_isDebugWindowEnabled == value)
                    return;

                _isDebugWindowEnabled = value;

                NotifyPropertyChanged(nameof(IsDebugWindowEnabled));
                /*
                if (_isDebugWindowEnabled)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //DebugWindowShowHide?.Invoke
                        DebugWindowShowHide2?.Invoke(this, true);
                    });
                }
                else
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //DebugWindowShowHide?.Invoke();
                        DebugWindowShowHide2?.Invoke(this, false);
                    });
                }
                */
            }
        }

        #endregion

        #region == Service Treeview ==

        private ServiceTreeBuilder _services = new ServiceTreeBuilder();
        public ObservableCollection<NodeTree> Services
        {
            get { return _services.Children; }
            set
            {
                _services.Children = value;
                NotifyPropertyChanged(nameof(Services));
            }
        }

        private NodeTree _selectedNode = new NodeService("", "", "", new Uri("http://127.0.0.1"), ApiTypes.atUnknown, ServiceTypes.Unknown);
        public NodeTree SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode == value)
                    return;

                _selectedNode = value;

                NotifyPropertyChanged(nameof(SelectedNode));

                // Clear Listview selected Item.
                SelectedItem = null;

                // Clear HTTP error if shown.
                HttpError = null;
                IsShowHttpClientErrorMessage = false;

                // Clear DB error if shown.
                DatabaseError = null;
                IsShowDatabaseErrorMessage = false;

                if (_selectedNode == null)
                    return;

                // Update Title bar info
                SelectedServiceName = _selectedNode.Name;

                // Reset visibility flags for buttons etc
                IsShowInFeedAndFolder = false;
                IsShowInFeed = false;

                if (_selectedNode.ViewType == ViewTypes.vtCards)
                    SelectedViewTabIndex = 0;
                else if (_selectedNode.ViewType == ViewTypes.vtMagazine)
                    SelectedViewTabIndex = 1;
                else if (_selectedNode.ViewType == ViewTypes.vtThreePanes)
                    SelectedViewTabIndex = 2;

                if (_selectedNode is NodeService)
                {
                    // Show HTTP Error if assigned.
                    if ((_selectedNode as NodeService).ErrorHttp != null)
                    {
                        HttpError = (_selectedNode as NodeService).ErrorHttp;
                        IsShowHttpClientErrorMessage = true;
                    }

                    // Show DB Error if assigned.
                    if ((_selectedNode as NodeService).ErrorDatabase != null)
                    {
                        DatabaseError = (_selectedNode as NodeService).ErrorDatabase;
                        IsShowDatabaseErrorMessage = true;
                    }

                    // NodeFeed is selected
                    if (_selectedNode is NodeFeed)
                    {
                        // Reset view...
                        (SelectedNode as NodeFeed).IsDisplayUnarchivedOnly = true;

                        if ((SelectedNode as NodeFeed).IsDisplayUnarchivedOnly)
                            _selectedComboBoxItemIndex = 0;
                        else
                            _selectedComboBoxItemIndex = 1;

                        // "Silent" update
                        NotifyPropertyChanged(nameof(SelectedComboBoxItemIndex));

                        IsShowInFeedAndFolder = true;
                        IsShowInFeed = true;
                    }
                    else
                    {
                        // TODO: 
                    }
                }
                else if (_selectedNode is NodeFolder)
                {
                    IsShowInFeedAndFolder = true;
                    IsShowInFeed = false;
                }

                Entries.Clear();

                Task nowait = Task.Run(() => LoadEntriesAsync(_selectedNode));
                //LoadEntries(_selectedNode);
            }
        }

        private string _selectedServiceName;
        public string SelectedServiceName
        {
            get { return _selectedServiceName; }
            set
            {
                if (_selectedServiceName == value)
                    return;

                _selectedServiceName = value;

                NotifyPropertyChanged(nameof(SelectedServiceName));
            }
        }

        #endregion

        #region == Entry ListViews ==

        private ObservableCollection<EntryItem> _entries = new ObservableCollection<EntryItem>();
        public ObservableCollection<EntryItem> Entries
        {
            get { return _entries; }
            set
            {
                if (_entries == value)
                    return;

                _entries = value;
                NotifyPropertyChanged(nameof(Entries));
            }
        }

        private EntryItem _selectedItem = null;
        public EntryItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;

                _selectedItem = value;
                NotifyPropertyChanged(nameof(SelectedItem));

                if (_selectedItem == null)
                {
                    WriteHtmlToContentPreviewBrowser?.Invoke(this, "");

                    NotifyPropertyChanged(nameof(EntryContentText));

                    return;
                }

                if (IsContentHTML)
                {
                    // This updates the view contents.
                    NotifyPropertyChanged(nameof(EntryContentHTML));

                    // Bring the browser to front.
                    IsContentBrowserVisible = true;

                    string s = EntryContentHTML;
                    WriteHtmlToContentPreviewBrowser?.Invoke(this, s);
                }
                else// if (IsContentText)
                {
                    NotifyPropertyChanged(nameof(EntryContentText));

                    IsContentBrowserVisible = false;
                }

                //NotifyPropertyChanged(nameof(Entries));
            }
        }

        private int _selectedViewTabIndex = 0;
        public int SelectedViewTabIndex
        {
            get { return _selectedViewTabIndex; }
            set
            {
                if (_selectedViewTabIndex == value)
                    return;

                _selectedViewTabIndex = value;
                NotifyPropertyChanged(nameof(SelectedViewTabIndex));

                if (SelectedNode is not null)
                {
                    if (_selectedViewTabIndex == 0)
                        SelectedNode.ViewType = ViewTypes.vtCards;
                    else if (_selectedViewTabIndex == 1)
                        SelectedNode.ViewType = ViewTypes.vtMagazine;
                    else if (_selectedViewTabIndex == 2)
                        SelectedNode.ViewType = ViewTypes.vtThreePanes;
                }
            }
        }

        public bool IsContentText
        {
            get
            {
                if (_selectedItem == null)
                    return false;

                if (!(_selectedItem is EntryItem))
                    return false;

                if ((_selectedItem as EntryItem).ContentType == EntryItem.ContentTypes.text)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsContentHTML
        {
            get
            {
                if (_selectedItem == null)
                    return false;

                if (!(_selectedItem is EntryItem))
                    return false;

                if ((_selectedItem as EntryItem).ContentType == EntryItem.ContentTypes.textHtml)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool _isContentBrowserVisible;
        public bool IsContentBrowserVisible
        {
            get { return _isContentBrowserVisible; }
            set
            {
                if (_isContentBrowserVisible == value)
                    return;

                _isContentBrowserVisible = value;
                NotifyPropertyChanged(nameof(IsContentBrowserVisible));
            }
        }

        private int _selectedComboBoxItemIndex;
        public int SelectedComboBoxItemIndex
        {
            get { return _selectedComboBoxItemIndex; }
            set
            {
                if (_selectedComboBoxItemIndex == value)
                    return;

                _selectedComboBoxItemIndex = value;
                NotifyPropertyChanged(nameof(SelectedComboBoxItemIndex));

                if (SelectedNode == null)
                    return;

                if (SelectedNode is NodeFeed)
                {
                    if (_selectedComboBoxItemIndex == 0)
                        (SelectedNode as NodeFeed).IsDisplayUnarchivedOnly = true;
                    else
                        (SelectedNode as NodeFeed).IsDisplayUnarchivedOnly = false;

                    Task nowait = Task.Run(() => LoadEntriesAsync(SelectedNode as NodeFeed));
                    //LoadEntries(SelectedNode as NodeFeed);
                }
            }
        }

        #endregion

        #region == Content View (TODO) ==

        public string EntryContentText
        {
            get
            {
                if (_selectedItem == null)
                    return "";

                if (_selectedItem is EntryItem)
                {
                    if (!string.IsNullOrEmpty((_selectedItem as EntryItem).Content))
                    {
                        return (_selectedItem as EntryItem).Content;
                    }
                }

                return "";
            }
        }

        public string EntryContentHTML
        {
            get
            {
                if (_selectedItem == null)
                    return WrapHtmlContent("");

                if (_selectedItem is EntryItem)
                {
                    if ((_selectedItem as EntryItem).ContentType == EntryFull.ContentTypes.textHtml)
                    {
                        return WrapHtmlContent((_selectedItem as EntryItem).Content);
                    }
                    else
                    {
                        return WrapHtmlContent("");
                    }
                }
                else
                {
                    return WrapHtmlContent("");
                }
            }
        }

        private static string WrapHtmlContent(string source, string styles = null)
        {
            if (styles == null)
            {
                styles = @"
::-webkit-scrollbar { width: 17px; height: 3px;}
::-webkit-scrollbar-button {  background-color: #666; }
::-webkit-scrollbar-track {  background-color: #646464; box-shadow: 0 0 4px #aaa inset;}
::-webkit-scrollbar-track-piece { background-color: #212121;}
::-webkit-scrollbar-thumb { height: 50px; background-color: #666;}
::-webkit-scrollbar-corner { background-color: #646464;}}
::-webkit-resizer { background-color: #666;}

body {
	
	line-height: 1.75em;
	font-size: 12px;
	background-color: #222;
	color: #aaa;
}

p {
	font-size: 12px;
}

h1 {
	font-size: 30px;
	line-height: 34px;
}

h2 {
	font-size: 20px;
	line-height: 25px;
}

h3 {
	font-size: 16px;
	line-height: 27px;
	padding-top: 15px;
	padding-bottom: 15px;
	border-bottom: 1px solid #D8D8D8;
	border-top: 1px solid #D8D8D8;
}

hr {
	height: 1px;
	background-color: #d8d8d8;
	border: none;
	width: 100%;
	margin: 0px;
}

a[href] {
	color: #1e8ad6;
}

a[href]:hover {
	color: #3ba0e6;
}

img {
    width: 160;
    height: auto;
    float: left;
    margin: 6px 12px 12px 6px;
}

li {
	line-height: 1.5em;
}
                ";
            }

            return String.Format(
                @"<html>
                    <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />

                        <!-- saved from url=(0014)about:internet -->

                        <style type='text/css'>
                            body {{ font: 10pt verdana; color: #101010; background: #cccccc; }}
                            table, td, th, tr {{ border: 1px solid black; border-collapse: collapse; }}
                        </style>

                        <!-- Custom style sheet -->
                        <style type='text/css'>{1}</style>
                    </head>
                    <body>{0}</body>
                </html>",
                source, styles);
        }

        #endregion

        #region == Status flags ==

        private bool _isFullyLoaded;
        public bool IsFullyLoaded
        {
            get
            {
                return _isFullyLoaded;
            }
            set
            {
                if (_isFullyLoaded == value)
                    return;

                _isFullyLoaded = value;
                this.NotifyPropertyChanged(nameof(IsFullyLoaded));
            }
        }
        
        private bool _isBusy;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                NotifyPropertyChanged(nameof(IsBusy));

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get
            {
                return _isWorking;
            }
            set
            {
                if (_isWorking == value)
                    return;

                _isWorking = value;
                NotifyPropertyChanged(nameof(IsWorking));

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
        }

        #endregion

        #region == Visivility flags == 

        private bool _isShowContentBrowserWindow;
        public bool IsShowContentBrowserWindow

        {
            get { return _isShowContentBrowserWindow; }
            set
            {
                if (_isShowContentBrowserWindow == value)
                    return;

                _isShowContentBrowserWindow = value;

                NotifyPropertyChanged(nameof(IsShowContentBrowserWindow));
            }
        }

        private bool _isShowInFeedAndFolder;
        public bool IsShowInFeedAndFolder

        {
            get { return _isShowInFeedAndFolder; }
            set
            {
                if (_isShowInFeedAndFolder == value)
                    return;

                _isShowInFeedAndFolder = value;

                NotifyPropertyChanged(nameof(IsShowInFeedAndFolder));
            }
        }

        private bool _isShowInFeed;
        public bool IsShowInFeed

        {
            get { return _isShowInFeed; }
            set
            {
                if (_isShowInFeed == value)
                    return;

                _isShowInFeed = value;

                NotifyPropertyChanged(nameof(IsShowInFeed));
            }
        }

        private bool _isDebugTextHasText;
        public bool IsDebugTextHasText
        {
            get
            {
                return _isDebugTextHasText;
            }
            set
            {
                if (_isDebugTextHasText == value)
                    return;

                _isDebugTextHasText = value;
                NotifyPropertyChanged(nameof(IsDebugTextHasText));
            }
        }

        #endregion

        #region == Status messages == 

        private string _statusBarMessage;
        public string StatusBarMessage
        {
            get { return _statusBarMessage; }
            set
            {
                if (_statusBarMessage == value)
                    return;

                _statusBarMessage = value;

                NotifyPropertyChanged(nameof(StatusBarMessage));
            }
        }

        #endregion

        #region == Error messages == 

        private bool _isShowMainErrorMessage = false;
        public bool IsShowMainErrorMessage
        {
            get { return _isShowMainErrorMessage; }
            set
            {
                if (_isShowMainErrorMessage == value)
                    return;

                _isShowMainErrorMessage = value;

                NotifyPropertyChanged(nameof(IsShowMainErrorMessage));
            }
        }

        private ErrorObject _mainError;
        public ErrorObject MainError
        {
            get { return _mainError; }
            set
            {
                if (_mainError == value)
                    return;

                _mainError = value;

                NotifyPropertyChanged(nameof(MainError));
            }
        }

        private bool _isShowHttpClientErrorMessage;
        public bool IsShowHttpClientErrorMessage
        {
            get { return _isShowHttpClientErrorMessage; }
            set
            {
                if (_isShowHttpClientErrorMessage == value)
                    return;

                _isShowHttpClientErrorMessage = value;

                NotifyPropertyChanged(nameof(IsShowHttpClientErrorMessage));
            }
        }

        private bool _isShowDatabaseErrorMessage;
        public bool IsShowDatabaseErrorMessage
        {
            get { return _isShowDatabaseErrorMessage; }
            set
            {
                if (_isShowDatabaseErrorMessage == value)
                    return;

                _isShowDatabaseErrorMessage = value;

                NotifyPropertyChanged(nameof(IsShowDatabaseErrorMessage));
            }
        }

        private ErrorObject _httpError;
        public ErrorObject HttpError
        {
            get { return _httpError; }
            set
            {
                if (_httpError == value)
                    return;

                _httpError = value;

                NotifyPropertyChanged(nameof(HttpError));
            }
        }

        private ErrorObject _databaseError;
        public ErrorObject DatabaseError
        {
            get { return _databaseError; }
            set
            {
                if (_databaseError == value)
                    return;

                _databaseError = value;

                NotifyPropertyChanged(nameof(DatabaseError));
            }
        }

        #endregion

        #endregion

        #region == Events ==

        public event EventHandler<ServiceDiscoveryEventArgs> OpenServiceDiscoveryView;
        public event EventHandler<BlogEntryEventArgs> OpenEditorView;
        public event EventHandler<BlogEntryEventArgs> OpenEditorNewView;

        // DebugWindow
        public delegate void DebugWindowShowHideEventHandler();
        public event DebugWindowShowHideEventHandler DebugWindowShowHide;

        public event EventHandler<bool> DebugWindowShowHide2;

        public event EventHandler<string> DebugOutput;

        public delegate void DebugClearEventHandler();
        public event DebugClearEventHandler DebugClear;

        public event EventHandler<string> WriteHtmlToContentPreviewBrowser;

        public event EventHandler<Uri> NavigateUrlToContentPreviewBrowser;

        public event EventHandler<Uri> OpenCurrentUrlInExternalBrowser;

        public delegate void ContentsBrowserWindowShowHideEventHandler();
        public event ContentsBrowserWindowShowHideEventHandler ContentsBrowserWindowShowHide;

        public event EventHandler<bool> ContentsBrowserWindowShowHide2;

        public event EventHandler<int> ResetListviewPosition;

        #endregion

        #region == Other ==

        private string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string _appDataFolder;
        private string _appConfigFilePath;

        private readonly DataAccess dataAccessModule = new DataAccess();

        private OpenDialogService _openDialogService = new OpenDialogService();

        private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        #endregion

        public MainViewModel()
        {
            // tmp
            IsSaveLog = true;
            IsShowMainErrorMessage = false;

            #region == Config folder ==

            _appDataFolder = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            _appConfigFilePath = _appDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";
            System.IO.Directory.CreateDirectory(_appDataFolder);

            #endregion

            #region == Commands init ==

            ServiceAddCommand = new RelayCommand(ServiceAddCommand_Execute, ServiceAddCommand_CanExecute);
            FolderAddCommand = new RelayCommand(FolderAddCommand_Execute, FolderAddCommand_CanExecute);
            ServiceUpdateCommand = new RelayCommand(ServiceUpdateCommand_Execute, ServiceUpdateCommand_CanExecute);

            TreeviewLeftDoubleClickCommand = new GenericRelayCommand<NodeTree>(
                param => TreeviewLeftDoubleClickCommand_Execute(param),
                param => TreeviewLeftDoubleClickCommand_CanExecute());

            ListviewLeftDoubleClickCommand = new GenericRelayCommand<EntryItem>(
                param => ListviewLeftDoubleClickCommand_Execute(param),
                param => ListviewLeftDoubleClickCommand_CanExecute());

            OpenEditorCommand = new GenericRelayCommand<EntryItem>(
                param => OpenEditorCommand_Execute(param),
                param => OpenEditorCommand_CanExecute());

            DeleteEntryCommand = new GenericRelayCommand<EntryItem>(
                param => DeleteEntryCommand_Execute(param),
                param => DeleteEntryCommand_CanExecute());

            GetEntryCommand = new GenericRelayCommand<EntryItem>(
                param => GetEntryCommand_Execute(param),
                param => GetEntryCommand_CanExecute());

            OpenInExternalBrowserCommand = new GenericRelayCommand<EntryItem>(
                param => OpenInExternalBrowserCommand_Execute(param),
                param => OpenInExternalBrowserCommand_CanExecute());

            OpenInExternalBrowserCurrentUrlCommand = new RelayCommand(OpenInExternalBrowserCurrentUrlCommand_Execute, OpenInExternalBrowserCurrentUrlCommand_CanExecute);

            OpenInAppBrowserCommand = new GenericRelayCommand<EntryItem>(
                param => OpenInAppBrowserCommand_Execute(param),
                param => OpenInAppBrowserCommand_CanExecute());

            ListviewEnterKeyCommand = new GenericRelayCommand<EntryItem>(
                param => ListviewEnterKeyCommand_Execute(param),
                param => ListviewEnterKeyCommand_CanExecute());

            ArchiveThisCommand = new GenericRelayCommand<EntryItem>(
                param => ArchiveThisCommand_Execute(param),
                param => ArchiveThisCommand_CanExecute());

            ArchiveSelectedCommand = new GenericRelayCommand<EntryItem>(
                param => ArchiveSelectedCommand_Execute(param),
                param => ArchiveSelectedCommand_CanExecute());
            
            OpenEditorAsNewCommand = new RelayCommand(OpenEditorAsNewCommand_Execute, OpenEditorAsNewCommand_CanExecute);
            ShowSettingsCommand = new RelayCommand(ShowSettingsCommand_Execute, ShowSettingsCommand_CanExecute);
            ShowDebugWindowCommand = new RelayCommand(ShowDebugWindowCommand_Execute, ShowDebugWindowCommand_CanExecute);
            CloseDebugWindowCommand = new RelayCommand(CloseDebugWindowCommand_Execute, CloseDebugWindowCommand_CanExecute);
            ClearDebugTextCommand = new RelayCommand(ClearDebugTextCommand_Execute, ClearDebugTextCommand_CanExecute);

            CloseContentBrowserCommand = new RelayCommand(CloseContentBrowserCommand_Execute, CloseContentBrowserCommand_CanExecute);

            ShowBrowserWindowCommand = new RelayCommand(ShowBrowserWindowCommand_Execute, ShowBrowserWindowCommand_CanExecute);

            ArchiveAllCommand = new RelayCommand(ArchiveAllCommand_Execute, ArchiveAllCommand_CanExecute);

            OpmlImportCommand = new RelayCommand(OpmlImportCommand_Execute, OpmlImportCommand_CanExecute);
            OpmlExportCommand = new RelayCommand(OpmlExportCommand_Execute, OpmlExportCommand_CanExecute);

            #endregion

            #region == SQLite DB init ==

            try
            {
                var databaseFileFolerPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + _appName;
                System.IO.Directory.CreateDirectory(databaseFileFolerPath);
                var dataBaseFilePath = databaseFileFolerPath + System.IO.Path.DirectorySeparatorChar + _appName + ".db";

                SqliteDataAccessResultWrapper res = dataAccessModule.InitializeDatabase(dataBaseFilePath);
                if (res.IsError)
                {
                    MainError = res.Error;
                    IsShowMainErrorMessage = true;

                    Debug.WriteLine("SQLite DB init: " + res.Error.ErrText + ": " + res.Error.ErrDescription + " @" + res.Error.ErrPlace + "@" + res.Error.ErrPlaceParent);
                }
            }
            catch (Exception e)
            {
                MainError = new ErrorObject();
                MainError.ErrType = ErrorObject.ErrTypes.DB;
                MainError.ErrCode = "";
                MainError.ErrText = e.ToString();
                MainError.ErrDescription = e.Message;
                MainError.ErrDatetime = DateTime.Now;
                MainError.ErrPlace = "dataAccessModule.InitializeDatabase";
                MainError.ErrPlaceParent = "MainViewModel()";
                IsShowMainErrorMessage = true;
            }

            #endregion

            // Load searvice tree
            if (File.Exists(_appDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml"))
            {
                XmlDocument doc = new XmlDocument();
                
                // TODO: try catch?

                doc.Load(_appDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml");

                _services.LoadXmlDoc(doc);

                InitClients();
            }
        }

        #region == Events ==

        public void OnDebugOutput(BaseClient sender, string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            if (IsDebugWindowEnabled)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DebugOutput?.Invoke(this, Environment.NewLine + data);
                });
            }

            IsDebugTextHasText = true;
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            StartUpdate();
        }

        #endregion

        #region == Startup and Shutdown ==

        // OnWindowLoaded
        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            #region == Load app setting  ==

            try
            {
                // Load config file
                if (File.Exists(_appConfigFilePath))
                {
                    XDocument xdoc = XDocument.Load(_appConfigFilePath);

                    #region == App Windows setting ==

                    if (sender is Window)
                    {
                        // Main Window element
                        var mainWindow = xdoc.Root.Element("MainWindow");
                        if (mainWindow != null)
                        {
                            var hoge = mainWindow.Attribute("top");
                            if (hoge != null)
                            {
                                (sender as Window).Top = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("left");
                            if (hoge != null)
                            {
                                (sender as Window).Left = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("height");
                            if (hoge != null)
                            {
                                (sender as Window).Height = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("width");
                            if (hoge != null)
                            {
                                (sender as Window).Width = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("state");
                            if (hoge != null)
                            {
                                if (hoge.Value == "Maximized")
                                {
                                    (sender as Window).WindowState = WindowState.Maximized;
                                }
                                else if (hoge.Value == "Normal")
                                {
                                    (sender as Window).WindowState = WindowState.Normal;
                                }
                                else if (hoge.Value == "Minimized")
                                {
                                    (sender as Window).WindowState = WindowState.Normal;
                                }
                            }

                        }

                    }

                    #endregion

                    #region == Options ==

                    var opts = xdoc.Root.Element("Options");
                    if (opts != null)
                    {
                        var hoge = opts.Attribute("SaveLog");
                        if (hoge != null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsSaveLog = true;

                            }
                            else
                            {
                                IsSaveLog = false;
                            }
                        }
                    }

                    #endregion
                }

                IsFullyLoaded = true;
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.WriteLine("FileNotFoundException while loading config: " + _appConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex + " while opening : " + _appConfigFilePath);
            }

            #endregion

            // error log
            if (IsSaveLog)
            {
                if (App.Current != null)
                {
                    App app = App.Current as App;
                    app.IsSaveErrorLog = true;
                    app.LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + _appName + "_errors.txt";
                }
            }

            // TODO: tmp
            IsDebugWindowEnabled = true;
            //CloseContentBrowserCommand_Execute();

            // starts update feeds and collections.
            StartUpdate();

            // starts hourly timer.
            var aTimer = new System.Timers.Timer(60 * 60 * 1000); // one hour in milliseconds
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();
        }

        // OnWindowClosing
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (!IsFullyLoaded)
                return;

            #region == Save Apps settings ==

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xdec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xdec);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            XmlAttribute attrs = doc.CreateAttribute("Version");
            attrs.Value = _appVer;
            root.SetAttributeNode(attrs);

            #region == Windows ==

            if (sender is Window)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("height");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("width");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();

                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);


                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }

            #endregion

            #region == Options ==

            XmlElement opts = doc.CreateElement(string.Empty, "Options", string.Empty);

            //
            attrs = doc.CreateAttribute("SaveLog");
            if (IsSaveLog)
            {
                attrs.Value = "True";
            }
            else
            {
                attrs.Value = "False";
            }
            opts.SetAttributeNode(attrs);

            /// 
            root.AppendChild(opts);

            #endregion

            try
            {
                // Save config file
                doc.Save(_appConfigFilePath);
            }
            //catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex + " while saving : " + _appConfigFilePath);
            }

            #endregion

            SaveServiceXml();

            // Save error logs.
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    App app = App.Current as App;
                    app.SaveErrorLog();
                });
            }
        }

        private void SaveServiceXml()
        {
            XmlDocument xdoc = _services.AsXmlDoc();

            xdoc.Save(System.IO.Path.Combine(_appDataFolder, "Searvies.xml"));
        }

        private void InitClients()
        {
            InitClientsRecursiveLoop(_services.Children);
        }

        private void InitClientsRecursiveLoop(ObservableCollection<NodeTree> nt)
        {
            // subscribe to DebugOutput event.
            foreach (NodeTree c in nt)
            {
                if ((c is NodeService) || (c is NodeFeed))
                {
                    if ((c as NodeService).Client != null)
                    {
                        (c as NodeService).Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);
                    }
                }

                if (c.Children.Count > 0)
                    InitClientsRecursiveLoop(c.Children);
            }
        }

        #endregion

        #region == Feed Auto Update ==

        private async void StartUpdate()
        {
            StatusBarMessage = "Updating...";

            await Task.Run(() => StartUpdateRecursiveLoopAsync(_services.Children));

            StatusBarMessage = "";
        }

        private async Task StartUpdateRecursiveLoopAsync(ObservableCollection<NodeTree> nt)
        {
            foreach (NodeTree c in nt)
            {
                if ((c is NodeEntryCollection) || (c is NodeFeed))
                {
                    if (c is NodeFeed)
                    {
                        DateTime now = DateTime.Now;
                        DateTime last = (c as NodeFeed).LastUpdate;

                        // check if lastupdate within...
                        if (last > now.AddHours(-1) && last <= now)
                        {
                            //Debug.WriteLine("Skippig: " + last.ToString());
                        }
                        else
                        {
                            //StatusBarMessage = "Updating " + (c as NodeFeed).Name;

                            (c as NodeFeed).LastUpdate = now;

                            //GetEntries(c);
                            Task nowait = Task.Run(() => GetEntriesAsync(c));

                            await Task.Delay(1000);
                        }
                    }
                }

                if (c.Children.Count > 0)
                {
                    //await Task.Run(() => StartUpdateRecursiveLoop(c.Children));
                    await StartUpdateRecursiveLoopAsync(c.Children);
                }
            }
        }

        #endregion

        #region == Public methods accessible from code behind ==

        public void AddFeed(FeedLink fl)
        {
            if (FeedDupeCheck(fl.FeedUri.AbsoluteUri))
            {
                Debug.WriteLine("FeedDupeCheck:" + fl.FeedUri.AbsoluteUri);
                return;
            }

            NodeFeed a = new(fl.Title, fl.FeedUri);
            a.IsSelected = true;

            a.SiteTitle = fl.SiteTitle;
            a.SiteUri = fl.SiteUri;

            a.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);

            // Add Node to internal (virtual) Treeview.
            if (Application.Current == null) { return; }
            if (SelectedNode is NodeFolder)
            {
                a.Parent = SelectedNode;
                Application.Current.Dispatcher.Invoke(() => SelectedNode.Children.Add(a));
                
                SelectedNode.IsExpanded = true;
            }
            else
            {
                a.Parent = _services;
                Application.Current.Dispatcher.Invoke(() => Services.Add(a));
            }

            SaveServiceXml();

            Task.Run(() => GetEntriesAsync(a));
        }

        private bool FeedDupeCheck(string feedUri)
        {
            return FeedDupeCheckRecursiveLoop(Services, feedUri);
        }

        private bool FeedDupeCheckRecursiveLoop(ObservableCollection<NodeTree> nt, string feedUri)
        {
            foreach (NodeTree c in nt)
            {
                if (c is NodeFeed)
                {
                    if ((c as NodeFeed).EndPoint.AbsoluteUri.Equals(feedUri))
                    {
                        return true;
                    }
                }

                if (c.Children.Count > 0)
                    if (FeedDupeCheckRecursiveLoop(c.Children, feedUri))
                        return true;
            }

            return false;
        }

        public void AddAtomPub(NodeService nodeService)
        {
            NodeService account = new NodeService(nodeService.Name, nodeService.UserName, nodeService.UserPassword, nodeService.EndPoint, ApiTypes.atAtomPub, ServiceTypes.AtomPub);

            account.Parent = _services;
            account.IsExpanded = true;
            account.IsSelected = true;
            account.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);

            // Add Node to internal (virtual) Treeview.
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() => Services.Add(account));

            SaveServiceXml();

            if (account.Client is AtomPubClient)
            {
                UpdateService(account);
            }
        }

        public void AddXmlRpc(RsdLink rsd, string userId, string userPassword)
        {
            Uri addr = null;
            ApiTypes at = ApiTypes.atUnknown;

            foreach (var hoge in rsd.Apis)
            {
                if ((hoge.Name.ToLower() == "wordpress") && hoge.Preferred)
                {
                    if (hoge.ApiLink != null)
                    {
                        addr = hoge.ApiLink;
                    }

                    at = ApiTypes.atXMLRPC_WordPress;

                    break;
                }
                else if ((hoge.Name.ToLower() == "movable type") && hoge.Preferred)
                {
                    if (hoge.ApiLink != null)
                    {
                        addr = hoge.ApiLink;
                    }

                    at = ApiTypes.atXMLRPC_MovableType;

                    break;
                }
            }

            if ((at != ApiTypes.atUnknown) && (addr != null))
            {
                NodeService account = new NodeService(rsd.Title, userId, userPassword, addr, at, ServiceTypes.XmlRpc);

                account.Parent = _services;
                account.IsExpanded = true;
                account.IsSelected = true;
                account.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);

                // Add Node to internal (virtual) Treeview.
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() => Services.Add(account));

                SaveServiceXml();

                if (account.Client is XmlRpcClient)
                {
                    UpdateService(account);
                }
            }
        }

        private async void UpdateService(NodeService account)
        {
            if ((account.ServiceType == ServiceTypes.XmlRpc) && (account.Client is XmlRpcClient))
            {
                // TODO: tmp. 
                account.Children.Clear();

                List<NodeWorkspace> blogs = await (account.Client as XmlRpcClient).GetBlogs();

                foreach (var b in blogs)
                {
                    if (b is NodeWorkspace)
                    {
                        foreach (var nec in b.Children)
                        {
                            if (nec is NodeXmlRpcEntryCollection)
                            {
                                account.Children.Add(nec);
                                nec.Parent = account;
                                nec.IsExpanded = true;
                                nec.IsSelected = true;
                            }
                        }
                    }
                }
            }
            else if ((account.ServiceType == ServiceTypes.AtomPub) && (account.Client is AtomPubClient))
            {
                // TODO: tmp. 
                account.Children.Clear();

                List<NodeWorkspace> blogs = await (account.Client as AtomPubClient).GetBlogs();

                foreach (var b in blogs)
                {
                    if (b is NodeWorkspace)
                    {
                        foreach (var nec in b.Children)
                        {
                            if (nec is NodeAtomPubEntryCollection)
                            {
                                account.Children.Add(nec);
                                nec.Parent = account;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("UpdateService: unknown type");
            }
        }

        public async void DeleteNodeTree(NodeTree nt)
        {
            if ((nt is NodeFolder) || (nt is NodeFeed) || (nt is NodeService))
            {
                if (nt is NodeFeed)
                {
                    var fnd = (nt as NodeFeed);

                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {   
                        // check status
                        if ( !((fnd.Status == NodeFeed.DownloadStatus.normal) || (fnd.Status == NodeFeed.DownloadStatus.error)) )
                        {
                            return;
                        }

                        IsBusy = true;

                    });

                    List<string> ids = new();
                    ids.Add(fnd.Id);

                    SqliteDataAccessResultWrapper resDelete = await DeleteEntriesByFeedIdsLock(ids);

                    if (resDelete.IsError)
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            fnd.ErrorDatabase = resDelete.Error;

                            if (fnd == SelectedNode)
                            {
                                DatabaseError = fnd.ErrorDatabase;
                                IsShowDatabaseErrorMessage = true;
                            }

                            IsBusy = false;

                            return;
                        });

                    }
                    else
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            nt.Parent.Children.Remove(nt);

                            IsBusy = false;
                        });
                    }
                }
                else if (nt is NodeFolder)
                {
                    if ((nt as NodeFolder).Children.Count> 0)
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsBusy = true;
                        });

                        List<NodeTree> tmpDeletes = new();

                        foreach (var ndc in (nt as NodeFolder).Children)
                        {
                            if (ndc is not NodeFeed)
                                continue;

                            // check status
                            if ( !( ((ndc as NodeFeed).Status == NodeFeed.DownloadStatus.normal) || ((ndc as NodeFeed).Status == NodeFeed.DownloadStatus.error)) )
                            {
                                IsBusy = false;

                                return;
                            }

                            List<string> ids = new();
                            ids.Add((ndc as NodeFeed).Id);

                            SqliteDataAccessResultWrapper resDelete = await DeleteEntriesByFeedIdsLock(ids);

                            if (resDelete.IsError)
                            {
                                if (Application.Current == null) { return; }
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    (ndc as NodeFeed).ErrorDatabase = resDelete.Error;

                                    if (ndc == SelectedNode)
                                    {
                                        DatabaseError = (ndc as NodeFeed).ErrorDatabase;
                                        IsShowDatabaseErrorMessage = true;
                                    }

                                    (ndc as NodeFeed).Status = NodeFeed.DownloadStatus.error;

                                    IsBusy = false;

                                    return;
                                });
                            }
                            else
                            {
                                tmpDeletes.Add(ndc);
                            }
                        }

                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (tmpDeletes.Count > 0)
                            {
                                foreach (var tmp in tmpDeletes)
                                {
                                    (nt as NodeFolder).Children.Remove(tmp);
                                }
                            }

                            // make sure folder is empty.
                            if ((nt as NodeFolder).Children.Count == 0)
                            {
                                (nt as NodeFolder).Parent.Children.Remove(nt);
                            }

                            IsBusy = false;
                        });
                    }
                    else
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            (nt as NodeFolder).Parent.Children.Remove(nt);
                        });
                    }
                }
                else if (nt is NodeService)
                {
                    // TODO:
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        nt.Parent.Children.Remove(nt);

                        //IsBusy = false;
                    });
                }
                else
                {
                    // TODO:
                }
            }
        }

        #endregion

        #region == Data retrieval and manupilations ==

        private SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIdLock(string id, bool bln)
        {
            SqliteDataAccessSelectResultWrapper res = new();

            try
            {
                _readerWriterLock.EnterReadLock();

                res = dataAccessModule.SelectEntriesByFeedId(id, bln);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            return res;
        }

        private SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIdsLock(List<string> list)
        {
            SqliteDataAccessSelectResultWrapper res = new();

            try
            {
                _readerWriterLock.EnterReadLock();

                res = dataAccessModule.SelectEntriesByFeedIds(list);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            return res;
        }

        private SqliteDataAccessSelectImageResultWrapper SelectImagesByImageIdLock(string id)
        {
            SqliteDataAccessSelectImageResultWrapper res = new();

            try
            {
                _readerWriterLock.EnterReadLock();

                res = dataAccessModule.SelectImageByImageId(id);
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            return res;
        }

        private SqliteDataAccessInsertResultWrapper InsertEntriesLock(List<EntryItem> list)
        {
            SqliteDataAccessInsertResultWrapper resInsert = new();
            
            bool isbreaked = false;

            try
            {
                _readerWriterLock.EnterWriteLock();
                if (_readerWriterLock.WaitingReadCount > 0)
                {
                    isbreaked = true;
                }
                else
                {
                    // Insert result to Sqlite database.
                    resInsert = dataAccessModule.InsertEntries(list);

                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
            if (isbreaked)
            {
                Thread.Sleep(10);
                //await Task.Delay(100);

                return InsertEntriesLock(list);
            }

            return resInsert;
        }

        private SqliteDataAccessInsertResultWrapper InsertImagesLock(List<EntryItem> list)
        {
            SqliteDataAccessInsertResultWrapper resInsert = new();

            bool isbreaked = false;

            try
            {
                _readerWriterLock.EnterWriteLock();
                if (_readerWriterLock.WaitingReadCount > 0)
                {
                    isbreaked = true;
                }
                else
                {
                    // Insert result to Sqlite database.
                    resInsert = dataAccessModule.InsertImages(list);

                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
            if (isbreaked)
            {
                Thread.Sleep(10);
                //await Task.Delay(100);

                return InsertImagesLock(list);
            }

            return resInsert;
        }

        private SqliteDataAccessResultWrapper UpdateAllEntriesAsReadLock(List<string> list)
        {
            SqliteDataAccessResultWrapper res = new();

            bool isbreaked = false;

            try
            {
                _readerWriterLock.EnterWriteLock();
                if (_readerWriterLock.WaitingReadCount > 0)
                {
                    isbreaked = true;
                }
                else
                {
                    res = dataAccessModule.UpdateAllEntriesAsRead(list);

                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
            if (isbreaked)
            {
                Thread.Sleep(10);
                //await Task.Delay(100);

                return UpdateAllEntriesAsReadLock(list);
            }

            return res;
        }
        
        private SqliteDataAccessResultWrapper UpdateEntriesAsReadLock(List<EntryItem> list)
        {
            SqliteDataAccessResultWrapper res = new();

            bool isbreaked = false;

            try
            {
                _readerWriterLock.EnterWriteLock();
                if (_readerWriterLock.WaitingReadCount > 0)
                {
                    isbreaked = true;
                }
                else
                {
                    res = dataAccessModule.UpdateEntriesAsRead(list);

                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
            if (isbreaked)
            {
                Thread.Sleep(10);
                //await Task.Delay(100);

                return UpdateEntriesAsReadLock(list);
            }

            return res;
        }

        private SqliteDataAccessResultWrapper UpdateEntryStatusLock(EntryItem entry)
        {
            SqliteDataAccessResultWrapper res = new();

            bool isbreaked = false;

            try
            {
                _readerWriterLock.EnterWriteLock();
                if (_readerWriterLock.WaitingReadCount > 0)
                {
                    isbreaked = true;
                }
                else
                {
                    res = dataAccessModule.UpdateEntryStatus(entry);

                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
            if (isbreaked)
            {
                Thread.Sleep(10);
                //await Task.Delay(100);

                return UpdateEntryStatusLock(entry);
            }

            return res;
        }

        private async Task<SqliteDataAccessResultWrapper> DeleteEntriesByFeedIdsLock(List<string> list)
        {
            SqliteDataAccessResultWrapper res = new();

            bool isbreaked = false;

            try
            {
                _readerWriterLock.EnterWriteLock();
                if (_readerWriterLock.WaitingReadCount > 0)
                {
                    isbreaked = true;
                }
                else
                {
                    res = dataAccessModule.DeleteEntriesByFeedIds(list);

                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
            if (isbreaked)
            {
                Thread.Sleep(10);
                //await Task.Delay(100);

                return await DeleteEntriesByFeedIdsLock(list);
            }

            return res;
        }

        private async Task GetEntriesAsync(NodeTree nd)
        {
            if (nd == null)
                return;

            if (nd is NodeFeed)
            {
                NodeFeed fnd = nd as NodeFeed;

                var fc = fnd.Client;

                // check some conditions.
                if ((fnd.Api != ApiTypes.atFeed) || (fc == null))
                    return;

                // Update Node Downloading Status
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    fnd.Status = NodeFeed.DownloadStatus.downloading;

                    fnd.LastUpdate = DateTime.Now;
                });

                // Get Entries from web.
                HttpClientEntryItemCollectionResultWrapper resEntries = await fc.GetEntries(fnd.EndPoint, fnd.Id);

                // Check Node exists. Could have been deleted.
                if (fnd == null)
                    return; 

                // Result is HTTP Error
                if (resEntries.IsError)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Sets Node Error.
                        fnd.ErrorHttp = resEntries.Error;

                        // If Node is selected, show the Error.
                        if (fnd == SelectedNode)
                        {
                            HttpError = fnd.ErrorHttp;
                            IsShowHttpClientErrorMessage = true;
                        }

                        // Update Node Downloading Status
                        fnd.Status = NodeFeed.DownloadStatus.error;
                    });

                    return;
                }
                // Result is success.
                else
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Clear Node Error
                        fnd.ErrorHttp = null;
                        if (fnd == SelectedNode)
                        {
                            // Hide any Error Message
                            HttpError = null;
                            IsShowHttpClientErrorMessage = false;
                        }

                        fnd.Status = NodeFeed.DownloadStatus.saving;
                    });

                    // 
                    SqliteDataAccessInsertResultWrapper resInsert = InsertEntriesLock(resEntries.Entries);

                    if (resEntries.Entries.Count > 0)
                    {
                        // Get Images
                        await GetImagesAsync(fnd, resInsert.InsertedEntries);

                        // Result is DB Error
                        if (resInsert.IsError)
                        {
                            if (Application.Current == null) { return; }
                            Application.Current.Dispatcher.Invoke(() =>
                            {

                                // Sets Node Error.
                                fnd.ErrorDatabase = resInsert.Error;

                                // If Node is selected, show the Error.
                                if (fnd == SelectedNode)
                                {
                                    DatabaseError = fnd.ErrorDatabase;
                                    IsShowDatabaseErrorMessage = true;
                                }

                                fnd.Status = NodeFeed.DownloadStatus.error;

                                // IsBusy = false;
                                return;
                            });
                        }
                        else
                        {
                            if (Application.Current == null) { return; }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                // Clear error.
                                fnd.ErrorDatabase = null;

                                // If Node is selected hide any Error message if shown.
                                if (nd == SelectedNode)
                                {
                                    DatabaseError = null;
                                    IsShowDatabaseErrorMessage = false;
                                }

                                // Update Node Unread count.
                                fnd.EntryCount += resInsert.InsertedEntries.Count;

                                // If parent is a folder 
                                if (nd.Parent is NodeFolder)
                                {
                                    // Update parent folder's unread count.
                                    (nd.Parent as NodeFolder).EntryCount = (nd.Parent as NodeFolder).EntryCount + resInsert.InsertedEntries.Count;
                                }
                            });

                            // If Node is selected Load Entries.
                            if (nd == SelectedNode)
                            {
                                if (resInsert.InsertedEntries.Count > 0)
                                {
                                    await LoadEntriesAsync(nd);

                                    return;
                                }
                            }
                            else if ((nd.Parent == SelectedNode) && (nd.Parent is NodeFolder))
                            {
                                if (Application.Current == null) { return; }
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    foreach (var asdf in resInsert.InsertedEntries)
                                    {
                                        Entries.Insert(0, asdf);
                                    }

                                    // sort
                                    Entries = new ObservableCollection<EntryItem>(Entries.OrderByDescending(n => n.Published));
                                });
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("0 entries. ");
                    }

                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Update Node Downloading Status
                        fnd.Status = NodeFeed.DownloadStatus.normal;
                    });
                }
            }
            else if (nd is NodeEntryCollection)
            {
                if ((nd as NodeEntryCollection).Parent is NodeService)
                {
                    NodeService ns = (nd as NodeEntryCollection).Parent as NodeService;


                    var bc = (nd as NodeEntryCollection).Client;
                    if (bc == null)
                        return;

                    // TODO:

                    HttpClientEntryItemCollectionResultWrapper resEntries = await bc.GetEntries((nd as NodeEntryCollection).Uri, ns.Id);


                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var asdf in resEntries.Entries)
                        {
                            Entries.Insert(0, asdf);
                        }

                        // sort
                        Entries = new ObservableCollection<EntryItem>(Entries.OrderByDescending(n => n.Published));
                    });
                }
            }
        }

        private async Task LoadEntriesAsync(NodeTree nd, bool forceUnread = false)
        {
            if (nd == null)
                return;

            // don't clear Entries here.

            if (nd is NodeFeed)
            {
                NodeFeed fnd = nd as NodeFeed;

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsWorking = true;

                    if (forceUnread)
                        fnd.IsDisplayUnarchivedOnly = true;

                    fnd.Status = NodeFeed.DownloadStatus.loading;
                });

                SqliteDataAccessSelectResultWrapper res = SelectEntriesByFeedIdLock(fnd.Id, fnd.IsDisplayUnarchivedOnly);

                if (res.IsError)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // set's error
                        fnd.ErrorDatabase = res.Error;

                        if (nd == SelectedNode)
                        {
                            // show error
                            DatabaseError = fnd.ErrorDatabase;
                            IsShowDatabaseErrorMessage = true;
                        }

                        IsWorking = false;

                        fnd.Status = NodeFeed.DownloadStatus.error;

                        return;
                    });

                }
                else
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Clear error
                        fnd.ErrorDatabase = null;

                        // Update the count
                        fnd.EntryCount = res.UnreadCount;

                        // If this is selected Node.
                        if (nd == SelectedNode)
                        {
                            // Hide error
                            DatabaseError = null;
                            IsShowDatabaseErrorMessage = false;

                            // Load entries.
                            //Entries = res.SelectedEntries;
                            // COPY!! 
                            Entries = new ObservableCollection<EntryItem>(res.SelectedEntries);

                            if (Entries.Count > 0)
                                ResetListviewPosition?.Invoke(this, 0);
                        }

                        fnd.Status = NodeFeed.DownloadStatus.normal;

                        IsWorking = false;
                    });

                    if (nd == SelectedNode)
                        if (res.SelectedEntries.Count > 0)
                            await LoadImagesAsync(nd, res.SelectedEntries);
                }

            }
            else if (nd is NodeFolder)
            {
                NodeFolder ndf = nd as NodeFolder;

                if (ndf.Children.Count > 0)
                {
                    IsWorking = true;

                    List<string> tmpList = new();

                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (NodeTree nt in ndf.Children)
                        {
                            if (nt is NodeFeed)
                            {
                                tmpList.Add((nt as NodeFeed).Id);
                            }
                        }

                        // TODO:
                        //ndf.Status = NodeFeed.DownloadStatus.normal;
                    });

                    SqliteDataAccessSelectResultWrapper res = SelectEntriesByFeedIdsLock(tmpList);

                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ndf.EntryCount = res.SelectedEntries.Count;

                        if (nd == SelectedNode)
                        {
                            // Load entries.  
                            //Entries = res.SelectedEntries;
                            // COPY!!
                            Entries = new ObservableCollection<EntryItem>(res.SelectedEntries);

                            if (Entries.Count > 0)
                                ResetListviewPosition?.Invoke(this, 0);
                        }

                        // TODO:
                        //ndf.Status = NodeFeed.DownloadStatus.normal;

                        IsWorking = false;
                    });

                    if (nd == SelectedNode)
                        if (res.SelectedEntries.Count > 0)
                            await LoadImagesAsync(nd, res.SelectedEntries);
                }

            }
            else if (nd is NodeEntryCollection)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsBusy = true;

                    // This changes the listview.
                    NotifyPropertyChanged(nameof(Entries));

                    IsBusy = false;
                });
            }
        }

        private async Task GetImagesAsync(NodeTree nd, ObservableCollection<EntryItem> entries)
        {
            if (nd is not NodeFeed) 
                return;

            //if (nd != SelectedNode)
            //    return;

            // Get images.
            List<EntryItem> list = new();

            foreach (var hoge in entries)
            {
                //if (nd != SelectedNode)
                //    return;

                if (!hoge.IsImageDownloaded) 
                { 
                    if (hoge.ImageUri != null)
                    {
                        list.Add(hoge);
                    }
                }
            }

            var fc = (nd as NodeFeed).Client;

            if ((fc is not null) && (list.Count > 0))
            {
                // Get it off the site.
                await fc.GetImages(list);

                SqliteDataAccessInsertResultWrapper resInsert = InsertImagesLock(list);

                if (resInsert.IsError)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // set's error
                        (nd as NodeFeed).ErrorDatabase = resInsert.Error;

                        if (nd == SelectedNode)
                        {
                            // show error
                            DatabaseError = (nd as NodeFeed).ErrorDatabase;
                            IsShowDatabaseErrorMessage = true;
                        }

                        (nd as NodeFeed).Status = NodeFeed.DownloadStatus.error;

                        return;
                    });

                }
                else
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Clear error
                        (nd as NodeFeed).ErrorDatabase = null;

                        // If this is selected Node.
                        if (nd == SelectedNode)
                        {
                            // Hide error
                            DatabaseError = null;
                            IsShowDatabaseErrorMessage = false;

                        }

                        (nd as NodeFeed).Status = NodeFeed.DownloadStatus.normal;

                        return;
                    });
                }
            }
        }

        private async Task LoadImagesAsync(NodeTree nd, ObservableCollection<EntryItem> entries)
        {
            if ((nd is not NodeFeed) && (nd is not NodeFolder))
                return;

            if (nd != SelectedNode)
                return;

            foreach (var hoge in entries)
            {
                if (nd != SelectedNode)
                    return;

                if (hoge.IsImageDownloaded)
                {
                    if (!string.IsNullOrEmpty(hoge.ImageId))
                    {
                        // Get it from image table.
                        SqliteDataAccessSelectImageResultWrapper res = SelectImagesByImageIdLock(hoge.ImageId);

                        if (!res.IsError)
                        {
                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        hoge.Image = res.Image;
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("Exception @LoadImages: " + e.Message);
                                    }
                                });
                            }
                        }

                        await Task.Delay(20);
                    }
                }
            }
        }

        private async Task ArchiveAllAsync(NodeTree nd)
        {
            if (nd == null)
                return;

            if (nd is NodeFeed)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsWorking = true;

                    // TODO: not really saving
                    (nd as NodeFeed).Status = NodeFeed.DownloadStatus.saving;
                });

                List<string> list = new();
                list.Add((nd as NodeFeed).Id);

                SqliteDataAccessResultWrapper res = UpdateAllEntriesAsReadLock(list);

                if (res.IsError)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        (nd as NodeFeed).ErrorDatabase = res.Error;

                        if (nd == SelectedNode)
                        {
                            DatabaseError = (nd as NodeFeed).ErrorDatabase;
                            IsShowDatabaseErrorMessage = true;
                        }

                        (nd as NodeFeed).Status = NodeFeed.DownloadStatus.error;

                        IsWorking = false;
                    });

                    return;
                }
                else
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Clear error
                        (nd as NodeFeed).ErrorDatabase = null;

                        //
                        (nd as NodeFeed).Status = NodeFeed.DownloadStatus.normal;

                        if (res.AffectedCount > 0)
                        {
                            // minus the parent folder's unread count.
                            if (nd.Parent is NodeFolder)
                            {
                                (nd.Parent as NodeFolder).EntryCount = (nd.Parent as NodeFolder).EntryCount - (nd as NodeFeed).EntryCount;
                            }

                            // reset unread count.
                            (nd as NodeFeed).EntryCount = 0;

                            if (nd == SelectedNode)
                            {
                                DatabaseError = null;
                                IsShowDatabaseErrorMessage = false;

                                // clear here.
                                Entries.Clear();

                                // 
                                //Task.Run(() => LoadEntries(_selectedNode));
                            }
                        }

                        IsWorking = false;
                    });

                    if (nd == SelectedNode)
                        await LoadEntriesAsync(_selectedNode);
                }
            }
            else if (nd is NodeFolder)
            {
                if ((nd as NodeFolder).Children.Count > 0)
                {
                    List<string> list = new();

                    foreach (NodeTree hoge in (nd as NodeFolder).Children)
                    {
                        if (hoge is NodeFeed)
                        {
                            list.Add((hoge as NodeFeed).Id);
                        }
                    }

                    SqliteDataAccessResultWrapper res = UpdateAllEntriesAsReadLock(list);

                    if (res.AffectedCount > 0)
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (NodeTree hoge in (nd as NodeFolder).Children)
                            {
                                if (hoge is NodeFeed)
                                {
                                    (hoge as NodeFeed).EntryCount = 0;
                                }
                            }

                            (nd as NodeFolder).EntryCount = 0;

                            if (nd == SelectedNode)
                                Entries.Clear();
                        });
                    }
                }

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsWorking = false;
                });
            }
        }

        private void ArchiveThis(NodeTree nd, FeedEntryItem entry)
        {
            if (nd == null)
                return;

            if ((nd is not NodeFeed) && (nd is not NodeFolder))
                return;

            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
            });

            List<EntryItem> list = new();

            list.Add(entry);

            SqliteDataAccessResultWrapper res = UpdateEntriesAsReadLock(list);

            if (res.IsError)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (nd is NodeFeed)
                        (nd as NodeFeed).ErrorDatabase = res.Error;

                    if ((nd == SelectedNode) && (nd is NodeService))
                    {
                        DatabaseError = (nd as NodeService).ErrorDatabase;
                        IsShowDatabaseErrorMessage = true;
                    }

                    IsBusy = false;
                });

                return;
            }
            else
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Clear error
                    if (nd is NodeFeed)
                        (nd as NodeFeed).ErrorDatabase = null;

                    if (res.AffectedCount > 0)
                    {
                        // remove entry from list
                        if (nd is NodeFeed)
                        {
                            if (nd.Parent is NodeFolder)
                            {
                                (nd.Parent as NodeFolder).EntryCount--;
                            }
                        }
                        if (nd is NodeFolder)
                        {
                            foreach (var cnd in (nd as NodeFolder).Children)
                            {
                                if (cnd is NodeFeed)
                                {
                                    if ((cnd as NodeFeed).Id == entry.ServiceId)
                                    {
                                        (cnd as NodeFeed).EntryCount--;
                                    }
                                }
                            }

                        }

                        // minus the count.
                        nd.EntryCount--;

                        // remove
                        if (nd == SelectedNode)
                            Entries.Remove(entry);
                    }

                    IsBusy = false;
                });
            }
        }

        private void UpdateEntryStatus(NodeTree nd, FeedEntryItem entry)
        {
            if (nd == null)
                return;

            if ((nd is not NodeFeed) && (nd is not NodeFolder))
                return;

            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
            });

            SqliteDataAccessResultWrapper res = UpdateEntryStatusLock(entry);

            if (res.IsError)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (nd is NodeFeed)
                        (nd as NodeFeed).ErrorDatabase = res.Error;

                    if ((nd == SelectedNode) && (nd is NodeFeed))
                    {
                        DatabaseError = (nd as NodeFeed).ErrorDatabase;
                        IsShowDatabaseErrorMessage = true;
                    }

                    IsBusy = false;
                });

                return;
            }
            else
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Clear error
                    if (nd is NodeFeed)
                        (nd as NodeFeed).ErrorDatabase = null;

                    if (nd == SelectedNode)
                    {
                        DatabaseError = null;
                        IsShowDatabaseErrorMessage = false;
                    }

                    IsBusy = false;
                });
            }
        }

        // TODO
        private async Task<bool> GetEntry(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return false;

            if (!(_selectedNode is NodeEntryCollection))
                return false;

            // Skip FeedEntry
            if (selectedEntry is not EntryFull)
                return false;

            BlogClient bc = selectedEntry.Client as BlogClient;
            if (bc == null)
                return false;

            if ((selectedEntry as EntryFull).EditUri == null)
                return false;

            EntryFull bfe = await bc.GetFullEntry((selectedEntry as EntryFull).EditUri, selectedEntry.ServiceId, selectedEntry.EntryId);

            if (selectedEntry == null)
                return false;

            //selectedEntry.EntryBody = bfe;

            return true;
        }

        // TODO
        private async Task<bool> DeleteEntry(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return false;

            if (!(_selectedNode is NodeEntryCollection))
                return false;

            // Skip FeedEntry
            if (selectedEntry is not EntryFull)
                return false;

            BlogClient bc = selectedEntry.Client as BlogClient;

            if (bc == null)
                return false;

            if ((selectedEntry as EntryFull).EditUri == null)
                return false;

            bool b = await bc.DeleteEntry((selectedEntry as EntryFull).EditUri);

            return b;
        }

        #endregion

        #region == ICommands ==

        #region == TreeView ==

        public ICommand ServiceAddCommand { get; }

        public bool ServiceAddCommand_CanExecute()
        {
            return true;
        }

        public void ServiceAddCommand_Execute()
        {
            ServiceDiscoveryEventArgs ag = new ServiceDiscoveryEventArgs();

            OpenServiceDiscoveryView?.Invoke(this, ag);
        }

        public ICommand FolderAddCommand { get; }

        public bool FolderAddCommand_CanExecute()
        {
            return true;
        }

        public void FolderAddCommand_Execute()
        {
            NodeFolder folder = new("New Folder");
            folder.Parent = _services;
            folder.IsSelected = true;

            // Add NodeFolder to internal (virtual) Treeview.
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() => Services.Add(folder));
        }

        public ICommand ServiceUpdateCommand { get; }

        public bool ServiceUpdateCommand_CanExecute()
        {
            if (_selectedNode == null)
                return false;

            if ((_selectedNode is NodeFeed) || (_selectedNode is NodeService) || (_selectedNode is NodeEntryCollection))
                return true;
            else
                return false;
        }

        public void ServiceUpdateCommand_Execute()
        {
            if (_selectedNode == null)
                return;

            if (_selectedNode is NodeEntryCollection)
            {
                Task.Run(() => GetEntriesAsync(_selectedNode));
            }
            else if (_selectedNode is NodeFeed)
            {
                // don't clear here.
                //if ((_selectedNode as NodeFeed).List.Count > 0) (_selectedNode as NodeFeed).List.Clear();

                (_selectedNode as NodeFeed).LastUpdate = DateTime.Now;

                Task.Run(() => GetEntriesAsync(_selectedNode));
            }
            else if (_selectedNode is NodeService)
            {
                UpdateService(_selectedNode as NodeService);
            }
        }

        public ICommand TreeviewLeftDoubleClickCommand { get; }

        public bool TreeviewLeftDoubleClickCommand_CanExecute()
        {
            return true;
        }

        public void TreeviewLeftDoubleClickCommand_Execute(NodeTree selectedNode)
        {
            if (selectedNode == null)
                return;

            selectedNode.IsExpanded = selectedNode.IsExpanded ? false : true;
        }

        #endregion

        #region == ListView ==

        public ICommand ListviewLeftDoubleClickCommand { get; }

        public bool ListviewLeftDoubleClickCommand_CanExecute()
        {
            if (SelectedItem == null) return false;
            return true;
        }

        public void ListviewLeftDoubleClickCommand_Execute(EntryItem selectedEntry)
        {
            if (SelectedNode == null)
                return;
            if (selectedEntry == null)
                return;

            if ((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder))
            {
                if (OpenInExternalBrowserCommand_CanExecute())
                    OpenInExternalBrowserCommand_Execute(selectedEntry);
            }
            else if (SelectedNode is NodeEntryCollection)
                if (OpenEditorCommand_CanExecute())
                    OpenEditorCommand.Execute(selectedEntry);
        }

        public ICommand ListviewEnterKeyCommand { get; }

        public bool ListviewEnterKeyCommand_CanExecute()
        {
            return true;
        }

        public void ListviewEnterKeyCommand_Execute(EntryItem selectedEntry)
        {
            if (SelectedNode == null)
                return;
            if (selectedEntry == null)
                return;

            if ((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder))
            {
                if (OpenInExternalBrowserCommand_CanExecute())
                    OpenInExternalBrowserCommand_Execute(selectedEntry);
            }
            else if (SelectedNode is NodeEntryCollection)
                if (OpenEditorCommand_CanExecute())
                    OpenEditorCommand.Execute(selectedEntry);
        }

        public ICommand OpenEditorCommand { get; }

        public bool OpenEditorCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void OpenEditorCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry is EntryFull)
            {
                if ((selectedEntry as EntryFull).EntryBody == null)
                    return;

                if (selectedEntry.Client == null)
                    return;

                BlogEntryEventArgs ag = new BlogEntryEventArgs
                {
                    Entry = (selectedEntry as EntryFull).EntryBody
                    //
                };

                OpenEditorView?.Invoke(this, ag);
            }


        }

        public ICommand DeleteEntryCommand { get; }

        public bool DeleteEntryCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void DeleteEntryCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry is EntryItem)
            {
                if (selectedEntry.Client == null)
                    return;

                if (selectedEntry is EntryItem)
                {
                    Task.Run(async () => {
                        bool b = await this.DeleteEntry(selectedEntry); ;
                        if (b)
                        {
                            if (selectedEntry.NodeEntry == null)
                                return;

                            // remove item from the list.
                            try
                            {
                                if (Application.Current == null) { return; }
                                Application.Current.Dispatcher.Invoke(() => selectedEntry.NodeEntry.List.Remove(selectedEntry));
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine("Error @NodeEntry.List.Remove" + e.Message);
                            }

                            NotifyPropertyChanged(nameof(Entries));
                        }
                    });
                }
            }

        }

        public ICommand GetEntryCommand { get; }

        public bool GetEntryCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void GetEntryCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry is EntryItem)
            {
                if (selectedEntry.Client == null)
                    return;

                if (selectedEntry is EntryItem)
                {
                    Task.Run(async () => {
                        bool b = await this.GetEntry(selectedEntry); ;
                        if (b)
                        {
                            if (selectedEntry.NodeEntry == null)
                                return;

                            if (selectedEntry == SelectedItem)
                            {
                                NotifyPropertyChanged(nameof(SelectedItem));

                                //System.Diagnostics.Debug.WriteLine("GetEntryCommand_Execute.");
                            }

                        }
                    });
                }
            }

        }

        public ICommand OpenEditorAsNewCommand { get; }

        public bool OpenEditorAsNewCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void OpenEditorAsNewCommand_Execute()
        {
            if (SelectedNode == null)
                return;

            if (!(SelectedNode is NodeEntryCollection))
                return;

            string serviceId = ((SelectedNode as NodeEntryCollection).Parent.Parent as NodeService).Id;


            // TODO: Check "accept".

            EntryFull newEntry = null;

            if (SelectedNode is NodeAtomPubEntryCollection)
            {
                newEntry = new AtomEntry("", serviceId, (SelectedNode as NodeEntryCollection).Client);
            }
            else if (SelectedNode is NodeXmlRpcEntryCollection)
            {
                // TODO: temp
                newEntry = new MTEntry("", serviceId, (SelectedNode as NodeEntryCollection).Client);
            }
            /*
            else if (SelectedNode is NodeXmlRpcMTEntryCollection)
            {
                newEntry = new MTEntry("", serviceId, (SelectedNode as NodeEntryCollection).Client);
            }
            else if (SelectedNode is NodeXmlRpcWPEntryCollection)
            {
                newEntry = new WPEntry("", serviceId, (SelectedNode as NodeEntryCollection).Client);
            }
            */

            if (newEntry == null)
                return;

            newEntry.PostUri = (SelectedNode as NodeEntryCollection).Uri;

            BlogEntryEventArgs ag = new BlogEntryEventArgs
            {
                Entry = newEntry
            };

            OpenEditorNewView?.Invoke(this, ag);
        }

        public ICommand OpenInExternalBrowserCommand { get; }

        public bool OpenInExternalBrowserCommand_CanExecute()
        {
            return true;
        }

        public void OpenInExternalBrowserCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry.AltHtmlUri != null)
            {
                // TODO: Not good. This write html to browser...
                SelectedItem = selectedEntry;

                //System.Diagnostics.Process.Start(selectedEntry.AltHTMLUri.AbsoluteUri);
                ProcessStartInfo psi = new ProcessStartInfo(selectedEntry.AltHtmlUri.AbsoluteUri);
                psi.UseShellExecute = true;
                Process.Start(psi);

                if (selectedEntry is FeedEntryItem)
                {
                    (selectedEntry as FeedEntryItem).Status = FeedEntryItem.ReadStatus.rsVisited;

                    if (SelectedNode != null)
                    {
                        if ((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder))
                        {
                            // UPDATE DB
                            //UpdateEntryStatus(SelectedNode, selectedEntry as FeedEntryItem);
                            Task.Run(() => UpdateEntryStatus(SelectedNode, selectedEntry as FeedEntryItem));
                        }
                    }
                }
            }
        }

        public ICommand OpenInExternalBrowserCurrentUrlCommand { get; }

        public bool OpenInExternalBrowserCurrentUrlCommand_CanExecute()
        {
            return true;
        }

        public void OpenInExternalBrowserCurrentUrlCommand_Execute()
        {
            if (SelectedItem == null)
                return;

            if (SelectedItem.AltHtmlUri != null)
            {
                OpenCurrentUrlInExternalBrowser?.Invoke(this, null);
            }
        }

        public ICommand OpenInAppBrowserCommand { get; }

        public bool OpenInAppBrowserCommand_CanExecute()
        {
            return true;
        }

        public void OpenInAppBrowserCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry.AltHtmlUri != null)
            {
                // TODO: Not good. This write html to browser....
                SelectedItem = selectedEntry;

                // Bring Browser to front.
                IsContentBrowserVisible = true;

                NavigateUrlToContentPreviewBrowser?.Invoke(this, selectedEntry.AltHtmlUri);

                if (selectedEntry is FeedEntryItem)
                {
                    (selectedEntry as FeedEntryItem).Status = FeedEntryItem.ReadStatus.rsVisited;

                    if (SelectedNode != null)
                    {
                        if ((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder))
                        {
                            // UPDATE DB
                            //UpdateEntryStatus(SelectedNode, selectedEntry as FeedEntryItem);
                            Task.Run(() => UpdateEntryStatus(SelectedNode, selectedEntry as FeedEntryItem));
                        }
                    }
                }
            }
        }

        public ICommand ArchiveAllCommand { get; }

        public bool ArchiveAllCommand_CanExecute()
        {
            if (SelectedNode == null) 
                return false;

            //return (SelectedNode is NodeFeed) ? true : false;
            if (!((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder)))
                return false;

            return true;
        }

        public void ArchiveAllCommand_Execute()
        {
            if (SelectedNode == null)
                return;

            if (!((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder)))
                return;

            //ArchiveAll(SelectedNode);
            Task.Run(() => ArchiveAllAsync(SelectedNode));
        }

        public ICommand ArchiveThisCommand { get; }

        public bool ArchiveThisCommand_CanExecute()
        {
            if (SelectedNode == null) 
                return false;

            //return (SelectedNode is NodeFeed) ? true : false;
            if (!((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder)))
                return false;

            return true;
        }

        public void ArchiveThisCommand_Execute(EntryItem selectedEntry)
        {
            if (SelectedNode == null)
                return;

            if (!((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder)))
                return;

            if (selectedEntry == null)
                return;

            if (!(selectedEntry is FeedEntryItem))
                return;

            //ArchiveThis(SelectedNode, selectedEntry as FeedEntryItem);
            Task.Run(() => ArchiveThis(SelectedNode, selectedEntry as FeedEntryItem));
        }

        public ICommand ArchiveSelectedCommand { get; }

        public bool ArchiveSelectedCommand_CanExecute()
        {
            if (SelectedNode == null)
                return false;

            //return (SelectedNode is NodeFeed) ? true : false;
            if (!((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder)))
                return false;

            if (SelectedItem is null)
                return false;

            if (SelectedItem is not FeedEntryItem)
                return false;

            if (!string.IsNullOrEmpty(SelectedItem.CommonStatus))
            {
                if (SelectedItem.CommonStatus.Equals("IsArchived"))
                    return false;
            }

            return true;
        }

        public void ArchiveSelectedCommand_Execute(EntryItem selectedEntry)
        {
            if (SelectedNode == null)
                return;

            if (!((SelectedNode is NodeFeed) || (SelectedNode is NodeFolder)))
                return;

            if (selectedEntry == null)
                return;

            if (selectedEntry is not FeedEntryItem)
                return;

            if (selectedEntry != SelectedItem)
                return;

            if (SelectedItem is null)
                return;

            if (SelectedItem is not FeedEntryItem)
                return;

            if (SelectedItem.CommonStatus.Equals("IsArchived"))
                return;

            //ArchiveThis(SelectedNode, selectedEntry as FeedEntryItem);
            Task.Run(() => ArchiveThis(SelectedNode, selectedEntry as FeedEntryItem));
        }

        #endregion

        #region == Visibility control ==

        public ICommand ShowSettingsCommand { get; }

        public bool ShowSettingsCommand_CanExecute()
        {
            return true;
        }

        public void ShowSettingsCommand_Execute()
        {
            // TODO:
            //System.Diagnostics.Debug.WriteLine("ShowSettingsCommand_Execute: not implemented yet.");

        }

        public ICommand ShowDebugWindowCommand { get; }
        public bool ShowDebugWindowCommand_CanExecute()
        {
            return true;
        }
        public void ShowDebugWindowCommand_Execute()
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DebugWindowShowHide?.Invoke();
            });
        }

        public ICommand CloseDebugWindowCommand { get; }
        public bool CloseDebugWindowCommand_CanExecute()
        {
            return true;
        }
        public void CloseDebugWindowCommand_Execute()
        {
            DebugWindowShowHide2?.Invoke(this, false);
        }

        public ICommand ClearDebugTextCommand { get; }
        public bool ClearDebugTextCommand_CanExecute()
        {
            return true;
        }
        public void ClearDebugTextCommand_Execute()
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DebugClear?.Invoke();
            });

            IsDebugTextHasText = false;
        }

        public ICommand CloseContentBrowserCommand { get; }
        public bool CloseContentBrowserCommand_CanExecute()
        {
            return true;
        }
        public void CloseContentBrowserCommand_Execute()
        {
            ContentsBrowserWindowShowHide2?.Invoke(this, false);
        }

        public ICommand ShowBrowserWindowCommand { get; }
        public bool ShowBrowserWindowCommand_CanExecute()
        {
            return true;
        }
        public void ShowBrowserWindowCommand_Execute()
        {
            ContentsBrowserWindowShowHide?.Invoke();
        }

        #endregion

        #region == OPML ==

        public ICommand OpmlImportCommand { get; }

        public bool OpmlImportCommand_CanExecute()
        {
            return true;
        }

        public void OpmlImportCommand_Execute()
        {
            var filepath = _openDialogService.GetOpenOpmlFileDialog("Import OPML");
            if (!string.IsNullOrEmpty(filepath.Trim()))
            {
                if (File.Exists(filepath.Trim()))
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.Load(filepath.Trim());

                        MainError = null;
                    }
                    catch (Exception e)
                    {
                        MainError = new ErrorObject();
                        MainError.ErrType = ErrorObject.ErrTypes.Other;
                        MainError.ErrCode = "";
                        MainError.ErrText = e.ToString();
                        MainError.ErrDescription = e.Message;
                        MainError.ErrDatetime = DateTime.Now;
                        MainError.ErrPlace = "OpmlImportCommand_Execute.LoadXmlDoc";
                        MainError.ErrPlaceParent = "MainViewModel()";

                        IsShowMainErrorMessage = true;

                        return;
                    }

                    Opml opmlLoader = new();

                    NodeFolder dummyFolder = opmlLoader.LoadOpml(doc);
                    if (dummyFolder is not null)
                    {
                        if (dummyFolder.Children.Count > 0)
                        {
                            foreach (var feed in dummyFolder.Children)
                            {
                                feed.Parent = _services;
                                Services.Add(feed);
                            }
                        }
                    }

                    SaveServiceXml();

                    StartUpdate();
                }
            }
        }

        public ICommand OpmlExportCommand { get; }

        public bool OpmlExportCommand_CanExecute()
        {
            return true;
        }

        public void OpmlExportCommand_Execute()
        {
            Opml opmlWriter = new();

            XmlDocument xdoc = opmlWriter.WriteOpml(_services);
            if (xdoc is not null)
            {
                var filepath = _openDialogService.GetSaveOpmlFileDialog("Export OPML");

                if (!string.IsNullOrEmpty(filepath))
                {
                    xdoc.Save(filepath.Trim());
                }
            }
        }

        #endregion

        #endregion
    }
}
