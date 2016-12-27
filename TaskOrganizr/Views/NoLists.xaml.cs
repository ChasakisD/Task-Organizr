using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NoLists : Page
    {
        private List<List> mylists = new List<List>();
        private ObservableCollection<Category> cats = new ObservableCollection<Category>();
        public NoLists()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            for (int i = 0; i < API.categories.Count; i++) cats.Add(API.categories.ElementAt(i));
            noListCatComboBox.ItemsSource = cats;
            var x = e.Parameter as Category;
            noListCatComboBox.SelectedItem = x;
        }

        private void noListCatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (noListCatComboBox.SelectedIndex >= 0)
            {
                var sitem = noListCatComboBox.SelectedItem as Category;
                if (sitem.Id != null)
                {
                    getListsPerCategory(sitem.Id);
                }
            }
        }

        public void getListsPerCategory(string catid)
        {
            mylists.Clear();
            for (int i = 0; i < API.totalLists.Count; i++)
            {
                if (catid.Equals("-1") || API.totalLists.ElementAt(i).CategoryId.Equals(catid))
                {
                    mylists.Add(API.totalLists.ElementAt(i));
                }
            }

            if (mylists.Count > 0) Frame.Navigate(typeof(ListPage), noListCatComboBox.SelectedIndex);
        }

        private async void addNoListButton_Click(object sender, RoutedEventArgs e)
        {
            var sitem = noListCatComboBox.SelectedItem as Category;
            if (sitem.Id != null)
            {
                await ListPage.showAddListContentDial(sitem);
                getListsPerCategory(sitem.Id);
            }
        }

    }
}
