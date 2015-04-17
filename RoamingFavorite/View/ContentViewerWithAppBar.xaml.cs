//---------------------------------------------------------------------
// <copyright file="ContentViewerWithAppBar.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
namespace RoamingFavorite.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using System.Collections.ObjectModel;
    using Windows.UI.Xaml.Automation;
    using Windows.UI.ViewManagement;
    using Windows.Security.Authentication.Web;
    using System.Dynamic;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Foundation;
    using RoamingFavorite.Data;
    using Windows.Security.Credentials;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Popups;
    using Windows.Storage;
    using Windows.UI.Xaml.Media.Imaging;
    using RoamingFavorite.Common;

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ContentViewerWithAppBar : RoamingFavorite.Common.LayoutAwarePage, PageCallBackInterface
    {
        private string pid = string.Empty;
        private bool _isNavigationCompleted = true;
        private DataTransferManager dataTransferManager;
        private SkyDriveFile item = null;
        private TabData tabObject = null;

        private string credsResourceUrl = null;
        private string credsPrevResourceUrl = null;

        private string username = null;
        private string password = null;

        private string jsQueryPassword = "document.querySelector('input[type=\"password\"]').value";
        private string jsQueryUserName = "document.querySelector('input[type=\"{0}\"]').value";
        private string jsQueryUserName2 = "document.querySelector('input[name=\"{0}\"]').value";
        private string[] UserNameType = {"userid", "userName", "email", "text"};
        private string[] UserName = { "userid", "userName", "email", "login" }; // this should be used with jsQueryUserName2
        private string SearchEngineBing = @"http://www.bing.com/search?q=";
        private string SearchEngineGoogle = @"https://www.google.com/#q=";
        private string selectedSearchEngine;

        //local variable to hold original Query/url entered in address/search bar
        //this variable will be used as as search string to search engine in case it neither matches a favorite nor a valid url
        private string searchQuery = string.Empty;


        // public ObservableCollection<FavDisplay> popUpListSource = new ObservableCollection<FavDisplay>();

        /// <summary>
        /// Initialize the instance of ContentViewerWithAppBar.
        /// </summary>
        public ContentViewerWithAppBar()
        {
            this.InitializeComponent();
            FavoriteBrowser.UnviewableContentIdentified += FavoriteBrowser_UnviewableContentIdentified;
            FavoriteBrowser.NavigationStarting += FavoriteBrowser_NavigationStarting;
            FavoriteBrowser.ContentLoading += FavoriteBrowser_ContentLoading;
            FavoriteBrowser.DOMContentLoaded += FavoriteBrowser_DOMContentLoaded;
            FavoriteBrowser.NavigationCompleted += FavoriteBrowser_NavigationCompleted;
            FavoriteBrowser.UnsafeContentWarningDisplaying += FavoriteBrowser_UnsafeContentWarningDisplaying;
            tabObject = MainFrameHost.tabSource.ActiveTab;
            tabObject.FrameType = this.GetType();
           // tabObject = tabObject;
            var credentialMgr = CredentialManager.GetInstance();
            credentialMgr.UserPinEvent += credentialMgr_UserPinEvent;

            this.Loaded += ContentViewerWithAppBar_Loaded;
            this.Unloaded += ContentViewerWithAppBar_Unloaded;
            switch(App.DefaultSearchEngine.ToLower())
            {
                case "bing": selectedSearchEngine = SearchEngineBing;
                    break;
                case "google": selectedSearchEngine = SearchEngineGoogle;
                    break;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            //enable button for this view
            ((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).IsEnabled = false;
            ((AppBarButton)MainFrameHost.Current.FindName("_ButtonAddFavorite")).IsEnabled = true;
            ((AppBarButton)MainFrameHost.Current.FindName("btnShowInIE")).IsEnabled = true;
            ((AppBarButton)MainFrameHost.Current.FindName("btnShowCredential")).IsEnabled = true;
            
        }

        void FavoriteBrowser_UnsafeContentWarningDisplaying(WebView sender, object args)
        {
            IsNavigationCompleted = true;
        }

        void ContentViewerWithAppBar_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= ContentViewerWithAppBar_SizeChanged;
        }

        void ContentViewerWithAppBar_Loaded(object sender, RoutedEventArgs e)
        {
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).Width = Window.Current.Bounds.Width - 380; //380 is the combined size of other controls on toolbar
            Window.Current.SizeChanged += ContentViewerWithAppBar_SizeChanged;

        }

        void ContentViewerWithAppBar_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width > 380)
            {
                ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).Width = Window.Current.Bounds.Width - 380; //380 is the combined size of other controls on toolbar
            }

            var applicationView = ApplicationView.GetForCurrentView();
            switch (App.visualState)
            {
                case AppVisualState.Filled:
                    VisualStateManager.GoToState(this, "Filled", false);
                    break;

                case AppVisualState.FullScreenLandscape:
                    VisualStateManager.GoToState(this, "FullScreenLandscape", false);
                    break;

                case AppVisualState.Snapped:
                    VisualStateManager.GoToState(this, "Snapped", false);
                    break;

                case AppVisualState.FullScreenPortrait:
                    VisualStateManager.GoToState(this, "FullScreenPortrait", false);
                    break;

                default:
                    return;
            }

        }

        void credentialMgr_UserPinEvent(object sender, PinEventArguments e)
        {
            if (e.State == Common.UserPinState.PinNotSet)
            {

                CredentialSettingsFlyout sf = new CredentialSettingsFlyout(item, credsResourceUrl);
                sf.ShowIndependent();
            }
        }

        /// <summary>
        /// Event to indicate webview is starting a navigation
        /// </summary>
        bool IsNavigationCompleted
        {
            get
            {
                return _isNavigationCompleted;
            }
            set
            {
                _isNavigationCompleted = value;
                ProgressRing.Visibility = (_isNavigationCompleted ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        /// <summary>
        /// Event is fired by webview when the content is not a webpage, such as a file download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void FavoriteBrowser_UnviewableContentIdentified(WebView sender, WebViewUnviewableContentIdentifiedEventArgs args)
        {
            IsNavigationCompleted = true;
            Windows.Foundation.IAsyncOperation<bool> b = Windows.System.Launcher.LaunchUriAsync(args.Uri);
            //pageIsLoading = false;
        }

        /// <summary>
        /// Event to indicate webview is starting a navigation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void FavoriteBrowser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if(args.Uri == null)
            {
                return;
            }
            tabObject.Url = args.Uri.ToString();
            tabObject.PageTitle = FavoriteBrowser.DocumentTitle;
            // var vault = new Windows.Security.Credentials.PasswordVault();
            bool isCredsPresent = true;
            if (!FavoriteBrowser.Source.AbsoluteUri.Contains("ms-appx-web:"))
            {
                ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = args.Uri.AbsoluteUri;// FavoriteBrowser.Source.AbsoluteUri;
            }
            credsPrevResourceUrl = args.Uri.Scheme + "://" + args.Uri.Host + "/";
            //credsPrevResourceUrl = FavoriteBrowser.Source.Scheme + "://" + FavoriteBrowser.Source.Host + "/";
            var credentialMgr = CredentialManager.GetInstance();
            var passwordCreds = credentialMgr.GetCredential(credsPrevResourceUrl);
            // vault.FindAllByResource(credsResourceUrl).FirstOrDefault();
            if (passwordCreds != null)
            {
                isCredsPresent = true;
            }
            else
            {
                isCredsPresent = false;
            }

            if (!isCredsPresent && true == await GetFravoriteBrowserWithCreds())
            {
                Flyout.ShowAttachedFlyout((FrameworkElement)sender);
            }
            IsNavigationCompleted = false;

            //btnShowCredential.IsEnabled = false;
           // FavoriteBrowser.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Event to indicate webview has resolved the uri, and that it is loading html content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void FavoriteBrowser_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            IsNavigationCompleted = true;
        }

        /// <summary>
        /// Event to indicate that the content is fully loaded in the webview. If you need to invoke script, it is best to wait for this event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void FavoriteBrowser_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            IsNavigationCompleted = true;
            //credsPrevResourceUrl = args.Uri.Scheme + "://" + args.Uri.Host + "/";
            //credsPrevResourceUrl = FavoriteBrowser.Source.Scheme + "://" + FavoriteBrowser.Source.Host + "/";
            try
            {
                var credentialMgr = CredentialManager.GetInstance();
                var passwordCreds = credentialMgr.GetCredential(args.Uri.Scheme + "://" + args.Uri.Host + "/");
                // vault.FindAllByResource(credsResourceUrl).FirstOrDefault();
                if (passwordCreds != null)
                {
                    await PopulateFravoriteBrowserWithCreds(passwordCreds.UserName, passwordCreds.Password);
                }
            }
            catch { };

            
        }

        /// <summary>
        /// Event to indicate webview has completed the navigation, either with success or failure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void FavoriteBrowser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            tabObject.PageTitle = FavoriteBrowser.DocumentTitle;
            IsNavigationCompleted = true;
            try
            {
                if (args.WebErrorStatus != Windows.Web.WebErrorStatus.CannotConnect)
                {
                    FavoriteBrowser.Visibility = Visibility.Visible;
                    credsResourceUrl = FavoriteBrowser.Source.Scheme + "://" + FavoriteBrowser.Source.Host + "/";
                    ((BitmapImage)MainFrameHost.Current.FindName("_IconImage")).UriSource = new Uri(@"http://getfavicon.appspot.com/" + System.Net.WebUtility.UrlEncode(args.Uri.AbsoluteUri) + @"?defaulticon=https://upload.wikimedia.org/wikipedia/commons/b/b9/RoamingFavoriteIcon.png");
                    tabObject.ImagePath = ((BitmapImage)MainFrameHost.Current.FindName("_IconImage")).UriSource.ToString();
                    
                }
                else
                {
                    if (!searchQuery.StartsWith(@"https://", StringComparison.CurrentCultureIgnoreCase) && !searchQuery.StartsWith(@"http://", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.FavoriteBrowser.Navigate(new Uri(selectedSearchEngine + Uri.EscapeUriString(searchQuery)));
                    }
                    else
                    {
                        NavigateBrowserToErrorpage();
                    }
                }
            }
            catch { NavigateBrowserToErrorpage(); }

            

            //if (this.FavoriteBrowser.CanGoBack)
            //{
            //    ((Button)MainFrameHost.Current.FindName("Back")).IsEnabled = true;
            //    VisualStateManager.GoToState(((Button)MainFrameHost.Current.FindName("Back")), "Normal", false);
            //}
            //else
            //{
            //    ((Button)MainFrameHost.Current.FindName("Back")).IsEnabled = false;
            //    VisualStateManager.GoToState(((Button)MainFrameHost.Current.FindName("Back")), "Disabled", true);
            //}

            //if (this.FavoriteBrowser.CanGoForward)
            //{
            //    ((Button)MainFrameHost.Current.FindName("Forward")).IsEnabled = true;
            //    VisualStateManager.GoToState(((Button)MainFrameHost.Current.FindName("Forward")), "Normal", false);
            //}
            //else
            //{
            //    ((Button)MainFrameHost.Current.FindName("Forward")).IsEnabled = false;
            //    VisualStateManager.GoToState(((Button)MainFrameHost.Current.FindName("Forward")), "Disabled", true);
            //}
        }

        /// <summary>
        /// Override the OnNavigatedFrom event of LayoutAwarePage
        /// </summary>
        /// <param name="e">Evernt arguments.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
            // Unregister the current page as a share source.
            this.dataTransferManager.DataRequested -=
                new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>
                    (this.OnDataRequested);
            //     Unhook the events
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Override the OnNavigatedFrom event of LayoutAwarePage
        /// </summary>
        /// <param name="e">Evernt arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
            item = (SkyDriveFile)e.Parameter;
            if(item == null)
            {
                item = new SkyDriveFile(tabObject.PageTitle, tabObject.Url, tabObject.ImagePath);
            }
            else
            {
                tabObject.ImagePath = item.ImagePath;
                tabObject.PageTitle = item.Title;
                tabObject.Url = item.URL; ;
            }
            string url = item.URL;
            

            // Register the current page as a share source.
            this.dataTransferManager = DataTransferManager.GetForCurrentView();
            this.dataTransferManager.DataRequested +=
                new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>
                    (this.OnDataRequested);

            if (url != null)
            {
                searchQuery = url;
                ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = url;
                Uri urlToBrowse = Utility.IsInternetURL(url);
                if (urlToBrowse != null)
                {
                    //its internet url
                    url = urlToBrowse.AbsoluteUri;
                    //update item
                    item.URL = url;
                }
                else
                {
                    url = selectedSearchEngine + Uri.EscapeUriString(url);
                }
                this.FavoriteBrowser.Navigate(new Uri(url));
                this.DataContext = url;
            }

            base.OnNavigatedTo(e);
        }

        // When share is invoked (by the user or programatically) the event handler we registered will be called to populate the datapackage with the
        // data to be shared.
        protected void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            // Call the scenario specific function to populate the datapackage with the data to be shared.
            GetShareContent(e.Request);
        }

        private async void GetShareContent(DataRequest request)
        {
            string[] urls = { "document.location.href;" };
            string url = string.Empty;
            url = await FavoriteBrowser.InvokeScriptAsync("eval", urls);
            DataPackage requestData = request.Data;
            requestData.Properties.Title = "Title";
            requestData.Properties.Description = ""; // The description is optional.
            requestData.SetWebLink(new Uri(url));
        }

        private void btnFacebookShare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
                //await FacebookLogin();
            }
            catch (Exception)
            {

            }
        }

        public async void ShowInIE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //string[] urls = { "document.location.href;" };
                //// string url = FavoriteBrowser.InvokeScriptAsync("eval", urls);
                //string url = string.Empty;
                //url = await FavoriteBrowser.InvokeScriptAsync("eval", urls);
                await Windows.System.Launcher.LaunchUriAsync(new Uri(((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText));
            }
            catch (Exception)
            {

            }
        }

        public void EditCollection_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// onCommandsRequested
        /// </summary>
        /// <param name="settingsPane"></param>
        /// <param name="e"></param>
        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            SettingsCommand defaultsCommand = new SettingsCommand("Credential Vault", "Credential Vault",
                (handler) =>
                {
                    // SettingsFlyout1 is defined in "SettingsFlyout1.xaml"
                    CredentialSettingsFlyout sf = new CredentialSettingsFlyout(item, credsResourceUrl);
                    sf.Show();
                });
            e.Request.ApplicationCommands.Add(defaultsCommand);
        }

        /// <summary>
        /// ShowCredential_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ShowCredential_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                    Button b = sender as Button;
                    if (b != null)
                    {
                        CredentialSettingsFlyout sf = new CredentialSettingsFlyout(item, credsResourceUrl);

                        //await PopulateFravoriteBrowserWithCreds(sf.UserId, sf.Password);
                        sf.ShowIndependent();
                    }
               
            }
            catch { };
        }

        /// <summary>
        /// Populate DOM with username and passowrd.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> PopulateFravoriteBrowserWithCreds(string userName, string password)
        {
            try
            {
                string jsQueryForEvalPassword = String.Format(jsQueryPassword + "=\"{0}\"", password);
                await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryForEvalPassword });
            }
            catch (Exception)
            {
                return false;
            }

            bool isAllUserTextIdsTried = false;
            //string[] strTokensForAllUserTextIds = new string[] { "userid", "userName", "email", "text"  };

            //for (int i = 0; i < strTokensForAllUserTextIds.Length && !isAllUserTextIdsTried; i++)
            //{
            //    try
            //    {
            //        string jsQueryForEvalUsername = String.Format(jsQueryUserName + "=\"{1}\"", strTokensForAllUserTextIds[i], userName);
            //        await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryForEvalUsername });
            //        isAllUserTextIdsTried = true;
            //    }
            //    catch
            //    {
            //        if (i == strTokensForAllUserTextIds.Length)
            //        {
            //            return false;
            //        }
            //    }

            //}
           
                for (int i = 0; i < this.UserName.Length && !isAllUserTextIdsTried; i++)
                {
                    try
                    {
                        string jsQueryForEvalUsername = String.Format(jsQueryUserName2 + "=\"{1}\"", UserName[i], userName);
                        await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryForEvalUsername });
                        isAllUserTextIdsTried = true;
                    }
                    catch
                    {

                    }

                }
                if (!isAllUserTextIdsTried)
                {
                    for (int i = 0; i < this.UserNameType.Length && !isAllUserTextIdsTried; i++)
                    {
                        try
                        {
                            string jsQueryForEvalUsername = String.Format(jsQueryUserName + "=\"{1}\"", UserNameType[i], userName);
                            await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryForEvalUsername });
                            isAllUserTextIdsTried = true;
                        }
                        catch
                        {

                        }
                    }
                }
           

            return true;
        }

        /// <summary>
        /// Populate DOM with username and passowrd.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> GetFravoriteBrowserWithCreds()
        {
            try
            {
                this.password = await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryPassword });
            }
            catch (Exception ex)
            {
                return false;
            }

            bool isAllUserTextIdsTried = false;
            //string[] strTokensForAllUserTextIds = new string[] { "userid", "userName", "email", "text" }; ;
            if (!string.IsNullOrEmpty(this.password))
            {
                for (int i = 0; i < this.UserNameType.Length && !isAllUserTextIdsTried; i++)
                {
                    try
                    {
                        string jsQueryForEvalUsername = String.Format(jsQueryUserName, UserNameType[i]);
                        this.username = await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryForEvalUsername });
                        isAllUserTextIdsTried = true;
                    }
                    catch
                    {

                    }

                }
                if (string.IsNullOrEmpty(this.username))
                {
                    isAllUserTextIdsTried = false;
                    for (int i = 0; i < this.UserName.Length && !isAllUserTextIdsTried; i++)
                    {
                        try
                        {
                            string jsQueryForEvalUsername = String.Format(jsQueryUserName2, UserName[i]);
                            this.username = await FavoriteBrowser.InvokeScriptAsync("eval", new string[] { jsQueryForEvalUsername });
                            isAllUserTextIdsTried = true;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(this.password) && !string.IsNullOrEmpty(this.username))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// btnSaveCred_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveCred_Click(object sender, RoutedEventArgs e)
        {
            //var vault = new Windows.Security.Credentials.PasswordVault();
            //vault.Add(new PasswordCredential(this.credsPrevResourceUrl, this.username, this.password));
            var credentialMgr = CredentialManager.GetInstance();
            bool result = credentialMgr.AddCredential(this.credsPrevResourceUrl, this.username, this.password);
            if (result)
            {
                CredsFlyout.Hide();
            }
            else
            {
                CredsFlyout.ShowAt(this.GridWebView);

            }
        }

        /// <summary>
        /// btnDontSaveCred_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDontSaveCred_Click(object sender, RoutedEventArgs e)
        {
            CredsFlyout.Hide();
        }

        /// <summary>
        /// SearchBoxEventsSuggestionsRequested
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void SearchBoxEventsSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            //DetermineSearchBoxWidth();
            var queryText = args.QueryText;
            var request = args.Request;
            var deferral = request.GetDeferral();

            try
            {
                var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
                source.GetSeachSuggestions(queryText, request.SearchSuggestionCollection);
            }
            catch (Exception)
            {

            }
            finally
            {
                deferral.Complete();
            }
        }

        public async void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            searchQuery = args.QueryText;
            if (string.IsNullOrEmpty(searchQuery))
            {
                return;
            }

            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = string.Empty;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).PlaceholderText = searchQuery;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).InvalidateArrange();

            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            SkyDriveFile fileCollection = source.GetSelectedFile(args.QueryText);
            try
            {
                //SkyDriveFolder result = await source.Search(queryText);

                if (object.Equals(fileCollection, null))
                {
                    Uri url = Utility.IsInternetURL(searchQuery);
                    if (url != null)
                    {
                        //its internet url
                        fileCollection = new SkyDriveFile(searchQuery, url.AbsoluteUri, "");
                        if(string.IsNullOrEmpty(item.UniqueId))
                        {
                            item.URL = fileCollection.URL;
                        }
                    }
                    else
                    {
                        fileCollection = new SkyDriveFile(searchQuery, selectedSearchEngine + Uri.EscapeUriString(searchQuery), "");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(fileCollection.URL))
                    {
                        await source.LoadFile(fileCollection);
                    }
                }
                ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = fileCollection.URL;
                NavigateBrowser(fileCollection.URL);
            }
            catch { };
        }

        /// <summary>
        /// SearchBoxEventsResultSuggestionChosen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SearchBoxEventsResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args)
        {
            this.FavoriteBrowser.Navigate(new Uri(selectedSearchEngine + args.Tag));
        }

        /// <summary>
        /// SearchBoxEventsLostFocus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchBoxEventsLostFocus(object sender, RoutedEventArgs e)
        {
            ((Border)MainFrameHost.Current.FindName("_IconBackground")).Visibility = Visibility.Visible;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = this.FavoriteBrowser.Source.AbsoluteUri;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).PlaceholderText = "Browse internet or search your Favorite here...";
        }

        /// <summary>
        /// SearchBoxEventsGotFocus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchBoxEventsGotFocus(object sender, RoutedEventArgs e)
        {
            //this._IconBackground.Visibility = Visibility.Collapsed;
            //this._FavouritesSearchBox.QueryText = "";
            //this._FavouritesSearchBox.PlaceholderText = "";
            //var ttv = ((SearchBox)sender).TransformToVisual(Window.Current.Content);
            //Point screenCoords = ttv.TransformPoint(new Point(0, 0));
        }

        /// <summary>
        /// NavigateBrowser
        /// </summary>
        /// <param name="url"></param>
        private void NavigateBrowser(string url, bool errorpage = false)
        {
            try
            {
                this.FavoriteBrowser.Navigate(new Uri(url));
            }
            catch (FormatException)
            {
                NavigateBrowserToErrorpage();
            }
        }

        /// <summary>
        /// NavigateBrowserToErrorpage
        /// </summary>
        private void NavigateBrowserToErrorpage()
        {
            string localErrorFile = "ms-appx-web:///Assets/error.html";
            this.FavoriteBrowser.Navigate(new Uri(localErrorFile));
        }

        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            if (FavoriteBrowser.CanGoBack)
            {
                //navigate back within web page
                this.FavoriteBrowser.GoBack();
            }
            else
            {
                //navigate back to groupeditempage
                if(tabObject.TabFrame.CanGoBack)
                {
                    tabObject.TabFrame.GoBack();
                }
            }
        }

        public bool CanGoBack
        {
            get
            {
                return (FavoriteBrowser.CanGoBack || tabObject.FolderStack.Count > 1);
            }
        }

        public void GoBack(object sender, RoutedEventArgs e)
        {
            this.NavigateBack(sender, e);
        }

        private void NavigateForward(object sender, RoutedEventArgs e)
        {
            if (FavoriteBrowser.CanGoForward)
            {
                this.FavoriteBrowser.GoForward();
            }
        }

        private void PlaceAddBookmarkPopup(object sender)
        {
            var ttv = ((Button)sender).TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            this.AddBookmarkPopup.VerticalOffset = ((Grid)MainFrameHost.Current.FindName("_FavoritesBar")).ActualHeight;//Window.Current.Bounds.Height - (BottomAppBar.ActualHeight + this.AddBookMarkBorder.Height);
            if (App.visualState == AppVisualState.Snapped)
            {
                this.AddBookMarkBorder.Height = 250;
                this.AddBookMarkBorder.Width = 300;
            }

            if ((Window.Current.Bounds.Width - screenCoords.X) < this.AddBookMarkBorder.Width)
            {
                this.AddBookmarkPopup.HorizontalOffset = Window.Current.Bounds.Width - this.AddBookMarkBorder.Width - 10; //10 for margin

            }
            else
            {
                this.AddBookmarkPopup.HorizontalOffset = screenCoords.X + (((Button)sender).ActualWidth - this.AddBookMarkBorder.Width) / 2 - 10;
            }
            if (this.AddBookmarkPopup.HorizontalOffset < 0)
            {
                this.AddBookmarkPopup.HorizontalOffset = 10;
            }
            this.AddBookmarkPopup.IsOpen = true;
        }

        public void ButtonAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            PlaceAddBookmarkPopup(sender);
            this.AddBookmarkName.Text = FavoriteBrowser.DocumentTitle;
            this.AddBookmarkURL.Text = FavoriteBrowser.Source.AbsoluteUri;
        }

        private async void PopupAddBookmark_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.AddBookmarkPopup.IsOpen = false;
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            if (this.AddBookmarkURL.Text.Trim() != string.Empty)
            {
                SkyDriveFile file = null;
                file = source.CurrentFolder.Items.FirstOrDefault(sf => sf.Title == AddBookmarkName.Text.Trim());
                // add url only if does not already exisits
                if (object.Equals(file, null))
                {
                    file = new SkyDriveFile(AddBookmarkName.Text.Trim(), AddBookmarkURL.Text.Trim(), string.Empty);
                    file.Parent_id = source.CurrentFolder.UniqueId;
                    await source.AddFile(file);
                    item = file;
                    //source.CurrentFolder.Items.Add(file);
                }
                this.AddBookmarkName.Text = string.Empty;
                this.AddBookmarkURL.Text = string.Empty;
                this.ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

        }
    }
}