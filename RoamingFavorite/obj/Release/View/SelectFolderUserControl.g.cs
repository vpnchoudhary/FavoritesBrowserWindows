﻿

#pragma checksum "C:\POC\roamsvc-code\RoamingFavorite\View\SelectFolderUserControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8619F9A2AF483055EA555C9D66BBE0B4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RoamingFavorite.View
{
    partial class SelectFolderUserControl : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 75 "..\..\View\SelectFolderUserControl.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.SelectButton_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 50 "..\..\View\SelectFolderUserControl.xaml"
                ((global::Windows.UI.Xaml.Controls.ListViewBase)(target)).ItemClick += this.ListItemSelected;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 26 "..\..\View\SelectFolderUserControl.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.FolderUp;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


