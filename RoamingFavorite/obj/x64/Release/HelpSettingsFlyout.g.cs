﻿

#pragma checksum "C:\POC\roamsvc-code\RoamingFavorite\HelpSettingsFlyout.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4BDB40D6102C1715FF53F5A8E5C33203"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RoamingFavorite
{
    partial class HelpSettingsFlyout : global::Windows.UI.Xaml.Controls.SettingsFlyout, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 22 "..\..\..\HelpSettingsFlyout.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.FAQHyperlinkButton_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 23 "..\..\..\HelpSettingsFlyout.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.EmailHyperlinkButton_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

