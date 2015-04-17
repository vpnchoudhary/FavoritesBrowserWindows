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
using RoamingFavorite.Data;
using RoamingFavorite.Common;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace RoamingFavorite.View
{
    public sealed partial class OptionsSettingFlyout : SettingsFlyout
    {
        public OptionsSettingFlyout()
        {
            this.InitializeComponent();
            this.alwaysIEOption.IsOn = App.OpenWebPageinIE;
        }

        
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            SkyDriveDataSource.DeleteContainer();
        }

        private void alwaysIEOption_Toggled(object sender, RoutedEventArgs e)
        {
            App.OpenWebPageinIE = this.alwaysIEOption.IsOn;
            if (App.OpenWebPageinIE)
            {
                SkyDriveDataSource.SaveSetting(constants.PreferenceOpenInIE, "true");
            }
            else
            {
                SkyDriveDataSource.SaveSetting(constants.PreferenceOpenInIE, "false");
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.DefaultSearchEngine = Convert.ToString(e.AddedItems[0]);
            SkyDriveDataSource.SaveSetting(constants.SearchEngine, App.DefaultSearchEngine);
        }
    }
}
