﻿

#pragma checksum "C:\POC\roamsvc-code\RoamingFavorite\View\OptionsSettingFlyout.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CD58330FE7CB278007F3050D965B1450"
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
    partial class OptionsSettingFlyout : global::Windows.UI.Xaml.Controls.SettingsFlyout, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 24 "..\..\View\OptionsSettingFlyout.xaml"
                ((global::Windows.UI.Xaml.Controls.ToggleSwitch)(target)).Toggled += this.alwaysIEOption_Toggled;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 32 "..\..\View\OptionsSettingFlyout.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.ComboBox_SelectionChanged;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 28 "..\..\View\OptionsSettingFlyout.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Reset_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


