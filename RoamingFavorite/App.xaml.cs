using RoamingFavorite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Live;
using RoamingFavorite.Data;
using RoamingFavorite.View;
using Windows.ApplicationModel.DataTransfer;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;



// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace RoamingFavorite
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
           ;
        }

       // public static List<IEnumerable<SkyDriveDataCommon>> FolderStack = new List<IEnumerable<SkyDriveDataCommon>>();
        //public static List<SkyDriveFolder> FolderStack = new List<SkyDriveFolder>();
        public static bool bClipBoardUsed = false;
        private static LiveConnectSession _session;
        public static LiveConnectSession Session
        {
            get
            {
                return _session;
            }
            set
            {
                _session = value;
                
            }
        }
        public static AppVisualState visualState = Common.AppVisualState.FullScreenLandscape;
        
        private static bool bOpenWebPageinIE = false;
        public static bool OpenWebPageinIE
        {
            get { return bOpenWebPageinIE; }
            set { bOpenWebPageinIE = value; }
        }

        private static string defaultSearchEngine = "bing";
        public static string DefaultSearchEngine
        {
            get { return defaultSearchEngine; }
            set { defaultSearchEngine = value; }
        }

        private static Uri urlToCopy = null;
        public static Uri UrlToCopy
        {
            get { return urlToCopy; }
        }

        public static bool ClipboardContainUrl()
        {
            if (urlToCopy != null)
                return true;
            else
                return false;
        }

        private static string _userName;
        public static string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;

            }
        }

        private static string cid;
        public static string CID
        {
            get
            {
                return cid;
            }
            set
            {
                cid = value;

            }
        }

        private static void LoadData()
        {
            //skyDriveDataSource.LoadData();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            // Create a Frame to act as the navigation context and associate it with
            // a SuspensionManager key
            var rootFrame = new Frame();
           
            //sualStateManager.GoToState()
            SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
               // await SuspensionManager.RestoreAsync();
            }

            if (rootFrame.Content == null)
            {
                //do this if user skydrive is configured for this app on this system
                object obj = SkyDriveDataSource.GetSetting(constants.bookmark);
                if (!Object.Equals(obj, null) && !object.Equals(obj, string.Empty))
                {
                    try
                    {
                        //
                        await SkyDriveDataSource.GetInstance();
                        HttpClient client = new HttpClient();
                        client.GetAsync(new Uri("http://bit.ly/FBSignin"));
                        await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
                    }
                    catch 
                    {
                        SkyDriveDataSource.DeleteContainer();
                    };
                    
                }
                
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainFrameHost)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            
            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            
            var deferral = e.SuspendingOperation.GetDeferral();
            //FavoriteFileStore fileStore = await FavoriteFileStore.GetInstance();
            //await fileStore.SaveXML();
            FavoritesDataSyncManager source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            await source.SaveData();
            deferral.Complete();
           
            
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            //var rootFrame = new Frame();
            //rootFrame.Navigate(typeof(ShareTargetFav), args.ShareOperation);
            //Window.Current.Content = rootFrame;
            //Window.Current.Activate();
            var shareTargetPage = new RoamingFavorite.View.ShareTargetFav();
            shareTargetPage.Activate(args);
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
           // await EnsureMainPageActivatedAsync(args);
            //var rootFrame = new Frame(); ;
            //if (args.QueryText == "")
            //{
            //    rootFrame.Navigate(typeof(GroupedItemsPage), "Root");
            //}
            //else
            //{
            //    rootFrame.Navigate(typeof(SearchResultsPage), args.QueryText);
            //}
            //Window.Current.Content = rootFrame;
            //Window.Current.Activate();
        }

    }
}
