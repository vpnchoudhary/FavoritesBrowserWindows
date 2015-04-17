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
using RoamingFavorite.View;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace RoamingFavorite
{
    public sealed partial class HelpSettingsFlyout : SettingsFlyout
    {
        public HelpSettingsFlyout()
        {
            this.InitializeComponent();
        }

        //private void TourHyperlinkButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var rootFrame = new Frame(); ;
        //    rootFrame.Navigate(typeof(Promo_Sync), false);
        //    Window.Current.Content = rootFrame;
        //    Window.Current.Activate();
           
        //}

        private void FAQHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(@"http://bit.ly/1eZZPcl"));
        }

        private void EmailHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var mailto = new Uri("mailto:?to=&subject=Instructions to sync IE's favorites on your desktop to SkyDrive&body=Follow the instructions at http://vipi64.wix.com/favoritesbrowser%23!instruction-to-import-ie-favorites%2Fc10la");
            Windows.System.Launcher.LaunchUriAsync(mailto);
        }
    }
}
