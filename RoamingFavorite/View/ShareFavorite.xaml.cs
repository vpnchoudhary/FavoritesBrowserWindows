//---------------------------------------------------------------------
// <copyright file="FavItemsPage.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
namespace RoamingFavorite.View
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using RoamingFavorite.Data;
    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.StartScreen;
    using System.Threading.Tasks;
    using System.IO;
    using Windows.Storage;
    using System.Xml.Serialization;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml.Navigation;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.ApplicationModel.DataTransfer.ShareTarget;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ShareFavorite : RoamingFavorite.Common.LayoutAwarePage
    {
        ShareOperation shareOperation;
        private string sharedHtml;
        private SkyDriveFolder itemSelected = null;

        /// <summary>
        /// Initialize the instance of FavItemsPage class.
        /// </summary>
        public ShareFavorite()
        {
            this.InitializeComponent();
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // It is recommended to only retrieve the ShareOperation object in the activation handler, return as
            // quickly as possible, and retrieve all data from the share target asynchronously.

            this.shareOperation = (ShareOperation)e.Parameter;

            // Retrieve the data package content.
            if (this.shareOperation.Data.Contains(StandardDataFormats.Html))
            {
                try
                {
                    sharedHtml = await this.shareOperation.Data.GetHtmlFormatAsync();
                }
                catch { }
            }
            string matchString = "<a (class=\"snippet-URL\" href=\"([^<]*))\">(.*?)</a>";
            Regex regex = new Regex(matchString);
            Match match = regex.Match(sharedHtml);
            if (match.Success)
            {
                Url.Text = match.Groups[2].Value;
                Title.Text = match.Groups[3].Value;
            }
            base.OnNavigatedTo(e);
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

            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            var sampleDataGroups = await source.GetGroups(source.Root, true);
            Selection.Text = "Bookmark";
            SemListView.ItemsSource = sampleDataGroups.AllItems;
           
            //    SemListView.ItemsSource = semSource;
            
        }

        private async void AddFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.shareOperation.ReportStarted();
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            await source.UploadFile(this.itemSelected.UniqueId, Url.Text, Title.Text);
            this.shareOperation.ReportCompleted();
        }

        private async void ListItemSelected(object sender, ItemClickEventArgs e)
        {
            itemSelected = ((SkyDriveFolder)e.ClickedItem);
            Selection.Text = itemSelected.Title;
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            var sampleDataGroups = await source.GetGroups(itemSelected.UniqueId, true);
            SemListView.ItemsSource = sampleDataGroups.AllItems;
           
        }
    }
}
