using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Windows.Data.Xml.Dom;
using System.Threading.Tasks;

namespace RoamingFavorite.Data
{
    [Windows.Foundation.Metadata.WebHostHidden]
    class FavoriteItem : RoamingFavorite.Common.BindableBase
    {
        string _title;
        public string Title
        {
            get { return _title; }
            set { this.SetProperty(ref this._title, value); }
        }
        string _url;
        public string URL
        {
            get { return _url; }
            set { this.SetProperty(ref this._url, value); }
        }
        string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set { this.SetProperty(ref this._imagePath, value); }
        }
        bool _isChecked = true;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.SetProperty(ref this._isChecked, value); }
        }
        public FavoriteItem(string title, string url, string imagepath)
        {
            _title = title;
            _url = url;
            if (String.IsNullOrEmpty(imagepath))
            {
                if (Uri.IsWellFormedUriString(_url, UriKind.Absolute))
                {
                    _imagePath = @"http://getfavicon.appspot.com/" + System.Net.WebUtility.UrlEncode(_url);
                }
                else
                {
                    _imagePath = @"ms-appx:///Assets/favorite.png";
                }
            }
            _isChecked = true;
        }
    }
    class FavoriteSuggestionProvider
    {
        ObservableCollection<SkyDriveFile> favoriteList = new ObservableCollection<SkyDriveFile>();
        public FavoriteSuggestionProvider()
        {
            this.loadXML();
        }
        public ObservableCollection<SkyDriveFile> FavoriteList
        {
            get { return favoriteList; }
        }
        public async void loadXML()
        {
            Windows.Storage.StorageFolder storageFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("XML"); // you can get the specific folder from KnownFolders or other folders via FolderPicker as well
            Windows.Storage.StorageFile storageFile = await storageFolder.GetFileAsync("FavoriteSuggestionList.xml");
            Windows.Data.Xml.Dom.XmlLoadSettings loadSettings = new Windows.Data.Xml.Dom.XmlLoadSettings();
            loadSettings.ProhibitDtd = false; // sample
            loadSettings.ResolveExternals = false; // sample
            XmlDocument doc = await XmlDocument.LoadFromFileAsync(storageFile, loadSettings);
            XmlNodeList groups = doc.SelectNodes("//FavoriteList/FavoriteItem");
            foreach (var group in groups)
            {
                SkyDriveFile favoriteItem = new SkyDriveFile(group.Attributes[0].NodeValue.ToString(), group.Attributes[1].NodeValue.ToString(),group.Attributes[2].NodeValue.ToString());
                favoriteItem.IsChecked = true;
                favoriteItem.ShowCheckBox = true;
                favoriteList.Add(favoriteItem);
            }
        }
    }

   
    
}
