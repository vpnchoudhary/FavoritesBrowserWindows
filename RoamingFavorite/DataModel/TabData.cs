using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using RoamingFavorite.View;

namespace RoamingFavorite.Data
{
    public class TabData : RoamingFavorite.Common.BindableBase
    {
        private int tabID;
        public int TabID
        {
            get { return this.tabID; }
        }
        private string url = string.Empty;
        public string Url
        {
            get { return this.url; }
            set { this.SetProperty(ref this.url, value); }
        }
        private string _title = string.Empty;
        public string PageTitle
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }
        private Frame tabFrame;
        public Frame TabFrame
        {
            get { return this.tabFrame; }
        }


        //todo -- bring all operation on folderstack in this class
        public List<SkyDriveFolder> FolderStack = new List<SkyDriveFolder>();

        private Type frameType;
        public Type FrameType
        {
            get { return this.frameType; }
            set { this.SetProperty(ref this.frameType, value); }
        }

        //private ImageSource _image = null;

        private string _imagePath = @"ms-appx:///Assets/fileIcon.png";
        public string ImagePath
        {
            get { return _imagePath; }
            set { this.SetProperty(ref this._imagePath, value); }
        }
        public TabData(int tbid, string pageurl, string pagetitle)
        {
            tabID = tbid;
            url = pageurl;
            _title = pagetitle;
            tabFrame = new Frame();
            
        }

        private Brush _tileBackground = new SolidColorBrush(Windows.UI.Colors.Transparent);
        public Brush BackGround
        {
            get { return _tileBackground; }
            set { this.SetProperty(ref this._tileBackground, value); }
        }
    }
    public class TabDataSource
    {
        ObservableCollection<TabData> tabCollection = new ObservableCollection<TabData>();
        //Dictionary<int, TabData>
        int uniqueId = 0;
        private TabData activeTab = null;
        public TabData ActiveTab
        {
            get {
                    
                    return activeTab;
                }
            set {
                    if (activeTab != null)
                    {
                        activeTab.BackGround = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    }
                    activeTab = value;
                    if (activeTab != null)
                    {
                        activeTab.BackGround = new SolidColorBrush(Windows.UI.Colors.White);
                    }    
                
                }
        }
        public ObservableCollection<TabData> TabCollection
        {
            get { return this.tabCollection; }
        }
        public void AddTab(string url, string title, string faviconPath)
        {
            var tab = new TabData(++uniqueId, url, title);
            if(!string.IsNullOrEmpty(faviconPath))
            {
                tab.ImagePath = faviconPath;
            }
            tab.FrameType = typeof(RoamingFavorite.View.GroupedItemsPage);
            tabCollection.Add(tab);
            ActiveTab = tab;
        }

        public void RemoveTab(TabData tab)
        {
            int index = tabCollection.IndexOf(tab);
            if(tabCollection.Count - 1 > index )
            {
                ActiveTab = tabCollection.ElementAt<TabData>(index + 1);
            }
            else if(index > 0)
            {
                ActiveTab = tabCollection.ElementAt<TabData>(index - 1);
            }
            else
            {
                ActiveTab = null;
            }
            tabCollection.Remove(tab);
            //tab.TabFrame.con = null;
            if(tab.TabFrame != null)
            {
                if (typeof(ContentViewerWithAppBar) == tab.FrameType)
                {
                    ((ContentViewerWithAppBar)tab.TabFrame.Content).NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
                    ((WebView)((ContentViewerWithAppBar)tab.TabFrame.Content).FindName("FavoriteBrowser")).NavigateToString("");
                }
                tab.TabFrame.Content = null;
            }
            tab = null;
        }
    }
}

