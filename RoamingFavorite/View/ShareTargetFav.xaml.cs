using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
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
using Windows.ApplicationModel.DataTransfer;
using RoamingFavorite.Data;

// The Share Target Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234241

namespace RoamingFavorite.View
{
    // TODO: Edit the manifest to enable use as a share target
    //
    // The package manifest could not be automatically updated.  Open the package manifest
    // file and ensure that support for activation as a share target is enabled.

    // TODO: Respond to activation as a share target
    //
    // The following code could not be automatically added to your application subclass,
    // either because the appropriate class could not be located or because a method with
    // the same name already exists.  Ensure that appropriate code deals with activation
    // by displaying a flyout appropriate for receiving a shared item.
    //
    // /// <summary>
    // /// Invoked when the application is activated as the target of a sharing operation.
    // /// </summary>
    // /// <param name="args">Details about the activation request.</param>
    // protected override void OnShareTargetActivated(Windows.ApplicationModel.Activation.ShareTargetActivatedEventArgs args)
    // {
    //     var shareTargetPage = new RoamingFavorite.View.ShareTargetFav();
    //     shareTargetPage.Activate(args);
    // }
    /// <summary>
    /// This page allows other applications to share content through this application.
    /// </summary>
    public sealed partial class ShareTargetFav : RoamingFavorite.Common.LayoutAwarePage
    {
        /// <summary>
        /// Provides a channel to communicate with Windows about the sharing operation.
        /// </summary>
        private Windows.ApplicationModel.DataTransfer.ShareTarget.ShareOperation _shareOperation;
        private SkyDriveFolder itemSelected = null;
        private string title = string.Empty;
        private string url = string.Empty;
        List<SkyDriveFolder> FolderStack = new List<SkyDriveFolder>();

        public ShareTargetFav()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when another application wants to share content through this application.
        /// </summary>
        /// <param name="args">Activation data used to coordinate the process with Windows.</param>
        public async void Activate(ShareTargetActivatedEventArgs args)
        {
            this._shareOperation = args.ShareOperation;

            if (this._shareOperation.Data.Contains(StandardDataFormats.Uri))
            {
                Uri uri = await this._shareOperation.Data.GetUriAsync();
                if (uri != null)
                {
                    // To output text from this example, you need a TextBlock control
                    // with a name of "contentValue". 
                    this.url = uri.AbsoluteUri;
                }
            }


            // Communicate metadata about the shared content through the view model
            var shareProperties = this._shareOperation.Data.Properties;
            var thumbnailImage = new BitmapImage();
            this.DefaultViewModel["Title"] = this.title = shareProperties.Title;
            this.DefaultViewModel["Description"] = this.url;// = shareProperties.Description;
            this.DefaultViewModel["Image"] = thumbnailImage;
            this.DefaultViewModel["Sharing"] = true;
            this.DefaultViewModel["ShowImage"] = false;
            this.DefaultViewModel["Comment"] = String.Empty;
            this.DefaultViewModel["SupportsComment"] = true;
            Window.Current.Content = this;
            Window.Current.Activate();
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
           // itemSelected = await source.GetFolder(source.Root);
            var sampleDataGroups = await source.GetGroups(source.Root, true);
            FolderStack.Add(sampleDataGroups);
            itemSelected = sampleDataGroups;
            this.DefaultViewModel["FolderName"] = itemSelected.Title;
            SemListView.ItemsSource = sampleDataGroups.AllItems;
            this.DefaultViewModel["Sharing"] = false;

            // Update the shared content's thumbnail image in the background
            if (shareProperties.Thumbnail != null)
            {
                var stream = await shareProperties.Thumbnail.OpenReadAsync();
                thumbnailImage.SetSource(stream);
                this.DefaultViewModel["ShowImage"] = true;
            }
        }

        private async void ListItemSelected(object sender, ItemClickEventArgs e)
        {
            this.DefaultViewModel["Sharing"] = true;
            SemListView.ItemsSource = null;
            this.DefaultViewModel["FolderName"] = ((SkyDriveFolder)e.ClickedItem).Title;
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            itemSelected = await source.GetGroups(((SkyDriveFolder)e.ClickedItem).UniqueId, true);
            SemListView.ItemsSource = itemSelected.AllItems;
            FolderStack.Add(itemSelected);
            this.DefaultViewModel["Sharing"] = false;
            //this.DefaultViewModel["Sharing"] = true;
            //SemListView.ItemsSource = null;
            //itemSelected = ((SkyDriveFolder)e.ClickedItem);
            //this.DefaultViewModel["FolderName"] = itemSelected.Title;
            //FolderStack.Add(itemSelected);
            //if (itemSelected.Subalbums.Count == 0)
            //{
            //    SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            //    var sampleDataGroups = await source.GetGroups(itemSelected.UniqueId, true);
            //    SemListView.ItemsSource = sampleDataGroups.AllItems;
            //}
            //this.DefaultViewModel["Sharing"] = false;
        }

        /// <summary>
        /// Invoked when the user clicks the Share button.
        /// </summary>
        /// <param name="sender">Instance of Button used to initiate sharing.</param>
        /// <param name="e">Event data describing how the button was clicked.</param>
        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DefaultViewModel["Sharing"] = true;
            this._shareOperation.ReportStarted();
            try
            {
                this.DefaultViewModel["Sharing"] = true;
                SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
                string uniqueid = source.Root;
                if (this.itemSelected != null)
                {
                    uniqueid = this.itemSelected.UniqueId;
                }
                bool result = await source.UploadFile(uniqueid, this.url, this.title);
            }
            catch (Exception ex)
            {
                this._shareOperation.ReportError(ex.ToString());
            }
            finally
            {
                this.DefaultViewModel["Sharing"] = false;
                this._shareOperation.ReportCompleted();
            }
        }

        private async void FolderUp(object sender, RoutedEventArgs e)
        {
            SemListView.ItemsSource = null;
            //this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            // Use the navigation frame to return to the previous page
            if (FolderStack.Count > 1)
            {
                this.DefaultViewModel["Sharing"] = true;
                SemListView.ItemsSource = FolderStack[FolderStack.Count - 2].AllItems;
                itemSelected = FolderStack[FolderStack.Count - 2];
                FolderStack.RemoveAt(FolderStack.Count - 1);
                this.DefaultViewModel["FolderName"] = itemSelected.Title;
                this.DefaultViewModel["Sharing"] = false;
            }
            if (FolderStack.Count == 1)
            {
                SemListView.ItemsSource = FolderStack[0].AllItems;
                itemSelected= FolderStack[0];
                this.DefaultViewModel["FolderName"] = itemSelected.Title;
            }
            

            //SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            //// Use the navigation frame to return to the previous page
            //if (FolderStack.Count > 1)
            //{
            //    this.DefaultViewModel["Sharing"] = true;
            //    SemListView.ItemsSource = null;
            //    var matches = await source.GetGroups((FolderStack[FolderStack.Count - 2]).UniqueId, true);
            //    itemSelected = FolderStack[FolderStack.Count - 2];
            //    this.DefaultViewModel["FolderName"] = itemSelected.Title;
            //    SemListView.ItemsSource = matches.AllItems;
            //    FolderStack.RemoveAt(FolderStack.Count - 1);
            //    this.DefaultViewModel["Sharing"] = false;
            //}
            //if (FolderStack.Count == 1)
            //{
            //    var sampleDataGroups = await source.GetGroups(source.Root, true);
            //    //var matches = sampleDataGroups.Where((item) => item.ParentID.Equals(string.Empty));
            //    SemListView.ItemsSource = sampleDataGroups.AllItems;
            //}
           // FolderBack.IsEnabled = CanGoBack;


        }

        private void EditBookmarkDesc_Click(object sender, RoutedEventArgs e)
        {
            this.BookmarkDesc.IsReadOnly = false;
        }
        
    }
}
