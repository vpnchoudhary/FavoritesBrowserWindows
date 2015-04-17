using HtmlAgilityPack;
using Microsoft.Live;
using RoamingFavorite.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace RoamingFavorite.Data
{
    public enum SkyDriveItemType
    {
        File,
        Folder,
        Audio,
        Video,
        Album,
        Photo,
        Friend,
        Event,
        Contact,
        Calender,
        Comment,
        Error,
        Permissions,
        Search,
        Tag,
        User,
        AdBig,
        AdSmall
    }

    public enum ItemState
    {
        Add,
        Delete,
        Update,
        Local,
        Server
    }
    public struct From
    {
        string Name;
        string Id;
        public From(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }
    }
    public struct Shared_with
    {
        string _access;
        public Shared_with(string access)
        {
            this._access = access;
        }
    }

    public class Comment : RoamingFavorite.Common.BindableBase
    {
        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }
        From _from;
        public From From
        {
            get { return this._from; }
            set { this.SetProperty(ref this._from, value); }
        }
        string _message;
        public string Message
        {
            get { return this._message; }
            set { this.SetProperty(ref this._message, value); }
        }
        string _created_time;
        public string Created_Time
        {
            get { return this._created_time; }
            set { this.SetProperty(ref this._created_time, value); }
        }

        public Comment(string uniqueId, From from, string message, string created_time)
        {
            this._uniqueId = uniqueId;
            this._from = from;
            this._message = message;
            this._created_time = created_time;
        }
    }

    /// <summary>
    /// Base class for <see cref="SkyDriveItem"/> and <see cref="SkyDriveAlbum"/> that
    /// defines properties common to both.
    /// </summary>

    public class ItemSelectionEventArguments : EventArgs
    {
        SkyDriveDataCommon selectedItem;
        public ItemSelectionEventArguments(SkyDriveDataCommon item)
        {
            selectedItem = item;
        }
        public SkyDriveDataCommon SelectedItem
        {
            get { return selectedItem; }
        }
    }
    
    public abstract class SkyDriveDataCommon : RoamingFavorite.Common.BindableBase
    {
        public static event EventHandler<ItemSelectionEventArguments> ItemSelectionEvent;
        void OnItemSelectionEvent(ItemSelectionEventArguments e)
        {
            ItemSelectionEvent(this, e);
        }
        private static Uri _baseUri = new Uri("ms-appx:///");
        public SkyDriveDataCommon(string title)
        {
            this._title = title;
            try
            {
                this._iconBackground = new SolidColorBrush(Color.FromArgb(150, 255, 0, 0));
            }
            catch { }
        }



        bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.SetProperty(ref this._isChecked, value); OnItemSelectionEvent(new ItemSelectionEventArguments(this)); }
        }

        bool _showCheckBox = false;
        public bool ShowCheckBox
        {
            get { return _showCheckBox; }
            set { 
                    this.SetProperty(ref this._showCheckBox, value);
                   
                }
        }

        int width = 1;
        public int Width
        {
            get { return width; }
            set { this.SetProperty(ref this.width, value); }
        }

        int height = 1;
        public int Height
        {
            get { return height; }
            set { this.SetProperty(ref this.height, value); }
        }



        public SkyDriveDataCommon() { }
        public SkyDriveDataCommon(string uniqueId, string name, string description, int size, string parent_id, SkyDriveItemType type, string updated_time)
        {
            this._uniqueId = uniqueId;
            this._title = name;
            this._description = description;
            this._parent_id = parent_id;
            this._type = type;
            this._size = size;
            if (!string.IsNullOrEmpty(updated_time))
            {
                try
                {
                    this._updated_time = updated_time;//.Substring(0, 10);
                }
                catch { };
            }
            try
            {
                this._tileBackground = new SolidColorBrush(Color.FromArgb(255, 40, 122, 237));
                this._iconBackground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
            }
            catch { };

        }

        private ItemState syncState;

        public ItemState SyncState
        {
            get { return this.syncState; }
            set { this.SetProperty(ref this.syncState, value); }
        }

        private SkyDriveItemType _type;
        public SkyDriveItemType Type
        {
            get { return this._type; }
            set { this.SetProperty(ref this._type, value); }
        }
        private string _parent_id;
        public string Parent_id
        {
            get { return this._parent_id; }
            set { this.SetProperty(ref this._parent_id, value); }
        }
        private string _updated_time;
        public string Updated_time
        {
            get { return this._updated_time; }
            set { this.SetProperty(ref this._updated_time, value); }
        }

        public string Formatted_updated_time
        {
            get
            {
                if (this._updated_time != null)
                {
                    this._updated_time.Substring(0, 10);
                }
                return this._updated_time;
            }
        }

        private int _size;
        public int Size
        {
            get { return this._size; }
            set { this.SetProperty(ref this._size, value); }
        }
        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;

        private string _imagePath = @"ms-appx:///Assets/fileIcon.png";
        public string ImagePath
        {
            get { return _imagePath; }
            set { this.SetProperty(ref this._imagePath, value); }
        }

        public ImageSource Image
        {
            get
            {
                try
                {
                    if (this._image == null && this._imagePath != null)
                    {
                        this._image = new BitmapImage(new Uri(this._imagePath));
                    }
                }
                catch
                {
                    this._imagePath = @"ms-appx:///Assets/fileIcon.png";
                    this._image = new BitmapImage(new Uri(this._imagePath));
                };
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }


        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        private Color _imageDominantColor = new Color();
        public Color ImageDominantColor
        {
            get { return _imageDominantColor; }
            set { _imageDominantColor = value; }
        }

        private Brush _tileBackground;
        public Brush TileBackground
        {
            get { return _tileBackground; }
            set { this.SetProperty(ref this._tileBackground, value); }
        }

        private Brush _iconBackground;
        public Brush IconBackground
        {
            get { return _iconBackground; }
            set { this.SetProperty(ref this._iconBackground, value); }
        }
    }

    public class AdItem : SkyDriveDataCommon
    {
        public AdItem(string title, SkyDriveItemType itemType) : base(title)
        {
            this.Type = itemType;
            switch(itemType)
            {
                case SkyDriveItemType.AdBig: this.Height = 6;
                    break;
                case SkyDriveItemType.AdSmall: this.Height = 3;
                    break;
            }
        }
    }
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SkyDriveFile : SkyDriveDataCommon
    {
        public SkyDriveFile() { }

        public SkyDriveFile(string uniqueId, string name, string description, int size, string parent_id, SkyDriveItemType type, string updated_time, string source)
            : base(uniqueId, name, description, size, parent_id, type, updated_time)
        {
            this._source = source;
            this._fileExtenstion = Path.GetExtension(name);
            if (this._fileExtenstion.Equals(".url", StringComparison.CurrentCultureIgnoreCase))
            {
                this._isInternetShortcut = true;
            }
            int urlExtIndex = name.LastIndexOf(".");
            if (urlExtIndex >= 0)
            {
                this.Title = name.Substring(0, urlExtIndex);
            }
            else
            {
                this.Title = name;
            }
        }

        public SkyDriveFile(string name, string url, string imagePath)
            : base(string.Empty, name, string.Empty, 0, string.Empty, SkyDriveItemType.File, string.Empty)
        {
            this.URL = url;
            this._fileExtenstion = ".url";
            this._isInternetShortcut = true;
            this.IsChecked = true;
            if (String.IsNullOrEmpty(imagePath))
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    this.ImagePath = @"http://www.google.com/s2/favicons?domain=" + new Uri(url).Host; //@"http://getfavicon.appspot.com/" + System.Net.WebUtility.UrlEncode(url);
                }
                else
                {
                    this.ImagePath = @"ms-appx:///Assets/favorite.png";
                }
            }
        }

        private string _fileExtenstion = string.Empty;
        public string FileExtenstion
        {
            get { return this._fileExtenstion; }
            set { this.SetProperty(ref this._fileExtenstion, value); }
        }
        private bool _isInternetShortcut = false;
        public bool IsInternetShortcut
        {
            get { return this._isInternetShortcut; }
            set { this.SetProperty(ref this._isInternetShortcut, value); }
        }
        private string _source = string.Empty;
        public string Source
        {
            get { return this._source; }
            set { this.SetProperty(ref this._source, value); }
        }
        private string _url = string.Empty;
        public string URL
        {
            get { return this._url; }
            set { this.SetProperty(ref this._url, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SkyDriveFolder : SkyDriveDataCommon
    {
        public SkyDriveFolder(string title) : base(title) { this.Type = SkyDriveItemType.Folder; }
        
        public SkyDriveFolder(string uniqueId, string name, string description, int size, string parent_id, 
            SkyDriveItemType type, string updated_time, int count)
            : base(uniqueId, name, description, size, parent_id, type, updated_time)
        {
            _count = count;
        }
        private int _count = 0;
        public int Count
        {
            get { return this._count; }
            set { this.SetProperty(ref this._count, value); }
        }

        private ObservableCollection<SkyDriveFile> _items = new ObservableCollection<SkyDriveFile>();
        public ObservableCollection<SkyDriveFile> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SkyDriveFolder> _subalbums = new ObservableCollection<SkyDriveFolder>();
        public ObservableCollection<SkyDriveFolder> Subalbums
        {
            get { return this._subalbums; }
            set { this._subalbums = value; }
        }

        private List<SkyDriveFile> otherFileItems = new List<SkyDriveFile>();
        public List<SkyDriveFile> OtherFileItems
        {
            get { return this.otherFileItems; }
        }

        private ObservableCollection<AdItem> _ads = new ObservableCollection<AdItem>();
        public ObservableCollection<AdItem> Ads
        {
            get { return this._ads; }
        }

        public IEnumerable<SkyDriveDataCommon> AllItems
        {
        //    // Provides a subset of the full items collection to bind to from a GroupedItemsPage
        //    // for two reasons: GridView will not virtualize large items collections, and it
        //    // improves the user experience when browsing through groups with large numbers of
        //    // items.
        //    //
        //    // A maximum of 12 items are displayed because it results in filled grid columns
        //    // whether there are 1, 2, 3, 4, or 6 rows displayed

            get { return new ObservableCollection<SkyDriveDataCommon>(this.Subalbums).Concat(new ObservableCollection<SkyDriveDataCommon>(this.Items)).Concat(new ObservableCollection<SkyDriveDataCommon>(this.Ads)); }
        }
    }

    
    public class NewSingupEventArguments : EventArgs
    {
        NewSignupState state;
        public NewSingupEventArguments(NewSignupState newState)
        {
            state = newState;
        }
        public NewSignupState State
        {
            get { return state; }
        }
    }


    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// </summary>
    /// 
    public sealed class SkyDriveDataSource : INotifyPropertyChanged
    {
        public static event EventHandler<NewSingupEventArguments> newSignupEvent;
        private static SkyDriveDataSource _sampleDataSource = null;
        private static AsyncLock obj = new AsyncLock();
        //private static SkyDriveFolder searchResultContainer = new SkyDriveFolder("SearchResults");
        public static async Task<SkyDriveDataSource> GetInstance()
        {
            if (!Utility.IsConnectedToInternet())
            {
                //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async() => 
                //{
                    MessageDialog dialog = new MessageDialog("Please check your internet connection settings and try again.", "Unable to connect to the Internet");
                    //dialog.ShowAsync();
                
                //});
            }
            using (await obj.LockAsync())
            {
                if (_sampleDataSource == null)
                {
                    _sampleDataSource = new SkyDriveDataSource();
                    await _sampleDataSource.InitAuth();
                    //t.Wait();
                }
            }
            
            return _sampleDataSource;
        }
        public LiveAuthClient authClient;
        //public static SkyDriveFolder RootFolder = new SkyDriveFolder("Root");
        private SkyDriveFolder _currentFolder = null;
        public SkyDriveFolder CurrentFolder
        {
            get { return _currentFolder; }
            set { _currentFolder = value; NotifyPropertyChanged("CurrentFolder"); }
        }
        private string _root;
        public string Root
        {
            get { return _root; }
        }
        public static bool done = false;
        private string _userName;
        public string UserName
        {
            get { return this._userName; }
            set
            {
                _userName = value;
                NotifyPropertyChanged("UserName");
            }
        }
        private string _imageUrl;
        public string ImageUrl
        {
            get { return this._imageUrl; }
            set
            {
                _imageUrl = value;
                NotifyPropertyChanged("ImageUrl");
            }
        }
        void OnNewSignupEvent(NewSingupEventArguments e)
        {
            newSignupEvent(this, e);
        }

        //fetch foder metadata and folder child (file and folder) metadata
        public async Task<SkyDriveFolder> GetGroups(string uniqueId, bool fFolderOnly = false, bool bLoadFileSynchroniously = false)
        {
            if (uniqueId.Equals("Root"))
            {
                uniqueId = Root;
            }
            SkyDriveFolder folder = await LoadFolder(uniqueId, fFolderOnly, true, bLoadFileSynchroniously);
            
            return folder;
        }

        //fetch folder metadata only
        public async Task<SkyDriveFolder> GetFolder(string uniqueId)
        {
            if (uniqueId.Equals("Root"))
            {
                uniqueId = Root;
            }

            SkyDriveFolder folder = await LoadFolder(uniqueId, true, false);
            return folder;
        }

        private async Task InitAuth()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                
                authClient = new LiveAuthClient();
                LiveLoginResult authResult = await authClient.LoginAsync(new List<string>() { "wl.basic", "wl.skydrive_update" }); //, "wl.contacts_skydrive" "wl.signin", "wl.basic",
                if (authResult.Status == LiveConnectSessionStatus.Connected)
                {
                    App.Session = authResult.Session;
                }
                await LoadProfile();
            }
        }

        private async Task LoadProfile()
        {
            LiveConnectClient client = new LiveConnectClient(App.Session);
            LiveOperationResult liveOpResult = await client.GetAsync("me");
            dynamic dynResult = liveOpResult.Result;
            App.UserName = dynResult.name;
            _userName = dynResult.name;

            App.CID = dynResult.id;
            _imageUrl = "https://apis.live.net/v5.0/" + App.CID + "/picture";
            await LoadData();
        }

        private SkyDriveDataSource()
        {
        }

        public async Task LoadData()
        {
           LiveConnectClient client = new LiveConnectClient(App.Session);
            //folder structure won't exist for new user unless user delete it.
           bool bFolderStructureExists = false;
           
           object value = GetSetting(constants.bookmark);
           if (!Object.Equals(value, null) && !object.Equals(value, string.Empty))
           {
              _root = Convert.ToString(value);
                
               //check that folder structure exists
                try
                {
                    LiveOperationResult albumOperationResult = await client.GetAsync(_root);
                    dynamic albumResult = albumOperationResult.Result;
                    object value1 = GetSetting(constants.AppName);
                    //varify the RoamingFavoriteApp is the parent of Bookmark folder
                    if (!Object.Equals(value1, null) && !object.Equals(value1, string.Empty))
                    {
                        if (string.Equals(albumResult.parent_id, Convert.ToString(value1), StringComparison.CurrentCultureIgnoreCase))
                        {
                            bFolderStructureExists = true;
                           // LoadSkyDriveObjects(_root, RootFolder);
                        }
                    }
                    
                }
                catch (Microsoft.Live.LiveConnectException)
                {
                    //resource does not exists. recreate folder structure
                    bFolderStructureExists = false;
                }
            }
            if (!bFolderStructureExists)
            {
                await CreateFolderStructure();
            }
        }

        public async Task CreateFolderStructure()
        {
        //New installation:- application is running first time on this system. 
        //check for new user. Existing user will have following folder structure:
        // skydrive root
        //            --RoamingFavoriteApp
        //                              --Bookmark
            LiveConnectClient client = new LiveConnectClient(App.Session);
            bool bRomaingFavExists = false;
            bool bBookmarkExists = false;
            LiveOperationResult albumOperationResult = await client.GetAsync("me/skydrive/files?filter=folders");
            dynamic albumResult = albumOperationResult.Result;
            foreach (dynamic album in albumResult.data)
            {
                if (album.name == constants.AppName)
                {
                    bRomaingFavExists = true;
                    //retrieve the folderid of RoamingFavoriteApp folder
                     object value1 = GetSetting(constants.AppName);
                    //varify the RoamingFavoriteApp is the parent of Bookmark folder
                     if (Object.Equals(value1, null) || object.Equals(value1, string.Empty))
                     {
                         //folder exists but setting on this computer is not set
                         SaveSetting(constants.AppName, album.id);
                     }
                     else if (!string.Equals(Convert.ToString(value1), album.id))
                     {
                         //update the setting with correct value
                         SaveSetting(constants.AppName, album.id);
                     }
                    LiveOperationResult OperationResult = await client.GetAsync(album.id + "/files?filter=folders");
                    dynamic Result = OperationResult.Result;
                    foreach (dynamic folder in Result.data)
                    {
                        if (folder.name == constants.bookmark)
                        {
                            bBookmarkExists = true;
                            _root = folder.id;
                            SaveSetting(constants.bookmark, folder.id);
                            break;
                        }
                    }
                    break;
                }
            }


            if (!bRomaingFavExists)
            {
                OnNewSignupEvent(new NewSingupEventArguments(NewSignupState.NewUser));
                string folderid = await CreateFolder("me/skydrive", constants.AppName);
                SaveSetting(constants.AppName, folderid);
                string subfolderid = await CreateFolder(folderid, constants.bookmark);
                SaveSetting(constants.bookmark, subfolderid);
                _root = subfolderid;
                OnNewSignupEvent(new NewSingupEventArguments(NewSignupState.FavoriteFolderCreated));
            }
            else if (!bBookmarkExists)
            {
                OnNewSignupEvent(new NewSingupEventArguments(NewSignupState.NewUser));
                string subfolderid = await CreateFolder(Convert.ToString(GetSetting(constants.AppName)), constants.bookmark);
                SaveSetting(constants.bookmark, subfolderid);
                _root = subfolderid;
                OnNewSignupEvent(new NewSingupEventArguments(NewSignupState.FavoriteFolderCreated));
            }
        }

        public static void SaveSetting(string key, string value)
        {
            //create application specific container if does not exists
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var container = roamingSettings.CreateContainer(constants.AppName, ApplicationDataCreateDisposition.Always);
            if (!object.Equals(container, null))
            {
                container.Values[key] = value;
            }
        }

        public async Task<bool> UploadFile(string folderid, string Url, string title)
        {
            bool success = false;
            try
            {
                string filename = title.Trim();
                filename = filename.Replace(":", "");
                filename = filename.Replace(";", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("|", "");
                filename = filename.Replace("\\", "");
                filename = filename.Replace("/", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("\"", "");
                if (filename.Length > 60)
                {
                    filename = filename.Substring(0, 56).Trim() + ".url";
                   
                }
                else
                {
                    filename = filename + ".url";
                }

                
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult liveOperationResult = await liveClient.PutAsync(folderid + "/files/" + filename, "[InternetShortcut]" + Environment.NewLine + "URL=" + Url);
                JsonObject jsonObject = JsonObject.Parse(liveOperationResult.RawResult);
                success = true;
               
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                throw ex;
            }
            catch (LiveConnectException exception)
            {
                throw exception;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return success;
        }

        public async Task<bool> UploadFile(SkyDriveFile file)
        {
            bool success = false;
            try
            {
                string filename = file.Title.Trim();
                filename = filename.Replace(":", "");
                filename = filename.Replace(";", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("|", "");
                filename = filename.Replace("\\", "");
                filename = filename.Replace("/", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("\"", "");
                if (filename.Length > 60)
                {
                    filename = filename.Substring(0, 56).Trim() + ".url";

                }
                else
                {
                    filename = filename + ".url";
                }

                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult liveOperationResult = await liveClient.PutAsync(file.Parent_id + "/files/" + filename, "[InternetShortcut]" + Environment.NewLine + "URL=" + file.URL);
                JsonObject jsonObject = JsonObject.Parse(liveOperationResult.RawResult);
                file.UniqueId = jsonObject.GetNamedString("id", string.Empty);
                file.Source = jsonObject.GetNamedString("source", string.Empty);
                file.Title = jsonObject.GetNamedString("name", filename);
                if (Path.GetExtension(file.Title).Equals(".url", StringComparison.CurrentCultureIgnoreCase))
                {
                    file.IsInternetShortcut = true;
                    file.FileExtenstion = ".url";
                    int urlExtIndex = file.Title.LastIndexOf(".url");
                    if (urlExtIndex >= 0)
                    {
                        file.Title = file.Title.Substring(0, urlExtIndex);
                    }
                }
                
                success = true;

            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                throw ex;
            }
            catch (LiveConnectException exception)
            {
                throw exception;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return success;
        }

        public async Task<bool> UpdateFile(SkyDriveFile file)
        {
            bool success = false;
            try
            {
                
                string filename = file.Title.Trim();
                filename = filename.Replace(":", "");
                filename = filename.Replace(";", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("|", "");
                filename = filename.Replace("\\", "");
                filename = filename.Replace("/", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("\"", "");
                if (filename.Length > 60)
                {
                    filename = filename.Substring(0, 56).Trim() + ".url";

                }
                else
                {
                    filename = filename + ".url";
                }

                var fileData = new Dictionary<string, object>();
                fileData.Add("name", filename);

                    LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                    await liveClient.PutAsync(file.UniqueId, fileData);
                    file.Title = filename.Remove(filename.Length-4,4);    
                    success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return success;
        }

        public async Task<bool> DeleteFile(string uniqueid)
        {
            bool result = false;
            LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.DeleteAsync(uniqueid);
                result = true;
            return result;
        }

        public async Task<bool> MoveItem(string sourceUniqueid, string destinationUniqueid)
        {
            bool result = false;
            LiveConnectClient liveClient = new LiveConnectClient(App.Session);
            LiveOperationResult operationResult =
                await liveClient.MoveAsync(sourceUniqueid, destinationUniqueid);
            //operationResult.Result
            result = true;
            return result;
        }

        public async Task<SkyDriveFolder> Refresh(string uniqueid)
        {
            return await this.GetGroups(uniqueid);
        }

        public async Task<string> CreateFolder(string parentid, string folderName)
        {
            try
            {
                var folderData = new Dictionary<string, object>();
                folderData.Add("name", folderName);
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.PostAsync(parentid, folderData);
                dynamic result = operationResult.Result;
                return result.id;
            }
            catch (LiveConnectException)
            {
                return string.Empty;
            }
        }

        public async Task<string> CreateFolder(SkyDriveFolder folder)
        {
            try
            {
                var folderData = new Dictionary<string, object>();
                folderData.Add("name", folder.Title);
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.PostAsync(folder.Parent_id, folderData);
                dynamic result = operationResult.Result;
                return result.id;
            }
            catch (LiveConnectException)
            {
                return string.Empty;
            }
        }

        public async Task<string> UpdateFolder(string uniqueid, string folderName)
        {
            try
            {
                var folderData = new Dictionary<string, object>();
                folderData.Add("name", folderName);
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.PutAsync(uniqueid, folderData);
                dynamic result = operationResult.Result;
                return result.id;
            }
            catch (LiveConnectException)
            {
                return string.Empty;
            }
        }
 
        public static object GetSetting(string key)
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var container = roamingSettings.CreateContainer(constants.AppName, ApplicationDataCreateDisposition.Always);
            if (!object.Equals(container, null))
            {
                Object value = container.Values[key];
                return value;
            }
            else
                return null;
        }

        public static void DeleteContainer()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Containers.ContainsKey(constants.AppName))
            {
                roamingSettings.DeleteContainer(constants.AppName);
            }
        }

        public async Task<SkyDriveFile>GetFile(string path)
        {
            LiveConnectClient client = new LiveConnectClient(App.Session);
            LiveOperationResult albumOperationResult = await client.GetAsync(path);
            dynamic album = albumOperationResult.Result;
            int size = Convert.ToInt32(Convert.ToString(album.size));
            var item = new SkyDriveFile(album.id, album.name, album.description, album.size, album.parent_id,
                    SkyDriveItemType.File, album.updated_time, album.source);
            return item;
        }

        // Returns a list of file and folders who are modified after the updated_date date.
        // This function works of folder returned from server. so i won't return any deleted item. 
        // Return: a tuple containg:
        //                          1) list of items which have date greater than last modified date of folder at local machine
        //                          2) list of items whoes date are not changed. means unchanged items
        // it is the responsiblity of the caller to determine deleted files by iterating the folder items at local against both the returned list. 
        // Items which are not found in either of the returned list will be considered deleted.


        public async Task<Tuple<List<SkyDriveDataCommon>, List<string>>> GetChangedFolderAndFiles(string path, string updated_time)
        {
            LiveConnectClient client = new LiveConnectClient(App.Session);
            path = path + "/files";
            LiveOperationResult albumOperationResult = await client.GetAsync(path);
            dynamic albumResult = albumOperationResult.Result;
            List<SkyDriveDataCommon> changedItems = new List<SkyDriveDataCommon>();
            List<string> unChanagedItems = new List<string>();
           // List<SkyDriveDataCommon> changedItems = new List<SkyDriveDataCommon>();
            foreach (dynamic album in albumResult.data)
            {
                //Comparedate returns 1 if LHS date is greater than RHS date
                if (Utility.CompareDates(album.updated_time, updated_time) == 1)
                {
                    SkyDriveDataCommon item = null;
                    if (album.type == "folder")
                    {
                        try
                        {
                            int count = Convert.ToInt32(Convert.ToString(album.count));
                            int size = Convert.ToInt32(Convert.ToString(album.size));
                            item = new SkyDriveFolder(album.id, album.name, album.description, album.size, album.parent_id,
                                SkyDriveItemType.Folder, album.updated_time, album.count);
                           
                            //changedItems.Add(group);
                        }
                        catch (Exception ex)
                        {
                            string eexp = ex.ToString();
                        }
                    }
                    else
                    {
                        int size = Convert.ToInt32(Convert.ToString(album.size));
                        item = new SkyDriveFile(album.id, album.name, album.description, album.size, album.parent_id,
                                SkyDriveItemType.File, album.updated_time, album.source);
                        //changedItems.Add(item);
                    }
                    changedItems.Add(item);
                }
                else
                {
                    unChanagedItems.Add(album.id);
                }
                   
            }
                
            return new Tuple<List<SkyDriveDataCommon>,List<string>>(changedItems, unChanagedItems);
        }

        //This function will serve following purpose:
        // 1) request for current folder only -- bFolderOnly = false and sSubFolders = false
        // 2) request for all subfolder and current folder -- bFolderOnly = true and sSubFolders = true
        // 3) request for current folder and its child files and folders (1st level only) -- bFolderOnly = false and sSubFolders = true
        private async Task<SkyDriveFolder> LoadFolder(string path, bool bFolderOnly = false, bool bSubFolders = true, bool bLoadFileSynchroniously = false)
        {
            LiveConnectClient client = new LiveConnectClient(App.Session);
            LiveOperationResult albumOperationResult = await client.GetAsync(path);
            dynamic albumResult = albumOperationResult.Result;
            //if(albumResult)
            int count;
            int size;
            try
            {
                count = Convert.ToInt32(Convert.ToString(albumResult.count));
                size = Convert.ToInt32(Convert.ToString(albumResult.size));
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
            SkyDriveFolder folder = new SkyDriveFolder(albumResult.id, albumResult.name, albumResult.description, albumResult.size, albumResult.parent_id,
                SkyDriveItemType.Folder, albumResult.updated_time, albumResult.count);
            if (!bSubFolders)
            {
                return folder;
            }

            if (bFolderOnly && bSubFolders)
            {
                path = path + "/files?filter=folders";
            }
            else
            {
                //request for both files and folders
                path = path + "/files";
            }

            albumOperationResult = await client.GetAsync(path);
            albumResult = albumOperationResult.Result;
            if (!bFolderOnly)
            {
                if (albumResult.data.Count >= 1)
                {
                    AdItem ad;
                    if (albumResult.data.Count % 5 == 0)
                    {
                        ad = new AdItem("ad", SkyDriveItemType.AdBig);
                    }
                    else
                    {
                        ad = new AdItem("ad", SkyDriveItemType.AdSmall);
                    }
                    folder.Ads.Add(ad);
                }
            }
            foreach (dynamic album in albumResult.data)
            {
                
                if (album.type == "folder")
                {
                    try
                    {
                        count = Convert.ToInt32(Convert.ToString(album.count));
                        size = Convert.ToInt32(Convert.ToString(album.size));
                        var group = new SkyDriveFolder(album.id, album.name, album.description, album.size, album.parent_id,
                            SkyDriveItemType.Folder, album.updated_time, album.count);
                        folder.Subalbums.Add(group);
                    }
                    catch (Exception ex)
                    {
                        string eexp = ex.ToString();
                    }
                }
                else
                {
                    
                    //var skFile = searchResultContainer.Items.FirstOrDefault(sf => sf.UniqueId == album.id);
                    //if (object.Equals(skFile, null))
                    //{
                        size = Convert.ToInt32(Convert.ToString(album.size));
                        int commentcount = Convert.ToInt32(Convert.ToString(album.comments_count));
                        var item = new SkyDriveFile(album.id, album.name, album.description, album.size, album.parent_id,SkyDriveItemType.File, album.updated_time, album.source);
                        if (item.FileExtenstion == ".url")
                        {
                            if (bLoadFileSynchroniously)
                            {
                                await LoadFavicons(item);
                            }
                            else
                            {
                                LoadFavicons(item);
                            }
                            folder.Items.Add(item);
                            // add file to search container also. For cases like 1) search limit 2) file added after search container created.
                            // also dublicate entry will not harm
                            //searchResultContainer.Items.Add(item);
                        }
                        else
                        {
                            folder.OtherFileItems.Add(item);
                        }
                    //}
                    //else
                    //{
                    //    if (string.IsNullOrEmpty(skFile.URL))
                    //    {
                    //        if (bLoadFileSynchroniously)
                    //        {
                    //            await LoadFile(skFile);
                    //        }
                    //        else
                    //        {
                    //            LoadFile(skFile);
                    //        }
                    //        folder.Items.Add(skFile);
                    //    }
                    //    else
                    //    {
                    //        folder.Items.Add(skFile);
                    //    }
                    //}

                }
            }
            return folder;
        }

        //public async Task<SkyDriveFolder> Search(string searchString)
        //{
        //    LiveConnectClient client = new LiveConnectClient(App.Session);
        //    string path = App.CID + "/skydrive/search?q=" + searchString;
        //    SkyDriveFolder searchResult = new SkyDriveFolder("Search Results for: " + searchString);
        //    LiveOperationResult albumOperationResult = await client.GetAsync(path);
        //    dynamic albumResult = albumOperationResult.Result;
        //    foreach (dynamic album in albumResult.data)
        //    {
        //        if (album.type == "folder")
        //        {
        //            //do nothing --- leaving this block in case we want to show folders also
        //        }
        //        else
        //        {
        //            SkyDriveFile sf = SkyDriveDataSource.searchResultContainer.Items.FirstOrDefault(sdfile => sdfile.UniqueId == album.id);
        //            if (object.Equals(sf, null))
        //            {
        //                var item = new SkyDriveFile(album.id, album.name, album.description, 0, album.parent_id,
        //                    SkyDriveItemType.File, album.updated_time, album.source);
        //                if (item.FileExtenstion == ".url")
        //                {
        //                    LoadFile(item);
        //                    searchResult.Items.Add(item);
        //                }
        //            }
        //            else
        //            {
        //                LoadFile(sf);
        //                searchResult.Items.Add(sf);
        //            }
        //        }
        //    }
        //    return searchResult;
        //}

        //public async Task<SkyDriveFolder> Search()
        //{
        //    //Add all cached files in search container

        //    FavoriteFileStore fileStore = await FavoriteFileStore.GetInstance();
        //    LiveConnectClient client = new LiveConnectClient(App.Session);
        //    string path = App.CID + "/skydrive/search?q=.url";
        //    LiveOperationResult albumOperationResult = await client.GetAsync(path);
        //    dynamic albumResult = albumOperationResult.Result;
        //    //int counter = 0;
        //    foreach (dynamic album in albumResult.data)
        //    {
        //        if (album.type == "folder")
        //        {
        //            //do nothing --- leaving this block in case we want to show folders also
        //        }
        //        else
        //        {
        //            var item = new SkyDriveFile(album.id, album.name, album.description, 0, album.parent_id,
        //                SkyDriveItemType.File, album.updated_time, album.source);
        //            if (item.FileExtenstion == ".url")
        //            {
        //                //load from cache if shortcut file already exists there.
        //                //fileStore.IsFileExists(ref item);
        //                searchResultContainer.Items.Add(item);
        //            }
        //        }
        //    }
        //    return searchResultContainer;
        //}

        /// <summary>
        /// SearchSuggestions
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="suggestions"></param>
        /// <returns></returns>
        //public void SearchSuggestions(string searchString, SearchSuggestionCollection suggestions)
        //{
        //    try
        //    {
        //        if (searchString != "")
        //        {
        //            var searchCollection = SkyDriveDataSource.searchResultContainer.Items.Where(sf => System.Text.RegularExpressions.Regex.IsMatch(sf.Title, searchString, System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        //            foreach (SkyDriveFile file in searchCollection)
        //            {
        //                suggestions.AppendQuerySuggestion(file.Title);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string err = e.Message;
        //    }
        //}

        public async Task LoadFile(SkyDriveFile file)
        {
            if (string.IsNullOrEmpty(file.URL))
            {
                if (string.IsNullOrEmpty(file.URL) && file.FileExtenstion == ".url")
                {
                    LiveOperationResult result = null;

                    try
                    {
                        LiveConnectClient client = new LiveConnectClient(App.Session);
                        result = await client.GetAsync(string.Format("{0}/content", file.UniqueId));//, this.cts.Token, progressHandler);
                    }
                    catch (Exception ex)
                    {
                        string exp = ex.ToString();
                        return;
                    }
                    file.Source = Convert.ToString(result.Result["location"]);
                    await LoadFavicons(file);
                }
            }
           
        }

        public async Task LoadFavicons(SkyDriveFile file)
        {
            try
            {
            //FavoriteFileStore fileStore = await FavoriteFileStore.GetInstance();
            //if (fileStore.IsFileExists(ref file))
            //{
            //    try
            //    {
            //        file.TileBackground = new SolidColorBrush(file.ImageDominantColor);
            //    }
            //    catch { }
            //    return;
            //}
            //else
            //{
                var httpClient = new HttpClient();
                var url = new Uri(file.Source);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await httpClient.SendAsync(httpRequestMessage);
                string resp = await response.Content.ReadAsStringAsync();
                string[] collStrings = resp.Split('\n');
                bool internetShortcut = false;
                string internetUrl = string.Empty;
                foreach (string s in collStrings)
                {
                    if (internetShortcut)
                    {
                        if (s.Contains("URL="))
                        {
                            internetUrl = s.Replace("URL=", "");
                        }
                    }
                    if (s.Contains("[InternetShortcut]"))
                    {
                        internetShortcut = true;
                    }
                }
                if (Uri.IsWellFormedUriString(internetUrl, UriKind.Absolute))
                {
                     Uri temp = new Uri(internetUrl);
                     string favicon = temp.Scheme + @"://" + temp.Host;
                     file.ImagePath = @"http://www.google.com/s2/favicons?domain=" + temp.Host; 
                     //file.ImagePath = @"http://getfavicon.appspot.com/" + System.Net.WebUtility.UrlEncode(favicon) + @"?defaulticon=https://upload.wikimedia.org/wikipedia/commons/b/b9/RoamingFavoriteIcon.png";
                    //file.ImagePath = internetUrl;
                     //   file.ImagePath = await LoadFavIconUrlFromPageUrl(internetUrl);

                        if (!string.IsNullOrEmpty(file.ImagePath))
                        {
                            try
                            {
                                file.ImageDominantColor = await Utility.GetImageColor(file.ImagePath);
                            }
                            catch { }
                        }
                        else
                        {
                            file.ImagePath = @"ms-appx:///Assets/fileIcon.png";
                            try
                            {
                                file.ImageDominantColor = Color.FromArgb(255, 40, 122, 237);
                            }
                            catch { }
                        }
                        try
                        {
                            file.TileBackground = new SolidColorBrush(file.ImageDominantColor);
                        }
                        catch { }
                        file.URL = internetUrl;
                }

                //fileStore.IsFileExists(ref file);
           // }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// LoadFavIconUrlFromPageUrl
        /// </summary>
        /// <param name="pageURL"></param>
        /// <returns></returns>
        public async Task<string> LoadFavIconUrlFromPageUrl(string pageURL)
        {
            var favIcon = "/favicon.ico";
            string favIconUrl = null;

            try
            {
                var httpClient = new HttpClient();
                var url = new Uri(pageURL);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await httpClient.SendAsync(httpRequestMessage);
                string resp = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resp))
                {
                    var host = url.Host;
                    var parentDomain = host.Substring(host.LastIndexOf('.', host.LastIndexOf('.') - 1) + 1);

                    favIconUrl = url.Scheme + "://www." + parentDomain + favIcon;

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(resp);

                    IEnumerable<HtmlNode> linkHrefNode = doc.DocumentNode.Descendants().Where(n => n.Name == "link");
                    foreach (HtmlNode link in linkHrefNode)
                    {
                        try
                        {
                            HtmlAttribute reference = link.Attributes["rel"];

                            if (string.Equals(reference.Value, "shortcut icon", StringComparison.CurrentCultureIgnoreCase))
                            {
                                favIcon = link.Attributes["href"].Value;

                                try
                                {
                                    Uri favIconUrlTemp = new Uri(favIcon);
                                    favIconUrl = "http://" + favIconUrlTemp.Host + favIconUrlTemp.PathAndQuery;
                                }
                                catch (Exception)
                                {
                                    var requestHost = response.RequestMessage.RequestUri.Host;
                                    var requestParentDomain = requestHost.Substring(requestHost.LastIndexOf('.', requestHost.LastIndexOf('.') - 1) + 1);
                                    favIconUrl = response.RequestMessage.RequestUri.Scheme + "://www." + requestParentDomain + "/" + favIcon;
                                }

                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }

            return favIconUrl;
        }

        //comment functionality
        public async Task<ObservableCollection<Comment>> ReadComments(string uniqueId)
        {
            ObservableCollection<Comment> commentList = new ObservableCollection<Comment>();
            LiveConnectClient client = new LiveConnectClient(App.Session);
            LiveOperationResult operationResult = await client.GetAsync(string.Format("{0}/comments", uniqueId));
            dynamic result = operationResult.Result;
            dynamic data = result.data;
            //StringBuilder comments = new StringBuilder("Comments:\n");
            if (data.Count > 0)
            {
                foreach (dynamic datum in data)
                {
                    commentList.Add(new Comment(datum.id, new From(datum.from.name, datum.from.id), datum.message, datum.created_time));
                }
            }
            return commentList;
        }

        public async Task<bool> CreateComment(string uniqueId, string message)
        {
            bool result = false;
            try
            {
                var commentData = new Dictionary<string, object>();
                commentData.Add("message", message);
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.PostAsync(string.Format("{0}/comments", uniqueId), commentData);
                result = true;
            }
            catch (LiveConnectException)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteComment(string commentid)
        {
            bool result = false;
            try
            {
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.DeleteAsync(commentid);
                result = true;
            }
            catch (LiveConnectException)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> ModifyDescription(string uniqueId, string description)
        {
            bool result = false;
            try
            {
                var fileData = new Dictionary<string, object>();
                fileData.Add("description", description);
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult =
                    await liveClient.PutAsync(uniqueId, fileData);
                result = true;
            }
            catch (LiveConnectException)
            {
                result = false;
            }
            return result;

        }

        // Declare the PropertyChanged event.
        public event PropertyChangedEventHandler PropertyChanged;

        // NotifyPropertyChanged will raise the PropertyChanged event, 
        // passing the source property that is being updated.
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}