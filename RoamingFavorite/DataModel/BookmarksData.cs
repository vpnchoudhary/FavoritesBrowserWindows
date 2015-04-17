using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Microsoft.Live;
using Windows.Data.Json;
using Windows.Storage;

namespace RoamingFavorite.Data
{
    class BookmarksData
    {
        private BookmarksData() { }

        private static BookmarksData bookmarksData = null;

        public static JsonArray jsonBookmarksData = null;

        private bool isBookmarksDataAvailable = false;

        public bool IsBookmarksDataAvailable
        {
            get { return isBookmarksDataAvailable; }
        }

        /// <summary>
        /// GetBookmarksDataInstance
        /// </summary>
        /// <returns></returns>
        public static BookmarksData GetBookmarksDataInstance()
        {
            if (null == bookmarksData)
            {
                bookmarksData = new BookmarksData();
                jsonBookmarksData = new JsonArray();
            }

            return bookmarksData;
        }

        /// <summary>
        /// PopulateBookmarksDataAsync
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> PopulateBookmarksDataAsync(string path, bool bUseCache = false)
        {
            try
            {
             await PopulateBookmarksData(path);
             isBookmarksDataAvailable = true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                isBookmarksDataAvailable = false;
            }

            return true;
        }

        /// <summary>
        /// PopulateBookmarksData
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<bool> PopulateBookmarksData(string path)
        {
            try
            {
                LiveConnectClient liveConnectClient = new LiveConnectClient(App.Session);
                LiveOperationResult liveOperationResult = await liveConnectClient.GetAsync(path);

                JsonObject jsonObject = JsonObject.Parse(liveOperationResult.RawResult);
                JsonArray jsonBookmarksArray = jsonObject.GetNamedArray("data");

                if (jsonBookmarksArray.Count > 0)
                {
                    foreach (JsonValue value in jsonBookmarksArray)
                    {
                        if (value.GetObject().GetNamedString("type") == "folder")
                        {
                            string folderPath = value.GetObject().GetNamedString("id") + "/files";
                            await PopulateBookmarksData(folderPath);
                        }
                        jsonBookmarksData.Add(value);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
            }

            return true;
        }
    }
}
