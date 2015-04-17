using RoamingFavorite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using RoamingFavorite.Data;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace RoamingFavorite.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainFrameHost : Page
    {

        private NavigationHelper navigationHelper;

        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public static MainFrameHost Current;

        //public static TabData ActiveTab;
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public static TabDataSource tabSource = new TabDataSource();
        //private Frame currentFrame;
        //private global::Windows.UI.Xaml.Controls.Frame host; 
        public MainFrameHost()
        {
            this.InitializeComponent();
            //this.host.CacheSize = 25;
            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            Current = this;
            //Current._FavouritesSearchBox.Background = null;
            tabSource.AddTab("", "Favorites", "");
            ((Border)this.pageRoot.FindName("FrameGrid")).Child = tabSource.ActiveTab.TabFrame;
            //this.host = tabSource.ActiveTab.TabFrame;
            this.TabList.ItemsSource = tabSource.TabCollection;
            tabSource.ActiveTab.TabFrame.Navigate(typeof(GroupedItemsPage));

        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void SearchBoxEventsSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.SearchBoxEventsSuggestionsRequested(sender, args);
        }

        private void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.SearchBoxEventsQuerySubmitted(sender, args);
        }

        private void SearchBoxEventsResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.SearchBoxEventsResultSuggestionChosen(sender, args);
        }

        private void SearchBoxEventsLostFocus(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.SearchBoxEventsLostFocus(sender, e);
        }

        private void SearchBoxEventsGotFocus(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.SearchBoxEventsGotFocus(sender, e);
        }

        private void EditCollection_Click(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.EditCollection_Click(sender, e);
        }

        private void ButtonAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.ButtonAddFavorite_Click(sender, e);
        }

        private void ShowInIE_Click(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.ShowCredential_Click(sender, e);
        }

        private void ShowCredential_Click(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.ShowCredential_Click(sender, e);

        }

        private void NewPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tabSource.AddTab("", "Favorites", "");
            this.TabList.ItemsSource = tabSource.TabCollection;
            //tabSource.ActiveTab.TabFrame = tabSource.ActiveTab.TabFrame;
            ((Border)this.pageRoot.FindName("FrameGrid")).Child = tabSource.ActiveTab.TabFrame;
            tabSource.ActiveTab.TabFrame.Navigate(typeof(GroupedItemsPage));
        }

        private void NavigateBack(object sender, RoutedEventArgs e)
        {

        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
            page.GoBack(sender, e);
        }

        bool CanGoBack
        {
            get
            {
                var page = tabSource.ActiveTab.TabFrame.Content as PageCallBackInterface;
                return page.CanGoBack;
            }
        }



        private void TabList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tab = e.ClickedItem as TabData;
            tabSource.ActiveTab = tab;
            ((Border)this.pageRoot.FindName("FrameGrid")).Child = tabSource.ActiveTab.TabFrame;
            ((SearchBox)this.pageRoot.FindName("_FavouritesSearchBox")).QueryText = tabSource.ActiveTab.Url;

            if (typeof(GroupedItemsPage) == tab.FrameType)
            {
                //enable button for this view
                ((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).IsEnabled = true;
                ((AppBarButton)MainFrameHost.Current.FindName("_ButtonAddFavorite")).IsEnabled = false;
                ((AppBarButton)MainFrameHost.Current.FindName("btnShowInIE")).IsEnabled = false;
                ((AppBarButton)MainFrameHost.Current.FindName("btnShowCredential")).IsEnabled = false;
                ((BitmapImage)MainFrameHost.Current.FindName("_IconImage")).UriSource = new Uri(@"ms-appx:///Assets/fileIcon.png");
            }
            else if (typeof(ContentViewerWithAppBar) == tab.FrameType)
            {
                ((AppBarButton)MainFrameHost.Current.FindName("collectionEditButton")).IsEnabled = false;
                ((AppBarButton)MainFrameHost.Current.FindName("_ButtonAddFavorite")).IsEnabled = true;
                ((AppBarButton)MainFrameHost.Current.FindName("btnShowInIE")).IsEnabled = true;
                ((AppBarButton)MainFrameHost.Current.FindName("btnShowCredential")).IsEnabled = true;
                ((BitmapImage)MainFrameHost.Current.FindName("_IconImage")).UriSource = new Uri(tab.ImagePath);
            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;
            if (cmd.DataContext is TabData)
            {
                var p = (TabData)cmd.DataContext;
                tabSource.RemoveTab(p);
                TabList.ItemsSource = tabSource.TabCollection;
                if (tabSource.ActiveTab == null)
                {
                    tabSource.AddTab("", "Favorites", "");
                    TabList.ItemsSource = tabSource.TabCollection;
                    ((Border)this.pageRoot.FindName("FrameGrid")).Child = tabSource.ActiveTab.TabFrame;
                    tabSource.ActiveTab.TabFrame.Navigate(tabSource.ActiveTab.FrameType);
                }
                else
                {
                    ((Border)this.pageRoot.FindName("FrameGrid")).Child = tabSource.ActiveTab.TabFrame;
                }
            }
        }

        private void GoHome(object sender, RoutedEventArgs e)
        {
            tabSource.ActiveTab.TabFrame.Navigate(typeof(GroupedItemsPage));

        }
    }
}