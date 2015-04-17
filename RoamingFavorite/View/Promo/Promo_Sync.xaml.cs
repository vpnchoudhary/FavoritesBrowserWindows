using RoamingFavorite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;


namespace RoamingFavorite.View
{
    
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Promo_Sync : RoamingFavorite.Common.LayoutAwarePage
    {
        int counter = 0;
        PromoDataSource promoContentSource;
        List<RadioButton> radioCollection = new List<RadioButton>();
        bool showFavoriteSuggestion = false;

        
        public Promo_Sync()
        {
            this.InitializeComponent();
            //populate promodatalist
            promoContentSource = new PromoDataSource();
            this.Loaded += Promo_Sync_Loaded;
            this.Unloaded += Promo_Sync_Unloaded;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if(navigationParameter != null)
            {
                showFavoriteSuggestion = (bool)navigationParameter;
            }
            for (int i = 0; i < promoContentSource.GetPromoData.Count; i++)
            {
                RadioButton temp = new RadioButton();
                temp.Style = this.Resources["customRadioButtonStyle"] as Style;
                temp.IsChecked = false;
                //temp.IsEnabled = false;
                radioCollection.Add(temp);
            }
            foreach (var control in radioCollection)
            {
                this.radioList.Children.Add(control);
            }
            UIRefresh();
        }

        void Promo_Sync_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnRegisterForShare();
        }

        void Promo_Sync_Loaded(object sender, RoutedEventArgs e)
        {
            this.RegisterForShare();
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

        private void emailButton_Click(object sender, RoutedEventArgs e)
        {
            // open share charm and prepare for email help page
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }

        private void ShareHtmlHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = "Instructions to sync IE's favorites on your desktop to SkyDrive";
            request.Data.Properties.Description = "";
            if (counter == this.promoContentSource.GetPromoData.Count-1)
            {
                try
                {
                    string localImage = "ms-appx:///Assets/SkyDriveSetupInstruction.png";
                    string html = @"<html><body><h2>Instructions to sync IE's favorites on your desktop to SkyDrive</h2>" +
                        @"<p><img src=""" + localImage + @""" alt=""Sync Favorites""></p>" +
                        @"<p><ol>" +
                        @"<li>Download SkyDrive http://windows.microsoft.com/en-us/skydrive/download</li>" +
                        @"<li>Open directory c:\users\%username%. (Replace %username% with login ID you use to logon to your computer)</li>" +
                        @"<li>Right click on Favorites folder and click Properties -> Click Location tab -> Click Move</li>" +
                        @"<li>Choose the folder C:\Users\%username%\SkyDrive\RoamingFavoritesApp\Bookmark.</li>" +
                        @"<li>Open folder C:\Users\%username%\SkyDrive and right click on RoamingFavoritesApp folder and click Make available offline.</li>" +
                        @"<li>Done. Repeat above instructions for all of your laptop/desktop.</li>" +
                        @"</ol></p></body></html>";
                    request.Data.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(html));
                    request.Data.Properties.ApplicationName = "Favorites Browser";
                    RandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromUri(new Uri(localImage));
                    request.Data.ResourceMap[localImage] = streamRef;
                }
                catch { request.FailWithDisplayText("Folder can't be shared as bookmarks."); } //do nothing : might be empty string becuase only folder is selected
            }
            else
            {
                request.FailWithDisplayText("Nothing to share.");
            }
            deferral.Complete();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoForward_Click(object sender, RoutedEventArgs e)
        {
            GoForward();           
        }

        private void GoBack()
        {
            if (counter == 0)
            {
                return;
            }
            else
            {
                counter--;
                UIRefresh();
            }
        }

        private void GoForward()
        {
            if (counter < (promoContentSource.GetPromoData.Count - 1))
            {
                counter++;
                UIRefresh();
            }
            else
            {
                if (showFavoriteSuggestion)
                {
                    Frame.Navigate(typeof(FavoriteSuggestions));
                }
                else
                {
                    Frame.Navigate(typeof(GroupedItemsPage), "Root");
                }
            }
        }

        void UIRefresh()
        {
            this.DataContext = promoContentSource.GetPromoData[counter];
            foreach (var item in this.radioCollection)
            {
                item.IsChecked = false;
            }
            this.radioCollection[counter].IsChecked = true;
            
        }

        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if(e.Velocities.Linear.X < 0)
            {
                //negative mean left swipe
                GoForward();
            }
            else
            {
                //positive means right swipe
                GoBack();
            }
        }
    }

    public enum StretchOption
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    }

    public class PromoData 
    {
        string promoHeader;
        public string PromoHeader
        {
            get { return promoHeader; }
        }

        string promoMessage;
        public string PromoMessage
        {
            get { return promoMessage; }
        }
        string promoImagePath;
        public string PromoImagePath
        {
            get { return promoImagePath; }
        }
        string imageStretchOption;
        public string ImageStretchOption
        {
            get { return imageStretchOption; }
        }
        string promoMessageWidth;
        public string PromoMessageWidth
        {
            get { return promoMessageWidth; }
        }
        bool showEmailButton;
        public bool ShowEmailButton
        {
            get { return showEmailButton; }
        }
        public PromoData(string header, string imagePath, string stretchOption, string message, string width, bool showEmail = false)
        {
            promoHeader = header;
            promoImagePath = imagePath;
            imageStretchOption = stretchOption;
            promoMessage = message;
            promoMessageWidth = width;
            showEmailButton = showEmail;
        }
    }

    public class PromoDataSource
    {
        List<PromoData> promoDataList = new List<PromoData>();
        public List<PromoData> GetPromoData
        {
            get { return promoDataList; }
        }

        public PromoDataSource()
        {
            //populate promodatalist
            promoDataList.Add(new PromoData("Welcome to Favorites Browser", "ms-appx:///Assets/RoamingFavorite_LandingPage.png", StretchOption.Fill.ToString(), "Roaming Favorite will help you keep your favorites and web credentials with you wherever you go.", "600"));
            promoDataList.Add(new PromoData("Always stay Up-To-Date", "ms-appx:///Assets/RoamingFavorite_sync.jpg", StretchOption.None.ToString(), "Instant Sync keep your favorites updated across your devices. All your data is stored in your own SkyDrive cloud storage to ensure safety.", "600"));
            promoDataList.Add(new PromoData("Save favorites from any device", "ms-appx:///Assets/Promo_ShareFavorite.png", StretchOption.Fill.ToString(), "Save favorites either through IE on any of your desktop or IE App from Windows tablet or desktop. All your favorites are synced across your devices and can be accessed from any of your device.", "600"));
            promoDataList.Add(new PromoData("Never forget your password again and confidently save your password.", "ms-appx:///Assets/Promo_SavePassword.png", StretchOption.Fill.ToString(), "Favorites Browser App provide deep integeration with build-in browser which provide you ability to save password for web sites and automatically populate them. Password are secured in Windows credential manager.", "600"));
            promoDataList.Add(new PromoData("Become productive use Snap mode to make it easier to work on projects", "ms-appx:///Assets/Promo_Snapmode_project.png", StretchOption.Fill.ToString(), "Favorites Browser App provide you the option to open web sites either in IE App or build-in browser. Snap mode make it easy work on your project and take notes. You may also share your work with family and friends by using share charm.", "600"));
            promoDataList.Add(new PromoData("Sync IE's favorites on your desktop's with Favorites Browser", "ms-appx:///Assets/RoamingFavorite_skydrive_install.jpg", StretchOption.None.ToString(),
                @"To effectively use Favorites Browser it is recommended that you Import IE's favorite to your SkyDrive cloud. Please tap the email button below to get the instruction to follow on your desktop machines.", "600",true));
        }
    }
} 
