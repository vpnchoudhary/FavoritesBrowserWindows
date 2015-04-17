using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoamingFavorite.Common
{
    interface PageCallBackInterface
    {
        void SearchBoxEventsSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args);


        void SearchBoxEventsQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args);

        void SearchBoxEventsResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args);

        void SearchBoxEventsLostFocus(object sender, RoutedEventArgs e);

        void SearchBoxEventsGotFocus(object sender, RoutedEventArgs e);

        void EditCollection_Click(object sender, RoutedEventArgs e);

        void ButtonAddFavorite_Click(object sender, RoutedEventArgs e);

        void ShowInIE_Click(object sender, RoutedEventArgs e);

        void ShowCredential_Click(object sender, RoutedEventArgs e);

        bool CanGoBack{get;}

        void GoBack(object sender, RoutedEventArgs e);
        
    }
}
