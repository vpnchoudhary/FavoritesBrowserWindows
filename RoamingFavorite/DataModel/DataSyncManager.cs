using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Windows.Data.Xml.Dom;
using System.Threading.Tasks;
using RoamingFavorite.Common;
using System.Runtime.Serialization;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Search;
using Newtonsoft.Json;
using RoamingFavorite.Common;


namespace RoamingFavorite.Data
{
    class FavoritesItem
    {
        public string UniqueId {get; set;}
        public string Title { get; set; }
        public string Parent_id { get; set; }
        public string Updated_time { get; set; }
        public int Size { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string ImageDominantColor { get; set; }
        public ItemState SyncState { get; set; }
    }

    class FavoritesFile : FavoritesItem
    {
        public string FileExtenstion { get; set; }
        public bool IsInternetShortcut { get; set; }
        public string Source { get; set; }
        public string URL { get; set; }
    }

    class FavoritesFolder : FavoritesItem
    {
        public int Count { get; set; }
        public IList<FavoritesFile> Items { get; set; }
        public IList<FavoritesFolder> Subalbums { get; set; }
    }

    enum ItemSyncState
    {
        New,
        Updated,
        Deleted,
        NoChange
    }

    enum DocumentLocation{
        Local,
        Roaming
    }

    abstract class DataSyncManager
    {
        protected StorageFile storageFile;
        protected string fileName;
        //determine if this document need to be synced with cloud
        protected DocumentLocation location;
        protected abstract Task loadData();
        
        public abstract Task SaveData();
       

        public abstract Task CreateDataStore();
        //public abstract void AddItem();
        //public abstract void DeleteItem();
        //public abstract void UpdateItem();
        public abstract Task Sync();
        //public abstract void GetItem();
        //public abstract void GetCollection();

    }

    class FavoritesDataSyncManager : DataSyncManager
    {
        //public SkyDriveFolder rootFavoritesFolder;
        public SkyDriveFolder CurrentFolder = null;
        private static FavoritesDataSyncManager instance = null;
        private static AsyncLock mutex = new AsyncLock();
        private Dictionary<string, SkyDriveFolder> FolderCollection = new Dictionary<string, SkyDriveFolder>();
        private Dictionary<string, SkyDriveFile> FileCollection = new Dictionary<string, SkyDriveFile>();
        private FavoritesDataSyncManager()
        {
            fileName = "FavoritesDataFile.xml";
        }

        public static async Task<FavoritesDataSyncManager> GetInstance(DocumentLocation docLocation)
        {
            //var mutex = new AsyncLock();
            using (await mutex.LockAsync())
            {
                if (instance == null)
                {
                    instance = new FavoritesDataSyncManager();
                    instance.location = docLocation;
                    await instance.loadData();
                }
            }
            return instance;
        }

        protected override async Task loadData()
        {
            storageFile = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            
            string json = await FileIO.ReadTextAsync(storageFile);
            if (!string.IsNullOrEmpty(json))
            {
                var jsonFavorites = JsonConvert.DeserializeObject<FavoritesFolder>(json);
                //now create rootFavoritesFolder from cached file
                //populateRootFavoritesFolder(rootFavoritesFolder, jsonFavorites);
                populateFavoritesFromCache(jsonFavorites);
                Sync();
                
            }
            else
            {
                await this.CreateDataStore();
            }
        }

       
        private void populateFavoritesFromCache(FavoritesFolder folder)
        {
            var skyFolder = CreateSkyDriveFolderFromJson(folder);
            foreach (var item in folder.Items)
            {
                var file = CreateSkyDriveFileFromJson(item);
                skyFolder.Items.Add(file);
                FileCollection.Add(file.UniqueId, file);
            }
            foreach (var sourcefolder in folder.Subalbums)
            {
                var destinationfolder = CreateSkyDriveFolderFromJson(sourcefolder);
                skyFolder.Subalbums.Add(destinationfolder);
            }
            FolderCollection.Add(skyFolder.UniqueId, skyFolder);
            foreach (var sourcefolder in folder.Subalbums)
            {
                populateFavoritesFromCache(sourcefolder);
            }
        }

        public override async Task Sync()
        {
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            SkyDriveFolder folderAtSource = await source.GetFolder(source.Root);
            var isDataChanged = await Sync(source.Root, FolderCollection[source.Root].Updated_time, folderAtSource);
            if (isDataChanged)
            {
                var cachedFolder = FolderCollection[source.Root];
                cachedFolder.Title = folderAtSource.Title;
                cachedFolder.Updated_time = folderAtSource.Updated_time;
                cachedFolder.Count = folderAtSource.Count;
                cachedFolder.Size = folderAtSource.Size;
            }
        }
        private async Task<bool> Sync(string folderId, string updated_time, SkyDriveFolder folderAtServer)
        {
            bool dataChanaged = false;
            //case 1 
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            SkyDriveFolder folderAtSource = folderAtServer;
            if (folderAtSource == null)
            {
                folderAtSource = await source.GetFolder(folderId);
            }
            if ((updated_time == folderAtSource.Updated_time) &&
                (FolderCollection[folderId].Items.Count + FolderCollection[folderId].Subalbums.Count + FolderCollection[folderId].OtherFileItems.Count == folderAtSource.Count))
            {
                //no change at root, return the cached folder
                dataChanaged = false;
                //int count = FolderCollection[folderId].Items.Count;
                foreach(var folder in FolderCollection[folderId].Subalbums)
                {
                    //check if there is any child folder which is not being loaded fully
                    if (folder.Count != (folder.Items.Count + folder.Subalbums.Count + folder.OtherFileItems.Count))
                    {
                        SkyDriveFolder itemAtServer = await source.GetFolder(folder.UniqueId);
                        dataChanaged = await Sync(folder.UniqueId, folder.Updated_time, itemAtServer);
                        if (dataChanaged)
                        {
                            var cachedFolder = FolderCollection[folder.UniqueId];
                            cachedFolder.Title = itemAtServer.Title;
                            cachedFolder.Updated_time = itemAtServer.Updated_time;
                            cachedFolder.Count = itemAtServer.Count;
                            cachedFolder.Size = itemAtServer.Size;
                        }
                    }
                }
            }
            else
            {
                
                Dictionary<string, bool> itemTobeDeleted = new Dictionary<string,bool>(FolderCollection[folderAtSource.UniqueId].Count);
                foreach (var item in FolderCollection[folderAtSource.UniqueId].AllItems)
                {
                    if (item.Type == SkyDriveItemType.File || item.Type == SkyDriveItemType.Folder)
                    {

                        itemTobeDeleted.Add(item.UniqueId, true);
                    }
                }
                
                // there are changes...determine which folder/file to sync or delete
                // result of GetChangedFolderAndFiles return a tuple which contains list of changed and unchanged files
                var changedItems = await source.GetChangedFolderAndFiles(folderAtSource.UniqueId, updated_time);
                
                foreach(var item in changedItems.Item1)
                {
                    dataChanaged = true;
                    //exclude items which were changed from list of items to be deleted
                    if (itemTobeDeleted.ContainsKey(item.UniqueId))
                    {
                        itemTobeDeleted[item.UniqueId] = false;
                    }
                    if(item.Type == SkyDriveItemType.Folder)
                    {
                        if(!FolderCollection.ContainsKey(item.UniqueId))
                        {
                            //add Folder to parent container
                            FolderCollection[item.Parent_id].Subalbums.Add((SkyDriveFolder)item);
                            FolderCollection.Add(item.UniqueId, (SkyDriveFolder)item);
                        }
                        else
                        {
                            // sync changed folder
                            await Sync(item.UniqueId, updated_time, (SkyDriveFolder)item);
                            //update changed folder properties at both parent and itself
                            var cachedFolder = FolderCollection[item.UniqueId];
                            var cachedFolderParent = FolderCollection[item.Parent_id];
                            SkyDriveFolder folderToUpdate = cachedFolderParent.Subalbums.FirstOrDefault(a => a.UniqueId == item.UniqueId);
                            if(folderToUpdate != null)
                            {
                                int index = cachedFolderParent.Subalbums.IndexOf(folderToUpdate);
                                cachedFolderParent.Subalbums.RemoveAt(index);
                                cachedFolderParent.Subalbums.Insert(index, (SkyDriveFolder)item);
                            }
                            cachedFolder.Title = item.Title;
                            cachedFolder.Updated_time = item.Updated_time;
                            cachedFolder.Count = ((SkyDriveFolder)item).Count;
                            cachedFolder.Size = ((SkyDriveFolder)item).Size;
                            cachedFolder.Parent_id = item.Parent_id;
                            
                        }
                    }
                    else
                    {
                        source.LoadFavicons((SkyDriveFile)item);
                        SkyDriveFolder parent = FolderCollection[item.Parent_id];
                        SkyDriveFile fileitem = parent.Items.FirstOrDefault(a => a.UniqueId == item.UniqueId);
                        if(fileitem != null)
                        {
                            int index = parent.Items.IndexOf(fileitem);
                            parent.Items.RemoveAt(index);
                            parent.Items.Insert(index, (SkyDriveFile)item);
                        }
                        else
                        {
                            parent.Items.Add((SkyDriveFile)item);
                        }
                    }
                }
                //exclude unchanged items from the list of tobedeleted items
                foreach(string s in changedItems.Item2)
                {
                    itemTobeDeleted[s] = false;
                }
                SkyDriveFolder folder = FolderCollection[folderAtSource.UniqueId];
                foreach(var item in itemTobeDeleted)
                {
                    if(item.Value)
                    {
                        var t = folder.AllItems.FirstOrDefault(a=> a.UniqueId == item.Key);
                       if(t!=null)
                       {
                            if(t.Type == SkyDriveItemType.Folder)
                            {
                                folder.Subalbums.Remove((SkyDriveFolder)t);
                            }
                            else
                            {
                                folder.Items.Remove((SkyDriveFile)t);
                            }
                       }
                    }
                }
            }
            return dataChanaged;
        }

        public async Task<SkyDriveFolder> GetRoot()
        {
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            return await GetCollection(source.Root);
        }

        public async Task<SkyDriveFolder> GetCollection(string folderId)
        {
            // check if folder exists in collection
            SkyDriveFolder result;
            if (!FolderCollection.TryGetValue(folderId, out result))
            {
                result = await GetFavoritesFromServer(folderId);
                FolderCollection[result.UniqueId] = result;
            }
            else
            {
                if (FolderCollection[folderId].Count != FolderCollection[folderId].Items.Count + FolderCollection[folderId].Subalbums.Count + FolderCollection[folderId].OtherFileItems.Count)
                {
                    result = await GetFavoritesFromServer(folderId);
                    FolderCollection[result.UniqueId] = result;
                }
            }
            return result;
        }

        public async Task<SkyDriveFolder> GetCollection(SkyDriveFolder folder)
        {
            return await GetCollection(folder.UniqueId);
        }

        
        public override async Task CreateDataStore()
        {
            if (this.location == DocumentLocation.Local)
            {
                 var rootFavoritesFolder = new SkyDriveFolder(constants.bookmark);
                 FolderCollection.Add(rootFavoritesFolder.UniqueId, rootFavoritesFolder);
            }
            else
            {
                await this.GetFavoritesFromServer();
            }
        }

        async Task GetFavoritesFromServer()
        {
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            var rootFavoritesFolder = await source.GetGroups("Root");
            FolderCollection.Add(rootFavoritesFolder.UniqueId, rootFavoritesFolder);
            FetchData(rootFavoritesFolder);
        }

        async Task<SkyDriveFolder> GetFavoritesFromServer(string folderId)
        {
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            var newfolder = await source.GetGroups(folderId);
            //FetchData(folder);
            return newfolder;
        }

        //recursively populate data from source
        private async Task FetchData(SkyDriveFolder folder)
        {
            //at this point we don't have folder contents. We need to make calls to get folder contents:
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            Task<SkyDriveFolder> [] asyncOps = (from item in folder.Subalbums select source.GetGroups(item.UniqueId)).ToArray();
            try
            {
                SkyDriveFolder[] folders = await Task.WhenAll(asyncOps);
                foreach(var fldr in folders)
                {
                    FolderCollection[fldr.UniqueId] =  fldr;
                }
                //folder.Subalbums = new ObservableCollection<SkyDriveFolder>(folders);
                for (int i = 0; i < folder.Subalbums.Count; i++ )
                {
                    await FetchData(folder.Subalbums[i]);
                }
            }
            catch(Exception exc)
            {
                foreach(Task<SkyDriveFolder> faulted in asyncOps.Where(t => t.IsFaulted))
                {
                    
                    // work with faulted and faulted.Exception
                    throw faulted.Exception;
                    //faulted.Start();
                    //faulted.Result
                }
            }
        }

        public async Task<string> AddFolder(SkyDriveFolder folder)
        {
            var source = await SkyDriveDataSource.GetInstance();
            folder.UniqueId = await source.CreateFolder(folder);
            FolderCollection.Add(folder.UniqueId, folder);
            FolderCollection[folder.Parent_id].Subalbums.Add((SkyDriveFolder)folder);
            await RecursivelyUpdateParent(folder);
            return folder.UniqueId;
        }

        private async Task<SkyDriveDataCommon> GetUpdatedMetadata(SkyDriveDataCommon item)
        {
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            SkyDriveDataCommon updatedItem;
            if (item.Type == SkyDriveItemType.Folder)
            {
                updatedItem = await source.GetFolder(item.UniqueId);
            }
            else
            {
                updatedItem = await source.GetFile(item.UniqueId);
            }
            return updatedItem;
        }

        public async Task<bool> AddFile(SkyDriveFile file)
        {
            var source = await SkyDriveDataSource.GetInstance();
            bool result = await source.UploadFile(file);
            if (result)
            {
                FolderCollection[file.Parent_id].Items.Add((SkyDriveFile)file);
                await RecursivelyUpdateParent(file);
            }
            return result;
        }

        private async Task<SkyDriveDataCommon> UpdateItem(SkyDriveDataCommon item)
        { 
            var updatedItem = await GetUpdatedMetadata(item);
            if (updatedItem != null)
            {
                if (item.Type == SkyDriveItemType.File)
                {
                    //file
                    var file = FolderCollection[item.Parent_id].Items.FirstOrDefault(a => a.UniqueId == item.UniqueId);
                    if (file != null)
                    {
                        //update file information from server
                        file.Size = updatedItem.Size;
                        file.Updated_time = updatedItem.Updated_time;
                        file.Parent_id = updatedItem.Parent_id;
                        file.Description = updatedItem.Description;
                        //update parent
                        await RecursivelyUpdateParent(FolderCollection[updatedItem.Parent_id]);
                    }
                }
                else
                {
                    //folder
                    var cachedFolder = FolderCollection[item.UniqueId];
                    var cachedFolderParent = FolderCollection[item.Parent_id];
                    SkyDriveFolder folderToUpdate = cachedFolderParent.Subalbums.FirstOrDefault(a => a.UniqueId == item.UniqueId);
                    if (folderToUpdate != null)
                    {
                        int index = cachedFolderParent.Subalbums.IndexOf(folderToUpdate);
                        cachedFolderParent.Subalbums.RemoveAt(index);
                        cachedFolderParent.Subalbums.Insert(index, (SkyDriveFolder)updatedItem);
                    }
                    cachedFolder.Title = updatedItem.Title;
                    cachedFolder.Updated_time = updatedItem.Updated_time;
                    cachedFolder.Count = ((SkyDriveFolder)updatedItem).Count;
                    cachedFolder.Size = ((SkyDriveFolder)updatedItem).Size;
                    cachedFolder.Parent_id = updatedItem.Parent_id;
                }
            }
            return updatedItem;
        }

        private async Task RecursivelyUpdateParent(SkyDriveDataCommon item)
        {
             var source = await SkyDriveDataSource.GetInstance();
             if (item.UniqueId == source.Root)
             {
                 return;
             }
            var updatedItem = await UpdateItem(item);
            if(updatedItem != null)
            {
                //update parent
                await RecursivelyUpdateParent(FolderCollection[updatedItem.Parent_id]);
            }
        }

        public async Task<string> EditFolder(string uniqueId, string title)
        {
            var source = await SkyDriveDataSource.GetInstance();
            uniqueId = await source.UpdateFolder(uniqueId, title);
            await RecursivelyUpdateParent(FolderCollection[uniqueId]);
            return uniqueId;
        }

        public async Task<bool> EditFile(SkyDriveFile file)
        {
            var source = await SkyDriveDataSource.GetInstance();
            bool result = await source.UpdateFile(file);
            if(result)
            {
                await RecursivelyUpdateParent(file);
            }
            return result;
        }

        public async Task<bool> DeleteItem(IList<SkyDriveDataCommon> items)
        {
            var source = await SkyDriveDataSource.GetInstance();
            bool result = false;
            foreach (SkyDriveDataCommon item in items)
            {
                result = await source.DeleteFile(item.UniqueId);
                if (result)
                {
                    switch (item.Type)
                    {
                        case SkyDriveItemType.File: FolderCollection[item.Parent_id].Items.Remove((SkyDriveFile)item);
                            break;
                        case SkyDriveItemType.Folder: FolderCollection[item.Parent_id].Subalbums.Remove((SkyDriveFolder)item);
                            FolderCollection.Remove(item.UniqueId);
                            break;
                    }
                }
            }
            await RecursivelyUpdateParent(FolderCollection[items[0].Parent_id]);
            return result;
        }

        public async Task<bool> MoveItem(IList<SkyDriveDataCommon> items, SkyDriveFolder destination)
        {
            var source = await SkyDriveDataSource.GetInstance();
            bool result = false;
            string parentid = items[0].Parent_id;
            foreach(var item in items)
            { 
                result = await source.MoveItem(item.UniqueId, destination.UniqueId);
                if (result)
                {
                    
                    // remove moved items from present view
                    // and add them to destination. this addition will not work until we append all results/data to root skydrivefolder
                    switch (item.Type)
                    {
                        case SkyDriveItemType.Folder: FolderCollection[item.Parent_id].Subalbums.Remove((SkyDriveFolder)item);
                            FolderCollection[item.UniqueId].Parent_id = destination.UniqueId;
                            item.Parent_id = destination.UniqueId;
                            FolderCollection[destination.UniqueId].Subalbums.Add((SkyDriveFolder)item);
                            //destination.Subalbums.Add((SkyDriveFolder)item);
                            break;
                        case SkyDriveItemType.File: FolderCollection[item.Parent_id].Items.Remove((SkyDriveFile)item);
                            item.Parent_id = destination.UniqueId;
                            FolderCollection[destination.UniqueId].Items.Add((SkyDriveFile)item);
                            break;
                    }
                    await UpdateItem(item);
                }
            }
            // update tree of source folder
            await RecursivelyUpdateParent(FolderCollection[parentid]);
            // update tree of destination folder
            await RecursivelyUpdateParent(destination);
            return result;
        }

        public async override Task SaveData()
        {
            //skydrive folder to json schema
            FavoritesFolder jsonFolder = new FavoritesFolder();
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            SkyDriveFolder rootFavoritesFolder;
            if (FolderCollection.TryGetValue(source.Root, out rootFavoritesFolder))
            {
                CopySkyDriveFolderToJson(rootFavoritesFolder, jsonFolder);
                PopulateJsonFromRootFavoritesFolder(rootFavoritesFolder, jsonFolder);
                string output = JsonConvert.SerializeObject(jsonFolder, Formatting.Indented);
                await Windows.Storage.FileIO.WriteTextAsync(storageFile, output);
            }
           
        }

        public void GetSeachSuggestions(string searchString, SearchSuggestionCollection suggestions)
        {
            if (searchString != "")
            {
                foreach(var folder in FolderCollection)
                {
                    var searchCollection = folder.Value.Items.
                        Where(sf => (sf.Title.myContains(searchString, StringComparison.OrdinalIgnoreCase) ||sf.URL.myContains(searchString, StringComparison.OrdinalIgnoreCase)));
                    foreach (SkyDriveFile file in searchCollection)
                    {
                        suggestions.AppendQuerySuggestion(file.Title + " | " + file.URL);
                    }
                }
            }
        }

        public SkyDriveFile GetSelectedFile(string searchString)
        {
            SkyDriveFile file = null;
            if (searchString != "")
            {
                string[] col = searchString.Split('|');
                foreach (var folder in FolderCollection)
                {
                    file = folder.Value.Items.
                        FirstOrDefault(sf => (sf.Title.myContains(col[0].Trim(), StringComparison.OrdinalIgnoreCase) && sf.URL.myContains(col[1].Trim(), StringComparison.OrdinalIgnoreCase)));
                    if (file != null)
                        break;
                }
            }
            return file;
        }

        public async Task LoadFile(SkyDriveFile file)
        {
            var source = await SkyDriveDataSource.GetInstance();
            await source.LoadFile(file);
        }

        private void PopulateJsonFromRootFavoritesFolder(SkyDriveFolder skyFolder, FavoritesFolder folder)
        {
            folder.Subalbums = new List<FavoritesFolder>(skyFolder.Subalbums.Count);
            folder.Items = new List<FavoritesFile>(skyFolder.Items.Count);
            foreach (var item in skyFolder.Items)
            {
                var favFile = new FavoritesFile();
                CopySkyDriveFileToJson(item, favFile);
                folder.Items.Add(favFile);
            }
            foreach (var item in skyFolder.Subalbums)
            {
                var favFolder = new FavoritesFolder();
                SkyDriveFolder itemfolder;
                if (FolderCollection.TryGetValue(item.UniqueId, out itemfolder))
                {
                    CopySkyDriveFolderToJson(itemfolder, favFolder);
                    folder.Subalbums.Add(favFolder);
                    PopulateJsonFromRootFavoritesFolder(itemfolder, favFolder);
                }
            }
        }

        #region SkyDriveItem to internal FavoritesItem object conversion functions

        private SkyDriveFolder CreateSkyDriveFolderFromJson(FavoritesFolder folder)
        {
            var skyFolder = new SkyDriveFolder(folder.UniqueId, folder.Title, folder.Description, folder.Size, folder.Parent_id, SkyDriveItemType.Folder, folder.Updated_time, folder.Count);
            skyFolder.ImagePath = folder.ImagePath;
            skyFolder.SyncState = folder.SyncState;
            return skyFolder;
        }
        private SkyDriveFile CreateSkyDriveFileFromJson(FavoritesFile item)
        {
            var skyFile = new SkyDriveFile(item.UniqueId, item.Title, item.Description, item.Size, item.Parent_id, SkyDriveItemType.File, item.Updated_time, item.Source);
            skyFile.ImageDominantColor = Utility.ColorFromString(item.ImageDominantColor);
            skyFile.TileBackground = new SolidColorBrush(skyFile.ImageDominantColor);
            skyFile.ImagePath = item.ImagePath;
            skyFile.URL = item.URL;
            skyFile.FileExtenstion = item.FileExtenstion;
            skyFile.IsInternetShortcut = item.IsInternetShortcut;
            skyFile.SyncState = item.SyncState;
            return skyFile;
        }
        private void CopySkyDriveFolderToJson(SkyDriveFolder source, FavoritesFolder destination)
        {
            destination.Title = source.Title;
            destination.UniqueId = source.UniqueId;
            destination.Updated_time = source.Updated_time;
            destination.Size = source.Size;
            destination.Count = source.Count;
            destination.ImagePath = source.ImagePath;
            destination.Parent_id = source.Parent_id;
            destination.Description = source.Description;
            destination.ImageDominantColor = source.ImageDominantColor.ToString();
            destination.SyncState = source.SyncState;
        }
        private void CopySkyDriveFileToJson(SkyDriveFile source, FavoritesFile destination)
        {
            destination.Title = source.Title;
            destination.UniqueId = source.UniqueId;
            destination.Updated_time = source.Updated_time;
            destination.Size = source.Size;
            destination.ImagePath = source.ImagePath;
            destination.Parent_id = source.Parent_id;
            destination.Description = source.Description;
            destination.ImageDominantColor = source.ImageDominantColor.ToString();
            destination.FileExtenstion = source.FileExtenstion;
            destination.IsInternetShortcut = source.IsInternetShortcut;
            destination.Source = source.Source;
            destination.URL = source.URL;
            destination.SyncState = source.SyncState;
        }

        #endregion


    }
}
