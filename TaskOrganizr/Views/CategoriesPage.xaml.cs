using Microsoft.Toolkit.Uwp.UI.Controls;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CategoriesPage : Page
    {
        public CategoriesPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            BusyModal.IsModal = true;
            categoriesList.ItemsSource = API.categories;
            await API.getCategories();
            BusyModal.IsModal = false;
            if (API.categories.Count == 0) Frame.Navigate(typeof(NoCategories));
        }

        private async void categoriesItemTemplate_LeftCommandRequested(object sender, EventArgs e)
        {
            var selecteditem = (sender as SlidableListItem).DataContext as Category;
            ListPage.showEditCategoryContentDial(selecteditem);
            await API.getCategories();
        }

        private async void categoriesItemTemplate_RightCommandRequested(object sender, EventArgs e)
        {
            var selecteditem = (sender as SlidableListItem).DataContext as Category;

            var result = await showCategoryWarningMessageDialog(selecteditem).ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                BusyModal.IsModal = true;
                await API.removeCategory(selecteditem);
                await API.getCategories();
                BusyModal.IsModal = false;
            }
        }

        private async void addCategoryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await ListPage.showAddCategoryContentDial();
        }

        private ContentDialog showCategoryWarningMessageDialog(Category category)
        {
            var dialog = new ContentDialog()
            {
                Title = "Are you sure?",
                VerticalAlignment = VerticalAlignment.Stretch,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var text = new TextBlock()
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                Text = "Do you want to remove category " + category.CatName + "\nand all of its contents ? "
            };

            panel.Children.Add(text);

            dialog.Content = panel;

            return dialog;
        }
    }
}
