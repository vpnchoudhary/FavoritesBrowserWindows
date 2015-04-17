using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Resources;
using Windows.UI.ApplicationSettings;
using Microsoft.Live;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using RoamingFavorite.Data;
using RoamingFavorite.Common;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;


// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace RoamingFavorite.View
{

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedItemsPage : RoamingFavorite.Common.LayoutAwarePage, PageCallBackInterface
    {
        static bool bClipBoardURLNotificationSeen = false;
        List<SkyDriveDataCommon> selectedItems = null;
        CoreApplicationView appView = null;
        int viewid;
        TabData tabObject = null;

        public GroupedItemsPage()
        {
            this.InitializeComponent();
            appView = CoreApplication.CreateNewView();
            
            SkyDriveDataSource.newSignupEvent += new EventHandler<NewSingupEventArguments>(NewSignupEventHandler);
            Clipboard.ContentChanged += new EventHandler<object>(ClipBoardContentChangedEventHandler);
            SkyDriveDataCommon.ItemSelectionEvent += new EventHandler<ItemSelectionEventArguments>(ItemSelectionEventHander);
            tabObject = MainFrameHost.tabSource.ActiveTab;
            tabObject.FrameType = this.GetType();
            tabObject.PageTitle = "Favorites";
            tabObject.Url = string.Empty;
            tabObject.ImagePath = @"ms-appx:///Assets/fileIcon.png";
            selectedItems = new List<SkyDriveDataCommon>();
            this.Loaded += GroupedItemsPage_Loaded;
            this.Unloaded += GroupedItemsPage_Unloaded;
            object var  = SkyDriveDataSource.GetSetting(constants.PreferenceOpenInIE);
            if(Convert.ToString(var).ToLower() == "true")
            {
                App.OpenWebPageinIE = true;
            }
            else
            {
                App.OpenWebPageinIE = false;
            }

            object searchEng = SkyDriveDataSource.GetSetting(constants.SearchEngine);
            if (searchEng != null)
            {
                App.DefaultSearchEngine = Convert.ToString(searchEng);
            }
        }

        private void ItemSelectionEventHander(object sender, ItemSelectionEventArguments e)
        {
            if(e.SelectedItem.IsChecked)
            {
                if (!this.selectedItems.Any(skItem => skItem == e.SelectedItem))
                {
                    this.selectedItems.Add(e.SelectedItem);
                }
            }
            else
            {
                if (this.selectedItems.Any(skItem => skItem == e.SelectedItem))
                {
                    this.selectedItems.Remove(e.SelectedItem);
                }
            }
            this.DetermineBottomAppBarVisibility();
        }

        void GroupedItemsPage_Loaded(object sender, RoutedEventArgs e)
        {
            //var ttv = this.glyph.TransformToVisual(Window.Current.Content);
            //Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            //this.mySearchBox.Width = Window.Current.Bounds.Width - (screenCoords.X + 20);
            
             Window.Current.SizeChanged += GroupedItemsPage_SizeChanged;
            RegisterForShare();
            DetermineVisualState();
        }

        

        void GroupedItemsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= GroupedItemsPage_SizeChanged;
            UnRegisterForShare();
        }

        private void DetermineVisualState()
        {
            selectedItems.RemoveRange(0, selectedItems.Count);
            var applicationView = ApplicationView.GetForCurrentView();
            var size = Window.Current.Bounds;

            if (applicationView.IsFullScreen)
            {
                if (applicationView.Orientation == ApplicationViewOrientation.Landscape)
                    App.visualState = AppVisualState.FullScreenLandscape;
                else
                    App.visualState = AppVisualState.FullScreenPortrait;
            }
            else
            {
                if (size.Width == 320)
                {
                    App.visualState = AppVisualState.Snapped;
                }
                else if (size.Width <= 500)
                    App.visualState = AppVisualState.Narrow;
                else
                    App.visualState = AppVisualState.Filled;
            }
            VisualStateManager.GoToState(this, App.visualState.ToString(), true);
            DetermineBottomAppBarVisibility();
            DetermineSearchBoxWidth();            
        }

        void GroupedItemsPage_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            DetermineVisualState();
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            SettingsCommand help = new SettingsCommand("Help", "Help",
                (handler) =>
                {
                    HelpSettingsFlyout helpSetting = new HelpSettingsFlyout();
                    helpSetting.Show();
                });
            SettingsCommand tour = new SettingsCommand("Tour", "Tour",
               (handler) =>
               {
                   this.Frame.Navigate(typeof(Promo_Sync), false);
               });

            SettingsCommand options = new SettingsCommand("Options", "Options",
                (handler) =>
                {
                    OptionsSettingFlyout sf = new OptionsSettingFlyout();
                    sf.Show();
                });
            SettingsCommand credentialVault = new SettingsCommand("CredentialVault", "CredentialVault",
                (handler) =>
                {
                    //MainFrameHost.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>{this._FavouritesSearchBox
                    ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = false;
                    //_ this.mySearchBox.FocusOnKeyboardInput = false;
                    if (selectedItems.Count == 1 && selectedItems[0].Type == SkyDriveItemType.File)
                    {
                        CredentialSettingsFlyout sf = new CredentialSettingsFlyout((SkyDriveFile)selectedItems[0], ((SkyDriveFile)selectedItems[0]).URL);
                        sf.Show();
                    }
                });
            SettingsCommand feedback = new SettingsCommand("Feedback", "Feedback",
               (handler) =>
               {
                   var mailto = new Uri("mailto:?to=favoritesbrowser@outlook.com&subject=Feedback RoamingFavorites App&body=");
                   Windows.System.Launcher.LaunchUriAsync(mailto);        
               });
            SettingsCommand About = new SettingsCommand("About", "About",
               (handler) =>
               {

                   AboutSettingsFlyout about = new AboutSettingsFlyout();
                   about.Show();
               });
            e.Request.ApplicationCommands.Add(credentialVault);
            e.Request.ApplicationCommands.Add(options);
            e.Request.ApplicationCommands.Add(help);
            e.Request.ApplicationCommands.Add(tour);
            e.Request.ApplicationCommands.Add(feedback);
            e.Request.ApplicationCommands.Add(About);
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
            base.OnNavigatedFrom(e);
        }
        private void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareHtmlHandler);
        }

        private void UnRegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= this.ShareHtmlHandler;
        }


        private void ShareHtmlHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBShareLinks"));
            string localImage = "ms-appx:///Assets/FolderIcon.png";
            DataPackage request = e.Request.Data;
            request.Properties.Title = "Bookmark list";
            request.Properties.Description = "Shared Bookmarks";
            request.Properties.ApplicationName = "Favorites Browser";
            if(selectedItems.Count > 0)
            {
                request.SetDataProvider(StandardDataFormats.Html, new DataProviderHandler(this.OnDeferredImageRequestedHandler));
                Windows.Storage.Streams.RandomAccessStreamReference streamRef = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(localImage));
                request.ResourceMap[localImage] = streamRef;
            }
            else
            {
                e.Request.FailWithDisplayText("Select bookmarks for sharing.");
            }
        }

        private async void OnDeferredImageRequestedHandler(DataProviderRequest request)
        {
            // In this delegate we provide updated Bitmap data using delayed rendering.
            if (this.selectedItems.Count > 0)
            {
                // If the delegate is calling any asynchronous operations it needs to acquire
                // the deferral first. This lets the system know that you are performing some
                // operations that might take a little longer and that the call to SetData 
                // could happen after the delegate returns. Once you acquired the deferral object 
                // you must call Complete on it after your final call to SetData.
                DataProviderDeferral deferral = request.GetDeferral();
                
                // Make sure to always call Complete when finished with the deferral.
                try
                {
                    // Decode the image and re-encode it at 50% width and height.
                    string html = await new SharedLinkProvider().GetHTML(this.selectedItems);

                    request.SetData(html);
                    
                }
                finally
                {
                    deferral.Complete();
                }
            }
        }

        async void ClipBoardContentChangedEventHandler(object sender, object e)
        {
            string clipboardUrl = await CheckClipBoardForContent();
            if (string.Empty != clipboardUrl)
            {
                this.clipBoardNotification.Text = clipboardUrl;
                this.clipboardUrlMessageGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.BottomAppBar.IsOpen = true;
                bClipBoardURLNotificationSeen = true;
                this.bottonCommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

        }

        void NewSignupEventHandler(object sender, NewSingupEventArguments e)
        {
            if (e.State == Common.NewSignupState.NewUser)
            {
                SkyDriveDataSource.DeleteContainer();
                this.NewSetup.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.NewSetup.Text = "Please wait while we set up your favorites folder on skydrive...";
            }
            else if (e.State == Common.NewSignupState.FavoriteFolderCreated)
            {
                this.Frame.Navigate(typeof(Promo_Sync), true);
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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (App.OpenWebPageinIE)
            {
                //creating a dummy view for opening on space created by this window.
                await appView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    viewid = ApplicationView.GetApplicationViewIdForWindow(CoreWindow.GetForCurrentThread());
                    var frame = new Frame();
                    Window.Current.Content = frame;
                });
            }

            //enable button for this view
            ((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).IsEnabled = true;
            ((AppBarButton)MainFrameHost.Current.FindName("_ButtonAddFavorite")).IsEnabled = false;
            ((AppBarButton)MainFrameHost.Current.FindName("btnShowInIE")).IsEnabled = false;
            ((AppBarButton)MainFrameHost.Current.FindName("btnShowCredential")).IsEnabled = false;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = "";
            ((BitmapImage)MainFrameHost.Current.FindName("_IconImage")).UriSource = new Uri(@"ms-appx:///Assets/fileIcon.png");

            //FavoriteFileStore fileStore = await FavoriteFileStore.GetInstance();
           
            groupedItemsViewSource.Source = null;
            //backButton.IsEnabled = CanGoBack;
            //if(!Utility.IsConnectedToInternet())
            //{
            //    MessageDialog dialog = new MessageDialog("Please check your internet connection settings and try again.", "Unable to connect to the Internet");
            //    await dialog.ShowAsync();
            //    return;
            //}
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            FavoritesDataSyncManager source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            // set user tile and user name text
            //imgProfile.Source = new BitmapImage(new Uri(source.ImageUrl));
            //string firstName = source.UserName.Split(' ')[0];
            //firstName = firstName.ToLower();
            //string firstChar = firstName.Substring(0, 1);
            //firstName = firstName.Remove(0, 1);
            //firstName = firstName.Insert(0, firstChar.ToUpper());
            //this.DefaultViewModel["UserName"] = firstName + "'s";
            this.DefaultViewModel["SelectedFolderName"] = "Bookmarks";
            //txtUserName.Text = 
            if (tabObject.FolderStack.Count == 0)
            {
                source.CurrentFolder = await source.GetRoot();
                groupedItemsViewSource.Source = source.CurrentFolder.AllItems;
                tabObject.FolderStack.Add(source.CurrentFolder);
                //we are at root of Bookmark
                this.DefaultViewModel["SelectedFolderName"] = source.CurrentFolder.Title;
                if(groupedItemsViewSource.View.Count == 0)
                {
                    // user have no bookmarks, show suggestion
                    this.Frame.Navigate(typeof(Promo_Sync), true);
                    //this.Frame.Navigate(typeof(FavoriteSuggestions));
                }
            }
            else
            {
                 source.CurrentFolder = await source.GetCollection(tabObject.FolderStack[tabObject.FolderStack.Count - 1]);
                 groupedItemsViewSource.Source = source.CurrentFolder.AllItems;
                //if (tabObject.FolderStack.Count > 1)
                //{
                    this.DefaultViewModel["SelectedFolderName"] = source.CurrentFolder.Title;
                    //this.glyph.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //}
            }
            //read clipboard first time after start of app. other clipboard scenario's will be handled by 
            //ClipBoardContentChangedEventHandler delegate
            //bottom appbar will not come again if user have seen it once unless user copy a new url in clipboard.
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            string clipboardUrl = await CheckClipBoardForContent();
            if (string.Empty != clipboardUrl)
            {
                this.clipBoardNotification.Text = clipboardUrl;
                this.clipboardUrlMessageGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.bottonCommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (!bClipBoardURLNotificationSeen)
                {
                    this.BottomAppBar.IsOpen = true;
                }
                bClipBoardURLNotificationSeen = true;
            }

            // we want to revert back all items to non-editable state
            await RevertEditState();
            if(source.CurrentFolder.AllItems.Count<SkyDriveDataCommon>() == 0)
            {
                BottomAppBar.IsOpen = true;
            }


        }
        private async Task<string> CheckClipBoardForContent()
        {
            string url = string.Empty;

            try
            {
                DataPackageView dataPackage = Clipboard.GetContent();
                if (dataPackage.Contains(StandardDataFormats.Text))
                {
                    Uri test = Utility.IsInternetURL(await dataPackage.GetTextAsync(), true);
                    //Uri test = new Uri(await dataPackage.GetTextAsync(), UriKind.Absolute);

                    if (test != null)
                    {
                        url = test.AbsoluteUri;
                    }
                }
            }
            catch (Exception ex) { ex.ToString(); }
            return url;
        }

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {

            //var group = (sender as FrameworkElement).DataContext;

            //var matches = SkyDriveDataSource.GetGroup(((SkyDriveDataCommon)group).UniqueId);
            //groupedItemsViewSource.Source = matches;
            //backButton.IsEnabled = CanGoBack;
        }

        /// <summary>
        /// Invoked when an item within a group is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //groupedItemsViewSource.Source = null;
            await RevertEditState();
            selectedItems.RemoveRange(0, selectedItems.Count);
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            FavoritesDataSyncManager source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            if(App.visualState == AppVisualState.Snapped)
            {
                this.itemListView.SelectionMode = ListViewSelectionMode.None;
                
                BottomAppBar.IsOpen = false;
            }
            try
            {
                if (((SkyDriveDataCommon)e.ClickedItem).Type == SkyDriveItemType.Folder)
                {
                    this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    var folder = (SkyDriveFolder)e.ClickedItem;
                    folder = await source.GetCollection(folder);
                    groupedItemsViewSource.Source = folder.AllItems;  //matches.AllItems;
                    tabObject.FolderStack.Add(folder);
                    //backButton.IsEnabled = CanGoBack;
                    source.CurrentFolder = folder;
                    //this.glyph.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.DefaultViewModel["SelectedFolderName"] = source.CurrentFolder.Title;
                }
                else if (((SkyDriveFile)e.ClickedItem).FileExtenstion.ToLower() == ".url")
                {
                    string url = ((SkyDriveFile)e.ClickedItem).URL;
                    if (!url.Equals(string.Empty))
                    {
                        //open webpage in IE if either app windows in snap mode or user have selected open all Webs page in IE 
                        if (App.OpenWebPageinIE || App.visualState == AppVisualState.Snapped || App.visualState == AppVisualState.Narrow)
                        {
                            //resize app windows into snap mode if openWebPageinIE is true
                            if (App.visualState != AppVisualState.Snapped)
                            {
                                var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                                     viewid,
                                     ViewSizePreference.UseMore,
                                     ApplicationView.GetForCurrentView().Id,
                                     ViewSizePreference.UseMinimum);
                                this.itemListView.SelectionMode = ListViewSelectionMode.Single;
                                this.itemListView.SelectedItem = (SkyDriveDataCommon)e.ClickedItem;
                                this.itemListView.ScrollIntoView(this.itemListView.SelectedItem, ScrollIntoViewAlignment.Leading);
                            }
                            else
                            {
                                // we don't want to move selected item to top of  list view in case of normal browsing in snapped mode
                                this.itemListView.SelectionMode = ListViewSelectionMode.Single;
                                this.itemListView.SelectedItem = (SkyDriveDataCommon)e.ClickedItem;
                            }
                            selectedItems.Add((SkyDriveDataCommon)e.ClickedItem);
                            DetermineBottomAppBarVisibility();    
                            Uri uri = new Uri(((SkyDriveFile)e.ClickedItem).URL);
                            await Windows.System.Launcher.LaunchUriAsync(uri);
                        }
                        else
                        {
                            this.Frame.Navigate(typeof(ContentViewerWithAppBar), e.ClickedItem);
                        }
                    }
                }
            }catch(Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                //source.CurrentFolder = await source.GetCollection(tabObject.FolderStack[tabObject.FolderStack.Count - 1]);
                groupedItemsViewSource.Source = source.CurrentFolder.AllItems;
                this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (App.OpenWebPageinIE && appView != null)
                {
                    ApplicationViewSwitcher.SwitchAsync(ApplicationView.GetForCurrentView().Id,
                    viewid,
                    ApplicationViewSwitchingOptions.ConsolidateViews);
                }
            }
        }

        public bool CanGoBack
        {
            get
            {
                if (tabObject.FolderStack.Count > 1)
                {
                    return true;
                }
                else
                { return false; }
            }
        }
        public async new void GoBack(object sender, RoutedEventArgs e)
        {
            selectedItems.RemoveRange(0, selectedItems.Count);
            if (App.visualState == AppVisualState.Snapped)
            {
                this.itemListView.SelectionMode = ListViewSelectionMode.None;
                selectedItems.RemoveRange(0, selectedItems.Count);
                BottomAppBar.IsOpen = false;
            }
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).PlaceholderText = string.Empty;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = string.Empty;
            DetermineSearchBoxWidth(true);
            groupedItemsViewSource.Source = null;
            this.noResultsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            FavoritesDataSyncManager source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            // Use the navigation frame to return to the previous page
            if (tabObject.FolderStack.Count > 1)
            {
                source.CurrentFolder = await source.GetCollection(tabObject.FolderStack[tabObject.FolderStack.Count - 2]);
                groupedItemsViewSource.Source = source.CurrentFolder.AllItems;
                tabObject.FolderStack.RemoveAt(tabObject.FolderStack.Count - 1);
                this.DefaultViewModel["SelectedFolderName"] = source.CurrentFolder.Title;
            }
            if (tabObject.FolderStack.Count == 1)
            {
                source.CurrentFolder = await source.GetCollection(tabObject.FolderStack[0]);
                groupedItemsViewSource.Source = source.CurrentFolder.AllItems;
                this.DefaultViewModel["SelectedFolderName"] = source.CurrentFolder.Title;
            }
            await RevertEditState();

            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async Task RevertEditState()
        {
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            FavoritesDataSyncManager source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            // we want to revert back all items to non-editable state
            foreach (var item in source.CurrentFolder.AllItems)
            {
                item.IsChecked = false;
                item.ShowCheckBox = false;
            }
            ((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).Visibility = Windows.UI.Xaml.Visibility.Visible;
             ((AppBarButton)MainFrameHost.Current.FindName("collectionCancelButton")).Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void AddCopiedURL_Click(object sender, RoutedEventArgs e)
        {
            this.CreateBookmarkButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.UpdateBookmarkButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.AddBookmarkURL.Text = this.clipBoardNotification.Text;
            this.AddBookmarkURL.IsReadOnly = true;
            PlaceAddBookmarkPopup(sender);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            //groupedItemsViewSource.Source = await source.Search("url");
        }

        public async void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            if(string.IsNullOrEmpty(args.QueryText))
            {
                //prepare search box for search by increasing its width. only for snapped mode. 
                DetermineSearchBoxWidth(false, true);
                return;
            }
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).QueryText = string.Empty;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).PlaceholderText = args.QueryText;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).InvalidateArrange();
            if (App.visualState == AppVisualState.Snapped )
            {
                var queryText = args.QueryText;
                try
                {
                    var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
                    SkyDriveFile fileCollection = source.GetSelectedFile(args.QueryText);
                    if (fileCollection != null)
                    {
                        Uri uri = new Uri(fileCollection.URL);
                        await Windows.System.Launcher.LaunchUriAsync(uri);
                    }
                }
                catch { };
            }
            else
            {
                var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
                SkyDriveFile fileCollection = source.GetSelectedFile(args.QueryText);
                try
                {
                    if (object.Equals(fileCollection, null))
                    {
                         // let ContentViewer do the heavy lifting
                         fileCollection = new SkyDriveFile(args.QueryText, args.QueryText, "");
                       
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(fileCollection.URL))
                        {
                            await source.LoadFile(fileCollection);
                        }
                        //fileCollection.URL = System.Net.WebUtility.UrlEncode(fileCollection.URL);
                    }
                    this.Frame.Navigate(typeof(ContentViewerWithAppBar), fileCollection);
                }
                catch { };
            }
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


        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            this.clipBoardNotification.Text = string.Empty;
            this.clipboardUrlMessageGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.bottonCommandBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Clipboard.Clear();
            bClipBoardURLNotificationSeen = false;
            this.BottomAppBar.IsOpen = false;
        }


        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBDelete"));
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            if (selectedItems.Count > 0)
            {
                await source.DeleteItem(selectedItems);
                await UIRefreshWithoutFetch();
            }
            selectedItems.Clear();
            DetermineBottomAppBarVisibility();
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

        }

        private async Task UIRefreshWithoutFetch()
        {
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            FavoritesDataSyncManager source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            source.CurrentFolder = await source.GetCollection(source.CurrentFolder);
            this.groupedItemsViewSource.Source = source.CurrentFolder.AllItems;
        }

        private async Task UIRefresh()
        {
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = true;
            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            //source.CurrentFolder = await source.Refresh(source.CurrentFolder.UniqueId);
            //tabObject.FolderStack.RemoveAt(tabObject.FolderStack.Count - 1);
            //tabObject.FolderStack.Add(source.CurrentFolder);
            //this.groupedItemsViewSource.Source = source.CurrentFolder.AllItems;

            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            await source.Sync();
            await UIRefreshWithoutFetch();
        }

        private async void EditAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBEditFolder"));
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            if (selectedItems.Count > 0)
            {
                ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = false;
                //edit the first selected item
                if (selectedItems[0].Type == SkyDriveItemType.File)
                {
                    //Modify Add file popup
                    this.CreateBookmarkButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.UpdateBookmarkButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.AddBookmarkName.Text = ((SkyDriveFile)this.selectedItems[0]).Title;
                    this.AddBookmarkURL.Text = ((SkyDriveFile)this.selectedItems[0]).URL;
                    this.AddBookmarkURL.IsReadOnly = true;
                    // open Add File popup
                    PlaceAddBookmarkPopup(sender);
                }
                else
                {
                    //Modify New Folder popup
                    this.CreateFolderName.Text = ((SkyDriveFolder)this.selectedItems[0]).Title;
                    this.CreateFolderButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.UpdateFolderButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    //open create folder pop
                    PlaceCreateFolderPopUP(sender);
                }
            }
        }

        private void PlaceAddBookmarkPopup(object sender)
        {
            var ttv = ((Button)sender).TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            this.AddBookmarkPopup.VerticalOffset = Window.Current.Bounds.Height - (BottomAppBar.ActualHeight + this.AddBookMarkBorder.Height);
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

        private void PlaceCreateFolderPopUP(object sender)
        {
            var ttv = ((Button)sender).TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            this.CreateFolderPopUP.VerticalOffset = Window.Current.Bounds.Height - (BottomAppBar.ActualHeight + 150);
            if ((Window.Current.Bounds.Width - screenCoords.X) < 250)
            {
                this.CreateFolderPopUP.HorizontalOffset = Window.Current.Bounds.Width - 250 - 10; //10 for margin

            }
            else
            {
                this.CreateFolderPopUP.HorizontalOffset = screenCoords.X + (((Button)sender).ActualWidth - 250) / 2 - 10;
            }
            if (this.CreateFolderPopUP.HorizontalOffset < 0)
            {
                this.CreateFolderPopUP.HorizontalOffset = 10;
            }

            this.CreateFolderPopUP.IsOpen = true;

        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = false;
            this.CreateBookmarkButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.UpdateBookmarkButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.AddBookmarkURL.IsReadOnly = false;
            PlaceAddBookmarkPopup(sender);
        }

        private void NewFolderAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = false;
            this.CreateFolderButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.UpdateFolderButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PlaceCreateFolderPopUP(sender);
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            //count in this while loop will decrease each time ischecked is set. 
            while(this.selectedItems.Count > 0 )
            {
                this.selectedItems[0].IsChecked = false;
            }
            this.selectedItems.Clear();
            this.BottomAppBar.IsSticky = false;
            this.BottomAppBar.IsOpen = false;
            this.buttonClearSelection.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.itemGridView.SelectedIndex = -1;
            
        }

        private void AppBar_Opened(object sender, object e)
        {
            AppVisualState state = App.visualState;
            if(state == AppVisualState.Filled)
            {
                if(Window.Current.Bounds.Width < 786)
                {
                    state = AppVisualState.Narrow;
                }
            }
            switch(state)
            {
                case AppVisualState.FullScreenLandscape:
                case AppVisualState.FullScreenPortrait:
                case AppVisualState.Filled:
                    this.syncButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.SettingAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.AddAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.AddFolderAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.buttonClearSelection.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.LeftCommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.webAccount.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.creditCard.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    if (selectedItems.Count > 0)
                    {

                        this.buttonClearSelection.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.LeftCommandBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.AddBookmarkName.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        if (selectedItems.Count == 1 && selectedItems[0].Type == SkyDriveItemType.File)
                        {
                            this.webAccount.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            this.creditCard.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                    break;
                case AppVisualState.Snapped:
                case AppVisualState.Narrow:
                    this.syncButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.SettingAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.webAccount.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.creditCard.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.buttonClearSelection.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.LeftCommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.AddFolderAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.AddAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        if (selectedItems.Count > 0)
                        {

                            if (selectedItems.Count == 1 && selectedItems[0].Type == SkyDriveItemType.File)
                            {
                                this.webAccount.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            }
                        }
                    break;
            }
            //if user is performing search then do no allow right command bar (folder and file creation)
            if (tabObject.FolderStack.Count > 0)
            {
                if (tabObject.FolderStack[tabObject.FolderStack.Count - 1].UniqueId.Equals(string.Empty))
                {
                    this.AddFolderAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.AddAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.syncButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {

                foreach (dynamic item in e.AddedItems)
                {
                    if (!this.selectedItems.Any(skItem => skItem==item))
                    {
                        selectedItems.Add(item as SkyDriveDataCommon);
                        ((SkyDriveDataCommon)item).IsChecked = true;
                    }
                }
            }
            if (e.RemovedItems.Count > 0)
            {
                foreach (dynamic item in e.RemovedItems)
                {
                    if (this.selectedItems.Any(skItem => skItem == item))
                    {
                        selectedItems.Remove(item);
                        ((SkyDriveDataCommon)item).IsChecked = false;
                    }
                }
            }
            DetermineBottomAppBarVisibility();
        }

        //Bottom appbar will be visible once user has selected any gridview item. But edit button will be disabled once user select more than more items.

        private void DetermineBottomAppBarVisibility()
        {
            switch (selectedItems.Count)
            {
                case 0: this.BottomAppBar.IsOpen = false;
                    this.BottomAppBar.IsSticky = false;
                    break;
                case 1: this.EditAppBarButton.IsEnabled = true;
                    this.BottomAppBar.IsOpen = true;
                    this.BottomAppBar.IsSticky = true;
                    break;
                default: this.EditAppBarButton.IsEnabled = false;
                    this.BottomAppBar.IsOpen = true;
                    this.BottomAppBar.IsSticky = true;
                    break;
            }
        }

        private async void PopupCreateFolder_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBCreateFolder"));
            this.CreateFolderPopUP.IsOpen = false;
            this.BottomAppBar.IsOpen = false;
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            if (this.CreateFolderName.Text.Trim() != string.Empty)
            {
                var skFolder = source.CurrentFolder.Subalbums.FirstOrDefault(sf => sf.Title.ToLower() == this.CreateFolderName.Text.Trim().ToLower());
                if (object.Equals(skFolder, null))
                {
                    SkyDriveFolder folder = new SkyDriveFolder(this.CreateFolderName.Text.Trim());
                    folder.Parent_id = source.CurrentFolder.UniqueId;
                    folder.UniqueId = await source.AddFolder(folder);
                    //source.CurrentFolder.Subalbums.Add(folder);
                }
                this.CreateFolderName.Text = string.Empty;
            }
            await UIRefreshWithoutFetch();
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = true;
        }

        private async void PopupAddBookmark_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBAddOpr"));
            if (this.clipBoardNotification.Text != string.Empty)
            {
                this.clipBoardNotification.Text = string.Empty;
                this.clipboardUrlMessageGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.bottonCommandBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Clipboard.Clear();
                bClipBoardURLNotificationSeen = false;
            }
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.AddBookmarkPopup.IsOpen = false;
            this.BottomAppBar.IsOpen = false;
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
                    //source.CurrentFolder.Items.Add(file);
                }
                this.AddBookmarkName.Text = string.Empty;
                this.AddBookmarkURL.Text = string.Empty;
            }
            await UIRefreshWithoutFetch();
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = true;
        }

        private async void PopupUpdateFolder_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBUpdateOpr"));
            this.CreateFolderPopUP.IsOpen = false;
            this.BottomAppBar.IsOpen = false;
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);

            if (this.CreateFolderName.Text.Trim() != string.Empty && selectedItems[0].Type == SkyDriveItemType.Folder)
            {
                var skFolder = source.CurrentFolder.Subalbums.FirstOrDefault(sf => sf.Title.ToLower() == this.CreateFolderName.Text.Trim().ToLower());
                if (skFolder== null)
                {
                    selectedItems[0].UniqueId = await source.EditFolder(selectedItems[0].UniqueId, this.CreateFolderName.Text.Trim());
                    selectedItems[0].Title = this.CreateFolderName.Text.Trim();
                }
                this.CreateFolderName.Text = string.Empty;
            }
            await UIRefreshWithoutFetch();
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = true;

        }

        private async void PopupUpdateBookmark_Click(object sender, RoutedEventArgs e)
        {
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.AddBookmarkPopup.IsOpen = false;
            this.BottomAppBar.IsOpen = false;
            var source = await SkyDriveDataSource.GetInstance();
            if (this.AddBookmarkName.Text.Trim() != string.Empty)
            {
                this.selectedItems[0].Title = AddBookmarkName.Text.Trim();
                bool result = await source.UpdateFile((SkyDriveFile)this.selectedItems[0]);
                this.AddBookmarkName.Text = string.Empty;
                this.AddBookmarkURL.Text = string.Empty;
            }
            await UIRefreshWithoutFetch();
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = true;
        }

        private async void RefreshAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await UIRefresh();
            this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void MoveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var ttv = ((Button)sender).TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            this.SelectDestinationPopup.VerticalOffset = Window.Current.Bounds.Height - (BottomAppBar.ActualHeight + 450);
            if ((Window.Current.Bounds.Width - screenCoords.X) < 550)
            {
                this.SelectDestinationPopup.HorizontalOffset = Window.Current.Bounds.Width - 400 - 10; //10 for margin

            }
            else
            {
                this.SelectDestinationPopup.HorizontalOffset = screenCoords.X + (((Button)sender).ActualWidth - 400) / 2 - 10;
            }
            if (this.SelectDestinationPopup.HorizontalOffset < 0)
            {
                this.SelectDestinationPopup.HorizontalOffset = 10;
            }
             await this.SelectFolderUserCntl.LoadFolder();
            this.SelectDestinationPopup.IsOpen = true;

            //select destination path
            // move one item at a time
        }

        private async void FolderSelected_Click(object sender, SkyDriveFolder e)
        {
            SkyDriveFolder destination = null;
            if (e != null)
            {
                //move items 
                destination = e;
                this.SelectDestinationPopup.IsOpen = false;
                this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
                try
                {
                    await source.MoveItem(selectedItems, destination);
                }
                catch { };
                selectedItems.Clear();
                await UIRefreshWithoutFetch();
                this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                
            }

        }

        private async void SelectDestinationPopup_Loaded(object sender, RoutedEventArgs e)
        {
           // await this.SelectFolderUserCntl.LoadFolder();
        }

        private void MailAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //create Html for selected favorite links
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();

        }

        public  void SearchBoxEventsLostFocus(object sender, RoutedEventArgs e)
        {
           // this.mySearchBox.IsEnabled = false;
        }

        public void SearchBoxEventsResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args)
        {
            //DetermineSearchBoxWidth();
            //this.mySearchBox.Focus(FocusState.Unfocused);
        }

        public void SearchBoxEventsGotFocus(object sender, RoutedEventArgs e)
        {
           // mySearchBox.FocusOnKeyboardInput = true;
        }

        

        public void ButtonAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void ShowInIE_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void ShowCredential_Click(object sender, RoutedEventArgs e)
        {
            

        }
        
        private void DetermineSearchBoxWidth(bool backButtonHit=false, bool increaseSearchWidth = false)
        {
            //this.collectionEditButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //this.pageTitle.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //mySearchBox.Height = 35;
            //if (App.visualState == AppVisualState.FullScreenLandscape || App.visualState == AppVisualState.FullScreenPortrait || App.visualState == AppVisualState.Filled)
            //{
            //    this.collectionEditButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //    var ttv = ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).TransformToVisual(Window.Current.Content);
            //    Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            //    if (App.visualState == AppVisualState.FullScreenLandscape)
            //    {
            //        mySearchBox.Width = Window.Current.Bounds.Width - (screenCoords.X + this.pageTitle.DesiredSize.Width + 40 + 50); //50 for edit button
            //    }
            //    else
            //    {
            //        mySearchBox.Width = Window.Current.Bounds.Width - (screenCoords.X + this.pageTitle.DesiredSize.Width + 40 +50);
            //    }
            //}
            //else if (backButtonHit)
            //{
            //    mySearchBox.Width = 30;
            //    mySearchBox.Height = 30;
            //}
            //else if (mySearchBox.Width < 40 )
            //{
            //    var ttv = this.mySearchBox.TransformToVisual(Window.Current.Content);
            //    Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            //    if (increaseSearchWidth)
            //    {
            //        this.pageTitle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //    }
            //    //mySearchBox.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
            //    mySearchBox.Width = Window.Current.Bounds.Width - ( screenCoords.X + 20); // 20 is the right margin of search box 
            //    mySearchBox.Height = 30;
            //}
        }

        private void WebAccount_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBWebAccount"));
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = false;
            CredentialSettingsFlyout sf = new CredentialSettingsFlyout((SkyDriveFile)selectedItems[0], ((SkyDriveFile)selectedItems[0]).URL);
            sf.ShowIndependent();
        }

        private void CreditCard_Click(object sender, RoutedEventArgs e)
        {
            ((SearchBox)MainFrameHost.Current.FindName("_FavouritesSearchBox")).FocusOnKeyboardInput = false;
            CredentialSettingsFlyout sf = new CredentialSettingsFlyout((SkyDriveFile)selectedItems[0], ((SkyDriveFile)selectedItems[0]).URL);
            sf.ShowIndependent();
        }

        private void SettingAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBSetting"));
            SettingsPane.Show();
        }

        public async void EditCollection_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBEditCollection"));
            //toggle selection mode
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            if ( ((AppBarButton)MainFrameHost.Current.FindName("collectionCancelButton")).Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                // in case cancel is clicked then revert back selection and cancel add edits
                foreach (var item in source.CurrentFolder.AllItems)
                {
                    if (item.Type == SkyDriveItemType.File || item.Type == SkyDriveItemType.Folder)
                    {
                        item.IsChecked = false;
                        item.ShowCheckBox = !item.ShowCheckBox;
                    }
                }
            }
            else
            {
                foreach (var item in source.CurrentFolder.AllItems)
                {
                    if (item.Type == SkyDriveItemType.File || item.Type == SkyDriveItemType.Folder)
                    {
                        item.ShowCheckBox = !item.ShowCheckBox;
                    }
                }
            }
            ((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).Visibility = (((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).Visibility == Windows.UI.Xaml.Visibility.Visible) ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
            ((AppBarButton)MainFrameHost.Current.FindName("collectionCancelButton")).Visibility = (((AppBarButton)MainFrameHost.Current.FindName("collectionCancelButton")).Visibility == Windows.UI.Xaml.Visibility.Visible) ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
            
        }

      
        private void Click_ImportInstructions(object sender, RoutedEventArgs e)
        {
            Uri importInstruction;
            
            switch(Utility.GetPlatform())
            {
                case Platform.ARM: importInstruction = new Uri(@"http://bit.ly/1bv62wf");
                    break;
                default: importInstruction = new Uri(@"http://bit.ly/1b7flrA");
                    break;
            }
            Windows.System.Launcher.LaunchUriAsync(importInstruction);
        }

        private void flyoutAreaClicked(object sender, PointerRoutedEventArgs e)
        {
            BottomAppBar.IsOpen = true;
        }
    }

    public class TileTemplateSelector : DataTemplateSelector
    {
        //These are public properties that will be used in the Resources section of the XAML.
        public DataTemplate FileItemTemplate { get; set; }
        public DataTemplate FolderItemTemplate { get; set; }
        public DataTemplate AdItemTemplate { get; set; }
        public DataTemplate AdItemTemplate2 { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as Page;

            if (item != null && currentPage != null)
            {
                switch (((SkyDriveDataCommon)item).Type)
                {
                    case SkyDriveItemType.Folder: return FolderItemTemplate;
                    case SkyDriveItemType.File: return FileItemTemplate;
                    case SkyDriveItemType.AdBig: return AdItemTemplate;
                    case SkyDriveItemType.AdSmall: return AdItemTemplate2;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }

    public class ListViewTileTemplateSelector : DataTemplateSelector
    {
        //These are public properties that will be used in the Resources section of the XAML.
        public DataTemplate FileItemTemplate { get; set; }
        public DataTemplate FolderItemTemplate { get; set; }
        public DataTemplate AdItemTemplate { get; set; }
        public DataTemplate AdItemTemplate2 { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as Page;

            if (item != null && currentPage != null)
            {
                switch (((SkyDriveDataCommon)item).Type)
                {
                    case SkyDriveItemType.Folder: return FolderItemTemplate;
                    case SkyDriveItemType.File: return FileItemTemplate;
                    case SkyDriveItemType.AdBig: return AdItemTemplate;
                    case SkyDriveItemType.AdSmall: return AdItemTemplate2;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }

    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, ((SkyDriveDataCommon)item).Width);
            element.SetValue(VariableSizedWrapGrid.RowSpanProperty, ((SkyDriveDataCommon)item).Height);

            base.PrepareContainerForItemOverride(element, item);
        }
    }


}
