﻿

#pragma checksum "C:\POC\roamsvc-code\RoamingFavorite\View\ContentViewerWithAppBar.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FF747F174CD44B936EEF58561DA22936"
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
    partial class ContentViewerWithAppBar : global::RoamingFavorite.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 159 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnSaveCred_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 160 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnDontSaveCred_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 138 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.PopupAddBookmark_Click;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 32 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.GoBack;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 41 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.NavigateBack;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 50 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.NavigateForward;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 104 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ButtonAddFavorite_Click;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 112 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ShowInIE_Click;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 118 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ShowCredential_Click;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 93 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.SearchBox)(target)).SuggestionsRequested += this.SearchBoxEventsSuggestionsRequested;
                 #line default
                 #line hidden
                #line 94 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.SearchBox)(target)).QuerySubmitted += this.SearchBoxEventsQuerySubmitted;
                 #line default
                 #line hidden
                #line 95 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.Controls.SearchBox)(target)).ResultSuggestionChosen += this.SearchBoxEventsResultSuggestionChosen;
                 #line default
                 #line hidden
                #line 96 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).LostFocus += this.SearchBoxEventsLostFocus;
                 #line default
                 #line hidden
                #line 97 "..\..\..\View\ContentViewerWithAppBar.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).GotFocus += this.SearchBoxEventsGotFocus;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


