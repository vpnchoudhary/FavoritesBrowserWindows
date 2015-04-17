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
using System.ComponentModel;
using System.Threading.Tasks;
using RoamingFavorite.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RoamingFavorite.View
{
      
    public sealed partial class SelectFolderUserControl : UserControl
    {
        private SkyDriveFolder itemSelected = null;
        List<SkyDriveFolder> FolderStack = new List<SkyDriveFolder>();
        public event EventHandler<SkyDriveFolder> FolderSelectedEvent;
       

        private bool sharing = false;
        public bool Sharing
        {
            get { return sharing; }
            set
            {
                sharing = value;
                if (sharing == true)
                {
                    this.progressRing.IsActive = true;
                   // this.buttonSelect.IsEnabled = true;
                }
                else
                {
                    this.progressRing.IsActive = false;
                    //this.buttonSelect.IsEnabled = false;
                }
            }
        }

        void OnFolderSelectedEvent(SkyDriveFolder e)
        {
            FolderSelectedEvent(this, e);
        }

        public SkyDriveFolder ItemSelected
        {
            get { return itemSelected;}
        }

        public SelectFolderUserControl()
        {
            this.InitializeComponent();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            OnFolderSelectedEvent(itemSelected);
        }

        private async void FolderUp(object sender, RoutedEventArgs e)
        {
            SemListView.ItemsSource = null;
            //this.mainProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            // Use the navigation frame to return to the previous page
            if (FolderStack.Count > 1)
            {
                //Sharing = true;
                Sharing = true;
                SemListView.ItemsSource = FolderStack[FolderStack.Count - 2].Subalbums;
                itemSelected = FolderStack[FolderStack.Count - 2];
                FolderStack.RemoveAt(FolderStack.Count - 1);
                this.FolderName.Text = itemSelected.Title;
                Sharing = false;
            }
            if (FolderStack.Count == 1)
            {
                SemListView.ItemsSource = FolderStack[0].Subalbums;
                itemSelected = FolderStack[0];
                this.FolderName.Text = itemSelected.Title;
            }

        }

        private async void ListItemSelected(object sender, ItemClickEventArgs e)
        {
            Sharing = true;
            SemListView.ItemsSource = null;
            this.FolderName.Text = ((SkyDriveFolder)e.ClickedItem).Title;
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            itemSelected = await source.GetCollection(((SkyDriveFolder)e.ClickedItem).UniqueId);
            SemListView.ItemsSource = itemSelected.Subalbums;
            FolderStack.Add(itemSelected);
            Sharing = false;
        }

        public async Task LoadFolder()
        {
            var source = await FavoritesDataSyncManager.GetInstance(DocumentLocation.Roaming);
            var sampleDataGroups = await source.GetRoot();
            FolderStack.Add(sampleDataGroups);
            itemSelected = sampleDataGroups;
            this.FolderName.Text = itemSelected.Title;
            SemListView.ItemsSource = sampleDataGroups.Subalbums;
            Sharing = false;   
        }
    }
}
