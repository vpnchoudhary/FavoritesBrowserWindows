using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using Windows.UI;
using RoamingFavorite.Common;

namespace RoamingFavorite.Common
{
    class Utility
    {
        public static int CompareDates(string date1, string date2)
        {
            DateTime dateOne = Convert.ToDateTime(date1);
            DateTime dateTwo = Convert.ToDateTime(date2);
            if(dateOne>dateTwo)
            { return 1; }
            else if(dateOne == dateTwo)
            { return 0; }
            else
            { return -1; }
        }
        public static bool IsConnectedToInternet()
        {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return (connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }

        public static Uri IsInternetURL(string searchQuery, bool notASeachQuery = false)
        {
            if (!notASeachQuery)
            {
                if (!searchQuery.StartsWith(@"https://", StringComparison.CurrentCultureIgnoreCase) && !searchQuery.StartsWith(@"http://", StringComparison.CurrentCultureIgnoreCase))
                {
                    searchQuery = @"http://" + searchQuery;
                }
            }
            Uri uriResult;
            bool result = Uri.TryCreate(searchQuery, UriKind.Absolute, out uriResult);
            if (result)
            {
                return uriResult;
            }
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color ColorFromString(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// GetImageColor
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public async static Task<Color> GetImageColor(string imagePath)
        {
            Color imageDominantColor = Color.FromArgb(255, 40, 122, 237); // IE fav color.

            try
            {
                Uri uri = new Uri(imagePath);

                HttpClient client = new HttpClient();
                using (HttpResponseMessage response = await client.GetAsync(uri))
                {
                    byte[] imgByte = await response.Content.ReadAsByteArrayAsync();
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                        {
                            writer.WriteBytes(imgByte);
                            await writer.StoreAsync();

                            var bmpDecoder = await BitmapDecoder.CreateAsync(stream);
                            var transform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1, InterpolationMode = BitmapInterpolationMode.Fant };

                            var pixels = await bmpDecoder.GetPixelDataAsync(
                                BitmapPixelFormat.Rgba8,
                                BitmapAlphaMode.Straight,
                                transform,
                                ExifOrientationMode.RespectExifOrientation,
                                ColorManagementMode.ColorManageToSRgb);

                            var bytes = pixels.DetachPixelData();

                            imageDominantColor = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);

                            return imageDominantColor;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return imageDominantColor;
            }
        }


        //http://www.pinvoke.net/default.aspx/coredll/getsysteminfo.html
        internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;
        internal const ushort PROCESSOR_ARCHITECTURE_ARM = 5;

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        [DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        public static Platform GetPlatform()
        {
            Platform processorArch = Platform.Unknown;
            try
            {
                SYSTEM_INFO sysInfo = new SYSTEM_INFO();
                GetNativeSystemInfo(ref sysInfo);
                switch (sysInfo.wProcessorArchitecture)
                {
                    case PROCESSOR_ARCHITECTURE_INTEL: processorArch = Platform.X86;
                        break;
                    case PROCESSOR_ARCHITECTURE_IA64: processorArch = Platform.IA64;
                        break;
                    case PROCESSOR_ARCHITECTURE_AMD64: processorArch = Platform.X64;
                        break;
                    case PROCESSOR_ARCHITECTURE_ARM: processorArch = Platform.ARM;
                        break;
                    case PROCESSOR_ARCHITECTURE_UNKNOWN: processorArch = Platform.Unknown;
                        break;
                }
            }
            catch { };
            return processorArch;
        }
    }

    public static class ExtentionMethods
    {
        public static bool myContains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
