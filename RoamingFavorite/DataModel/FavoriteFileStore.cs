using RoamingFavorite.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;


namespace RoamingFavorite.Data
{

    //optimized synchronization object to use with using keyword.
    //todo, should be moved to utility class
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                        m_releaser :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            m_releaser.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }

    //This class caches the file object. the IsFileExists() should be used before geting details of a shortcut file (.url):
    // this IsFileExists(ref file) returns true if file exists in cache and returns file through ref file argument. This function also update its cache if ref file object contains url. 

    public class FavoriteFileStore
    {

        private XmlDocument document;
        private StorageFile storageFile;
        private static FavoriteFileStore instance = null;
        private FavoriteFileStore() { }
        public static async Task<FavoriteFileStore> GetInstance()
        {
            var mutex = new AsyncLock();
            using (await mutex.LockAsync())
            {
                if (instance == null)
                {
                    instance = new FavoriteFileStore();
                    await instance.loadXML();
                }
            }
            return instance;
        }

        private async Task loadXML()
        {
            storageFile = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync("FavoriteFileStore.xml", CreationCollisionOption.OpenIfExists);
            Windows.Data.Xml.Dom.XmlLoadSettings loadSettings = new Windows.Data.Xml.Dom.XmlLoadSettings();
            loadSettings.ProhibitDtd = false; // sample
            loadSettings.ResolveExternals = false; // sample
            document = new XmlDocument();
            string content = await FileIO.ReadTextAsync(storageFile);
            if (!string.IsNullOrEmpty(content))
            {
                document.LoadXml(content);
            }
            else
            {
                document.AppendChild(document.CreateElement("Favorites"));
            }
        }

        public void AddFile()
        {

        }

        public void DeleteFile()
        {

        }

        public void ModifyFile()
        {

        }
        // update skydrive object if there is an entry in the xml otherwise add the file entry in xml
        public bool IsFileExists(ref SkyDriveFile file)
        {
            bool bFileExists = false;
            IXmlNode node = null;
            try
            {
                node = document.SelectSingleNode(@"Favorites/File[@id= """ + file.UniqueId + @"""]");
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            if (node != null)
            {
                //
                if (!string.IsNullOrEmpty(file.URL))
                {
                    node.Attributes[3].NodeValue = file.Source;
                    node.Attributes[4].NodeValue = file.URL;
                    node.Attributes[2].NodeValue = file.Updated_time;
                    node.Attributes[7].NodeValue = file.ImagePath;
                    node.Attributes[8].NodeValue = file.ImageDominantColor.ToString();
                    bFileExists = false;
                }
                else
                {
                    // file.Source = node.Attributes[3].NodeValue.ToString();
                    file.URL = node.Attributes[4].NodeValue.ToString();
                    file.ImagePath = node.Attributes[7].NodeValue.ToString();
                    file.ImageDominantColor = Utility.ColorFromString(node.Attributes[8].NodeValue.ToString());
                    bFileExists = true;
                    // file.Updated_time = node.Attributes[2].NodeValue.ToString();
                }
            }
            else
            {
                try
                {
                    XmlElement childnode = document.CreateElement("File");
                    XmlAttribute nameAttribute = document.CreateAttribute("name");
                    nameAttribute.Value = file.Title;
                    XmlAttribute idAttribute = document.CreateAttribute("id");
                    idAttribute.Value = file.UniqueId;
                    XmlAttribute timeAttribute = document.CreateAttribute("Updated_time");
                    timeAttribute.Value = file.Updated_time;
                    XmlAttribute sourceAttribute = document.CreateAttribute("source");
                    sourceAttribute.Value = file.Source;
                    XmlAttribute urlAttribute = document.CreateAttribute("url");
                    urlAttribute.Value = file.URL;
                    XmlAttribute descAttribute = document.CreateAttribute("description");
                    descAttribute.Value = file.Description;
                    XmlAttribute sizeAttribute = document.CreateAttribute("size");
                    sizeAttribute.Value = Convert.ToString(file.Size);
                    XmlAttribute iconidAttribute = document.CreateAttribute("iconid");
                    iconidAttribute.Value = file.ImagePath;
                    XmlAttribute iconTileAttribute = document.CreateAttribute("iconTileColor");
                    iconTileAttribute.Value = file.ImageDominantColor.ToString();
                    childnode.SetAttributeNode(nameAttribute);
                    childnode.SetAttributeNode(idAttribute);
                    childnode.SetAttributeNode(timeAttribute);
                    childnode.SetAttributeNode(sourceAttribute);
                    childnode.SetAttributeNode(urlAttribute);
                    childnode.SetAttributeNode(descAttribute);
                    childnode.SetAttributeNode(sizeAttribute);
                    childnode.SetAttributeNode(iconidAttribute);
                    childnode.SetAttributeNode(iconTileAttribute);
                    document.SelectSingleNode("Favorites").AppendChild(childnode);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
            return bFileExists;
        }

        //save the entire xml dom to file
        public async Task SaveXML()
        {
            await document.SaveToFileAsync(storageFile);
        }
    }
}
