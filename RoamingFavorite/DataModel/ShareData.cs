using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;

namespace RoamingFavorite.Data
{
    public class SharedLinkData 
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImagePath { get; set; }
        public int Depth { get; set; }
        public SharedLinkData(string title, string url, string imagePath, int depth)
        {
            Title = title;
            Url = url;
            ImagePath = imagePath;
            Depth = depth;
        }
    }
    
    public class SharedLinkProvider
    {
        string localImage = @"ms-appx:///Assets/FolderIcon.png";
        string htmlItem = @"<div style=""margin-left: {3}px"";><span><img src=""{0}"" alt=""Icon Image"" Height=20 Width=20></span>" +
                                @"<span style=""margin-left: 10px"";><a href=""{2}"">{1}</a></span></div>";
        string htmlFolder = @"<div div style=""margin-left: {2}px"";><span><img src=""{0}"" alt=""Folder Image"" Height=20 Width=20></span>" +
                               @"<span style=""margin-left: 10px"">{1}</span></div>";
        ObservableCollection<SharedLinkData> sharedLinkCollection = new ObservableCollection<SharedLinkData>();

        public async Task<string> GetHTML(List<SkyDriveDataCommon> selectedItems)
        {
            int depth = 1;
            foreach (SkyDriveDataCommon item in selectedItems)
            {
                if (item.Type == SkyDriveItemType.File)
                {
                    sharedLinkCollection.Add(new SharedLinkData(item.Title, ((SkyDriveFile)item).URL, item.ImagePath, depth + 1));
                    
                    //htmlContent.Append(string.Format(htmlTemplate, item.ImagePath, item.Title, ((SkyDriveFile)item).URL));
                }
                else
                {
                    sharedLinkCollection.Add(new SharedLinkData(item.Title, string.Empty, localImage, depth));
                    // item is a folder
                    await GetFiles((SkyDriveFolder)item, depth + 1);
                }
            }
            //create HTML
            return HtmlFormatHelper.CreateHtmlFormat(CreateHTML());
        }

        private async Task GetFiles(SkyDriveFolder folder, int depth)
        {
            //at this point we don't have folder contents. We need to make calls to get folder contents:
            SkyDriveDataSource source = await SkyDriveDataSource.GetInstance();
            try
            {
                folder = await source.GetGroups(folder.UniqueId, false, true);
            }catch(Exception ex)
            {
                string exp = ex.ToString();
            }
 
            //list of files in a folder
            foreach(SkyDriveFile file in folder.Items)
            {
                //add to shared link collection
                sharedLinkCollection.Add(new SharedLinkData(file.Title, file.URL, file.ImagePath, depth + 1));
            }
            foreach(SkyDriveFolder subFolder in folder.Subalbums)
            {
                sharedLinkCollection.Add(new SharedLinkData(subFolder.Title, string.Empty, localImage, depth));
                await GetFiles(subFolder, depth + 1);
            }
        }

        private string CreateHTML()
        {
            StringBuilder htmlContent = new StringBuilder();// HtmlFormatHelper.CreateHtmlFormat();
            string headerHtml = @"<div><span><h3>Following links are shared with you:</h3><span></div>";
            htmlContent.Append(headerHtml);
            foreach(SharedLinkData data in sharedLinkCollection)
            {
                if (string.IsNullOrEmpty(data.Url))
                {
                    //folder: use folder template
                    htmlContent.Append(string.Format(htmlFolder, data.ImagePath, data.Title, Convert.ToString(data.Depth * 20)));
                }
                else
                {
                    //its a file
                    htmlContent.Append(string.Format(htmlItem, data.ImagePath, data.Title, data.Url, Convert.ToString(data.Depth * 20)));
                }
            }
            string endHtml = @"<div><span><h5>Links shared using Favorites Browser App</h5><span></div>";
            htmlContent.Append(endHtml);
            return htmlContent.ToString();
        }
    }
}
