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
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RoamingFavorite.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HelpPage2 : Page
    {
        public HelpPage2()
        {
            this.InitializeComponent();
            this.Loaded += HelpPage2_Loaded;
            this.Unloaded += HelpPage2_Unloaded;
        }

        void HelpPage2_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnRegisterForShare();
        }

        void HelpPage2_Loaded(object sender, RoutedEventArgs e)
        {
            this.RegisterForShare();
        }

        private void GoNext(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GroupedItemsPage), "Root");
        }

        private void emailButton_Click(object sender, RoutedEventArgs e)
        {
            // open share charm and prepare for email help page
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
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
            DataRequest request = e.Request;
            DataRequestDeferral deferral = request.GetDeferral();
            request.Data.Properties.Title = "Instructions to sync IE's favorites on your desktop to SkyDrive";
            request.Data.Properties.Description = "";
            //if (counter == this.promoContentSource.GetPromoData.Count - 1)
            //{
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
            //}
            //else
            //{
            //    request.FailWithDisplayText("Nothing to share.");
            //}
            deferral.Complete();
        }
    }
}
