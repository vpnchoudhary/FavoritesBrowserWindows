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
using System.Text;
using System.Collections.ObjectModel;
using RoamingFavorite.Data;
using Windows.Security.Credentials;
using Windows.ApplicationModel.DataTransfer;
    
// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace RoamingFavorite.View
{
    //todo use below state and clean up UI udpate from multiple points
    [Flags]
    enum BookmarkState
    {
        CredentialPresent = 0x01,
        PinCreated        = 0x02,
        NotePresent      =  0x04
    }
    
    enum LastClickState
    {
        BtnEyeClicked,
        DeleteClicked,
        AddClicked,
        EditClicked
    }
    public sealed partial class CredentialSettingsFlyout : SettingsFlyout
    {
        private SkyDriveFile item = null;
        bool bcredentialPresent = false;
        bool showAddNotes = false;
        bool showNotes = false;
        public bool ShowAddNotes
        {
            get { return showAddNotes; }
        }
        public bool ShowNotes
        {
            get { return this.showNotes; }
        }
        private string browserurl;
        LastClickState btnClickedState;
        public bool bCredentialPresent
        {
            get { return bcredentialPresent; }
        }
        public string UserId
        {
            get { return this.UserName.Text; }
        }
        public string Password
        {
            get { return this.password.Password; }
        }
        public CredentialSettingsFlyout(SkyDriveFile skydriveFile, string url)
        {
            this.InitializeComponent();
            var credentialMgr = CredentialManager.GetInstance();
            credentialMgr.UserPinEvent += credentialMgr_UserPinEvent;
            browserurl = url;
            item = skydriveFile;
            
            // determine if browserurl is same as file url
            // user might come here with a favorite url but browsed away from favorite url
            // in that case don't show add notes option
            if (!string.IsNullOrEmpty(item.UniqueId))
            {
                if (new Uri(item.URL).Host == new Uri(browserurl).Host)
                {
                    this.NoteBox.Text = this.item.Description;
                    if (!this.item.Description.Equals(string.Empty))
                    {
                        this.showAddNotes = false;
                        this.showNotes = true;
                    }
                    else
                        this.showAddNotes = true;
                }
            }
            SetVisibility();
            var creds = credentialMgr.GetCredential(browserurl);
            if (creds != null)
            {
                this.UserName.IsReadOnly = true;
                this.UserName.Text = creds.UserName;
                this.password.Password = creds.Password;
                this.password.IsEnabled = false;
                this.txtPassword.IsReadOnly = true;
                this.BtnAdd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.BtnEdit.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bcredentialPresent = true;
                SetVisibility();
            }
            else if (!credentialMgr.IsPinSetForCredentials)
            {
                this.CreatePin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.CredentialPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
        private void credentialMgr_UserPinEvent(object sender, PinEventArguments e)
        {
            if(e.State == Common.UserPinState.PinNotSet)
            {
                //open pin setup popup
                this.CreatePin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.CredentialPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else if (e.State == Common.UserPinState.PinSet)
            {
                this.VerifyPinPopUP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            
        }
        private void SetVisibility()
        {
            this.PageURL.Text = browserurl;
            if (bcredentialPresent)
            {
                this.txtHeader.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.ExistingCredPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.AddPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.txtHeader.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.ExistingCredPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.AddPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            if (showNotes)
            {
                this.txtHeaderNotes.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.NotePanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.AddNotePanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else if(showAddNotes)
            {
                this.txtHeaderNotes.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.NotePanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.AddNotePanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.txtHeaderNotes.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.NotePanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.AddNotePanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
        private void BtnAddVewCredential_Click(object sender, RoutedEventArgs e)
        {
            btnClickedState = LastClickState.AddClicked;
            var credentialMgr = CredentialManager.GetInstance();
            if(credentialMgr.AddCredential(browserurl, UserName.Text.Trim(), password.Password))
            { 
                this.UserName.IsReadOnly = true;
                this.password.IsEnabled = false;
                this.txtPassword.IsReadOnly = true;
                bcredentialPresent = true;
                SetVisibility();
                this.BtnAdd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.BtnEdit.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        private void BtnEditVewCredential_Click(object sender, RoutedEventArgs e)
        {
            btnClickedState = LastClickState.EditClicked;
            this.UserName.IsReadOnly = false;
            this.password.IsEnabled = true;
            this.txtPassword.IsReadOnly = false;
            this.BtnAdd.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.BtnEdit.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        private void BtnEye_Click(object sender, RoutedEventArgs e)
        {
            btnClickedState = LastClickState.BtnEyeClicked;
            var credentialMgr = CredentialManager.GetInstance();
            if (credentialMgr.IsChanllengeMet)
            {
                if (this.password.Visibility == Windows.UI.Xaml.Visibility.Visible)
                {
                    this.txtPassword.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.password.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.txtPassword.Text = this.password.Password;
                }
                else
                {
                    this.txtPassword.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    this.password.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    this.password.Password = txtPassword.Text;
                }
            }
            else
            {
                // show verify pin popup
                this.VerifyPinPopUP.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        private void copy_Click(object sender, RoutedEventArgs e)
        {
            Button copy = (Button)sender;
            string stringToClipBoard = string.Empty;
            switch (copy.Name)
            {
                case "copyPassword": stringToClipBoard = this.password.Password; //copy password to clipboard
                    break;
                case "copyUserName": stringToClipBoard = this.UserName.Text;
                    break;
            }
            var dataPackage = new DataPackage();
            dataPackage.SetText(stringToClipBoard);
            Clipboard.SetContent(dataPackage);
            App.bClipBoardUsed = true;
        }
        private void AddCredential_Click(object sender, RoutedEventArgs e)
        {
            var credentialMgr = CredentialManager.GetInstance();
            if (credentialMgr.IsPinSetForCredentials)
            {
                this.ExistingCredPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.CreatePin.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            btnClickedState = LastClickState.DeleteClicked;
            var credentialMgr = CredentialManager.GetInstance();
                if (credentialMgr.RemoveCredential(browserurl))
                {
                    bcredentialPresent = false;
                    this.password.Password = string.Empty;
                    this.UserName.Text = string.Empty;
                    SetVisibility();
                }
        }
        private void AddNotes_Click(object sender, RoutedEventArgs e)
        {
            this.showAddNotes = false;
            this.showNotes = true;
            this.SetVisibility();
        }
        private async void BtnAddNotes_Click(object sender, RoutedEventArgs e)
        {
            var source = await SkyDriveDataSource.GetInstance();
            bool result = await source.ModifyDescription(item.UniqueId, NoteBox.Text.Trim());
            if(result)
            {
                item.Description = NoteBox.Text.Trim();
            }
            this.showAddNotes = false;
            this.SetVisibility();

        }
        private void CreatePin_Click(object sender, RoutedEventArgs e)
        {
            if (pin.Password == verifyPin.Password)
            {
                this.pinerrorMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                var credentialMgr = CredentialManager.GetInstance();
                credentialMgr.AddHashKey(pin.Password);
                this.CreatePin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.pinerrorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        private void VerifyPin_Click(object sender, RoutedEventArgs e)
        {
            var credentialMgr = CredentialManager.GetInstance();
            if (credentialMgr.ValidateCode(this.PinChallenge.Password))
            {
                this.verifyPinErrorMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.VerifyPinPopUP.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                switch(btnClickedState)
                {
                    case LastClickState.BtnEyeClicked: this.BtnEye_Click(sender, e);
                        break;
                    case LastClickState.DeleteClicked: this.BtnDelete_Click(sender, e);
                        break;
                }
                
            }
            else
            {
                this.verifyPinErrorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }
}
