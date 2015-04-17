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
using Windows.ApplicationModel;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace RoamingFavorite.View
{
    public sealed partial class AboutSettingsFlyout : SettingsFlyout
    {
        public AboutSettingsFlyout()
        {
            this.InitializeComponent();
            this.Publisher.Text = Package.Current.PublisherDisplayName;
            this.PackageVersion.Text = Convert.ToString(Package.Current.Id.Version.Major) + "." + Convert.ToString(Package.Current.Id.Version.Minor) + "." + Convert.ToString(Package.Current.Id.Version.Build) + "." + Convert.ToString(Package.Current.Id.Version.Revision);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://bit.ly/1duifRj"));
        }
    }
}
