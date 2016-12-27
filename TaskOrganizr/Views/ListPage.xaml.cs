using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListPage : Page
    {
        public static ObservableCollection<List> lists = new ObservableCollection<List>();
        public static ObservableCollection<Category> filledCategories = new ObservableCollection<Category>();
        public ListPage()
        {
            this.InitializeComponent();
        }
        //Reset Values and Restore Data
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            listsList.ItemsSource = lists;
            catComboBox.ItemsSource = filledCategories;
            BusyModal.IsModal = true;
            await API.getLists();
            await API.getCategories();
            getCurrCategories();

            var param = (int)e.Parameter;
            catComboBox.SelectedIndex = param;
            getListsPerCategory(filledCategories.ElementAt(param).Id);

            if (API.categories.Count == 0) Frame.Navigate(typeof(NoCategories));
            else if (lists.Count == 0) Frame.Navigate(typeof(NoLists), catComboBox.SelectedItem);

            BusyModal.IsModal = false;
        }

        /*
         * ///////////////////////////////////////
         * //// EVENTS THAT AFFECT CATEGORIES ////
         * ///////////////////////////////////////
         * */

        //Adds to lists the lists that are in the category
        //Use -1 to add every list
        public static void getListsPerCategory(string catid)
        {
            lists.Clear();
            for (int i = 0; i < API.totalLists.Count; i++)
            {
                if (catid.Equals("-1") || API.totalLists.ElementAt(i).CategoryId.Equals(catid))
                {
                    lists.Add(API.totalLists.ElementAt(i));
                }
            }
        }

        public static void getCurrCategories()
        {
            filledCategories.Clear();
            foreach (var categ in API.categories) filledCategories.Add(categ);
            Category temp = new Category()
            {
                Id = "-1",
                CatName = "All Categories",
                UserId = API.currentUser.Id
            };
            Category addcat = new Category()
            {
                Id = "-2",
                CatName = "Add Category...",
                UserId = API.currentUser.Id
            };

            filledCategories.Add(temp);
            filledCategories.Add(addcat);
        }

        //Get lists when change category
        private async void catComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (catComboBox.SelectedIndex >= 0)
            {
                if (filledCategories.ElementAt(catComboBox.SelectedIndex).Id.Equals("-2"))
                {
                    await showAddCategoryContentDial();
                    await API.getCategories();
                    getCurrCategories();
                    catComboBox.SelectedItem = filledCategories.ElementAt(filledCategories.Count - 3);
                }
                else
                {
                    getListsPerCategory(filledCategories.ElementAt(catComboBox.SelectedIndex).Id);
                    if (lists.Count == 0) Frame.Navigate(typeof(NoLists), catComboBox.SelectedItem);
                }
            }
        }

        /*
         * //////////////////////////////////
         * //// EVENTS THAT AFFECT LISTS ////
         * //////////////////////////////////
         * */

        //Get TaskItems on list change
        private void listsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listsList.SelectedIndex >= 0)
            {
                if (myDataGrid.ActualWidth >= 1032) tasksFrame.Navigate(typeof(TasksPage), listsList.SelectedIndex);
                else Frame.Navigate(typeof(TasksPage), listsList.SelectedIndex);
            }
        }

        //Add list from content dialog(event called by appbarbuttonn:AddList)
        private async void AddList_Click(object sender, RoutedEventArgs e)
        {
            if (catComboBox.SelectedIndex < filledCategories.Count - 2)
            {
                Category category = catComboBox.SelectedItem as Category;
                await showAddListContentDial(category);
                getListsPerCategory(category.Id);
            }
            else
            {
                notSelectedCategory.ShowAt(addListAppBarButton);
            }
        }

        //Hide Flyout when clicking "OK"
        private void innerAddListButtonFlyout_Click(object sender, RoutedEventArgs e)
        {
            notSelectedCategory.Hide();
        }

        //Edit List
        private void listsItemTemplate_LeftCommandRequested(object sender, EventArgs e)
        {
            var selecteditem = (sender as SlidableListItem).DataContext as List;
            showEditListContentDial(selecteditem);
        }

        //Delete Completed TaskItem
        private async void listsItemTemplate_RightCommandRequested(object sender, EventArgs e)
        {
            var deletedList = (sender as SlidableListItem).DataContext as List;

            ContentDialog dialog = showCategoryWarningMessageDialog(deletedList);

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                BusyModal.IsModal = true;
                await API.removeList(deletedList);
                getListsPerCategory(deletedList.CategoryId);
                BusyModal.IsModal = false;
                if (lists.Count == 0) Frame.Navigate(typeof(NoLists), catComboBox.SelectedItem);
            }
        }

        /*
         * //////////////////////////////////////
         * //// EVENTS THAT AFFECT TASKITEMS ////
         * //////////////////////////////////////
         * */

        //Edit List Content Dialog
        private async void showEditListContentDial(List list)
        {
            var dialog = new ContentDialog()
            {
                Title = "Edit List: " + list.ListName,
                VerticalAlignment = VerticalAlignment.Stretch,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var panel = new StackPanel();

            var editlname = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10, 10, 10, 10),
                Text = list.ListName,
                PlaceholderText = "Task Name here.."
            };
            editlname.TextChanged += new TextChangedEventHandler(addListNameTextBox_TextChanged);

            var innerpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var image = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:///Images/warning.png")),
                Height = 18
            };

            var tasknamefound = new TextBlock()
            {
                Text = "List name already exists!",
                Margin = new Thickness(10, 0, 0, 0)
            };

            innerpanel.Children.Add(image);
            innerpanel.Children.Add(tasknamefound);

            panel.Children.Add(editlname);
            panel.Children.Add(innerpanel);

            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                List newlist = new List()
                {
                    Id = list.Id,
                    ListName = editlname.Text,
                    CategoryId = list.CategoryId,
                    UserId = list.UserId
                };
                await API.editList(newlist);
                getListsPerCategory(newlist.CategoryId);
            }
        }

        //Add List Content Dialog
        public async static Task showAddListContentDial(Category category)
        {
            var dialog = new ContentDialog()
            {
                Title = "Add List to Category " + category.CatName + ":",
                VerticalAlignment = VerticalAlignment.Stretch,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var panel = new StackPanel();

            var addlname = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10, 10, 10, 10),
                PlaceholderText = "List Name here.."
            };
            addlname.TextChanged += new TextChangedEventHandler(addListNameTextBox_TextChanged);

            var innerpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var image = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:///Images/warning.png")),
                Height = 18
            };

            var tasknamefound = new TextBlock()
            {
                Text = "List name already exists!",
                Margin = new Thickness(10, 0, 0, 0)
            };

            innerpanel.Children.Add(image);
            innerpanel.Children.Add(tasknamefound);

            panel.Children.Add(addlname);
            panel.Children.Add(innerpanel);

            dialog.Content = panel;
            dialog.IsPrimaryButtonEnabled = false;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                List newlist = new List()
                {
                    ListName = addlname.Text,
                    CategoryId = category.Id,
                    UserId = API.currentUser.Id
                };
                await API.addList(newlist);
            }
        }

        //Add Category Content Dialog
        public async static Task showAddCategoryContentDial()
        {
            var dialog = new ContentDialog()
            {
                Title = "Add Category: ",
                VerticalAlignment = VerticalAlignment.Stretch,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var panel = new StackPanel();

            var addcname = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10, 10, 10, 10),
                PlaceholderText = "Category Name here.."
            };
            addcname.TextChanged += new TextChangedEventHandler(addCatNameTextBox_TextChanged);

            var innerpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var image = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:///Images/warning.png")),
                Height = 18
            };

            var tasknamefound = new TextBlock()
            {
                Text = "Category name already exists!",
                Margin = new Thickness(10, 0, 0, 0)
            };

            innerpanel.Children.Add(image);
            innerpanel.Children.Add(tasknamefound);

            var relativepanel = new RelativePanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var redpanel = new RelativePanel();
            var redtextblock = new TextBlock()
            {
                Text = "Red Value:",
                FontSize = 18,
            };
            redtextblock.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var redslider = new Slider()
            {
                Margin = new Thickness(20, 5, 0, 0),
                Value = 0,
                Maximum = 255,
                Minimum = 0,
                Width = 200
            };
            redslider.ValueChanged += new RangeBaseValueChangedEventHandler(redSlider_ValueChanged);
            redslider.SetValue(RelativePanel.RightOfProperty, redtextblock);
            redslider.SetValue(RelativePanel.AlignVerticalCenterWithProperty, redtextblock);

            redpanel.Children.Add(redtextblock);
            redpanel.Children.Add(redslider);

            var greenpanel = new RelativePanel();
            greenpanel.SetValue(RelativePanel.BelowProperty, redpanel);
            var greentextblock = new TextBlock()
            {
                Text = "Green Value:",
                FontSize = 18,
            };
            greentextblock.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var greenslider = new Slider()
            {
                Margin = new Thickness(20, 5, 0, 0),
                Value = 0,
                Maximum = 255,
                Minimum = 0,
                Width = 200
            };
            greenslider.ValueChanged += new RangeBaseValueChangedEventHandler(greenSlider_ValueChanged);
            greenslider.SetValue(RelativePanel.RightOfProperty, greentextblock);
            greenslider.SetValue(RelativePanel.AlignVerticalCenterWithProperty, greentextblock);

            greenpanel.Children.Add(greentextblock);
            greenpanel.Children.Add(greenslider);

            var bluepanel = new RelativePanel();
            bluepanel.SetValue(RelativePanel.BelowProperty, greenpanel);
            var bluetextblock = new TextBlock()
            {
                Text = "Blue Value:",
                FontSize = 18,
            };
            bluetextblock.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var blueslider = new Slider()
            {
                Margin = new Thickness(20, 5, 0, 0),
                Value = 0,
                Maximum = 255,
                Minimum = 0,
                Width = 200
            };
            blueslider.ValueChanged += new RangeBaseValueChangedEventHandler(blueSlider_ValueChanged);
            blueslider.SetValue(RelativePanel.RightOfProperty, bluetextblock);
            blueslider.SetValue(RelativePanel.AlignVerticalCenterWithProperty, bluetextblock);

            bluepanel.Children.Add(bluetextblock);
            bluepanel.Children.Add(blueslider);

            var circlepanel = new RelativePanel();
            circlepanel.SetValue(RelativePanel.BelowProperty, bluepanel);

            var ellipse = new Ellipse()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Width = 40,
                Height = 40
            };
            Color color = Color.FromArgb(170, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
            ellipse.Fill = new SolidColorBrush(color);
            ellipse.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            ellipse.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var circletextblock = new TextBlock()
            {
                Text = "Color:",
                FontSize = 18
            };
            circletextblock.SetValue(RelativePanel.AlignVerticalCenterWithProperty, ellipse);
            circletextblock.SetValue(RelativePanel.LeftOfProperty, ellipse);

            circlepanel.Children.Add(ellipse);
            circlepanel.Children.Add(circletextblock);

            relativepanel.Children.Add(redpanel);
            relativepanel.Children.Add(greenpanel);
            relativepanel.Children.Add(bluepanel);
            relativepanel.Children.Add(circlepanel);

            panel.Children.Add(addcname);
            panel.Children.Add(innerpanel);
            panel.Children.Add(relativepanel);

            dialog.Content = panel;
            dialog.IsPrimaryButtonEnabled = false;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                color = Color.FromArgb(200, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
                string hex = color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                Category temp = new Category()
                {
                    CatName = addcname.Text,
                    Hex = hex,
                    UserId = API.currentUser.Id
                };
                await API.addCategory(temp);
                getListsPerCategory(temp.Id);
            }
        }

        public async static void showEditCategoryContentDial(Category category)
        {
            byte a = byte.Parse(category.Hex.Substring(1, 2), NumberStyles.HexNumber);
            byte r = byte.Parse(category.Hex.Substring(3, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(category.Hex.Substring(5, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(category.Hex.Substring(7, 2), NumberStyles.HexNumber);
            Color color = Color.FromArgb(a, r, g, b);

            var dialog = new ContentDialog()
            {
                Title = "Edit Category: " + category.CatName,
                VerticalAlignment = VerticalAlignment.Stretch,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var panel = new StackPanel();

            var addcname = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10, 10, 10, 10),
                PlaceholderText = "Category Name here..",
                Text = category.CatName
            };
            addcname.TextChanged += new TextChangedEventHandler(editCatNameTextBox_TextChanged);

            var innerpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var image = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:///Images/warning.png")),
                Height = 18
            };

            var tasknamefound = new TextBlock()
            {
                Text = "Category name already exists!",
                Margin = new Thickness(10, 0, 0, 0)
            };

            innerpanel.Children.Add(image);
            innerpanel.Children.Add(tasknamefound);

            var relativepanel = new RelativePanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var redpanel = new RelativePanel();
            var redtextblock = new TextBlock()
            {
                Text = "Red Value:",
                FontSize = 18,
            };
            redtextblock.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var redslider = new Slider()
            {
                Margin = new Thickness(20, 5, 0, 0),
                Value = color.R,
                Maximum = 255,
                Minimum = 0,
                Width = 200
            };
            redslider.ValueChanged += new RangeBaseValueChangedEventHandler(redSlider_ValueChanged);
            redslider.SetValue(RelativePanel.RightOfProperty, redtextblock);
            redslider.SetValue(RelativePanel.AlignVerticalCenterWithProperty, redtextblock);

            redpanel.Children.Add(redtextblock);
            redpanel.Children.Add(redslider);

            var greenpanel = new RelativePanel();
            greenpanel.SetValue(RelativePanel.BelowProperty, redpanel);
            var greentextblock = new TextBlock()
            {
                Text = "Green Value:",
                FontSize = 18,
            };
            greentextblock.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var greenslider = new Slider()
            {
                Margin = new Thickness(20, 5, 0, 0),
                Value = color.G,
                Maximum = 255,
                Minimum = 0,
                Width = 200
            };
            greenslider.ValueChanged += new RangeBaseValueChangedEventHandler(greenSlider_ValueChanged);
            greenslider.SetValue(RelativePanel.RightOfProperty, greentextblock);
            greenslider.SetValue(RelativePanel.AlignVerticalCenterWithProperty, greentextblock);

            greenpanel.Children.Add(greentextblock);
            greenpanel.Children.Add(greenslider);

            var bluepanel = new RelativePanel();
            bluepanel.SetValue(RelativePanel.BelowProperty, greenpanel);
            var bluetextblock = new TextBlock()
            {
                Text = "Blue Value:",
                FontSize = 18,
            };
            bluetextblock.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var blueslider = new Slider()
            {
                Margin = new Thickness(20, 5, 0, 0),
                Value = color.B,
                Maximum = 255,
                Minimum = 0,
                Width = 200
            };
            blueslider.ValueChanged += new RangeBaseValueChangedEventHandler(blueSlider_ValueChanged);
            blueslider.SetValue(RelativePanel.RightOfProperty, bluetextblock);
            blueslider.SetValue(RelativePanel.AlignVerticalCenterWithProperty, bluetextblock);

            bluepanel.Children.Add(bluetextblock);
            bluepanel.Children.Add(blueslider);

            var circlepanel = new RelativePanel();
            circlepanel.SetValue(RelativePanel.BelowProperty, bluepanel);

            var ellipse = new Ellipse()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Width = 40,
                Height = 40
            };
            Color newcolor = Color.FromArgb(200, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
            ellipse.Fill = new SolidColorBrush(color);
            ellipse.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            ellipse.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            var circletextblock = new TextBlock()
            {
                Text = "Color:",
                FontSize = 18
            };
            circletextblock.SetValue(RelativePanel.AlignVerticalCenterWithProperty, ellipse);
            circletextblock.SetValue(RelativePanel.LeftOfProperty, ellipse);

            circlepanel.Children.Add(ellipse);
            circlepanel.Children.Add(circletextblock);

            relativepanel.Children.Add(redpanel);
            relativepanel.Children.Add(greenpanel);
            relativepanel.Children.Add(bluepanel);
            relativepanel.Children.Add(circlepanel);

            panel.Children.Add(addcname);
            panel.Children.Add(innerpanel);
            panel.Children.Add(relativepanel);

            dialog.Content = panel;
            dialog.IsPrimaryButtonEnabled = true;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                newcolor = Color.FromArgb(200, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
                string hex = newcolor.A.ToString("X2") + newcolor.R.ToString("X2") + newcolor.G.ToString("X2") + newcolor.B.ToString("X2");
                Category temp = new Category()
                {
                    Id = category.Id,
                    CatName = addcname.Text,
                    Hex = hex,
                    UserId = category.UserId
                };
                await API.editCategory(temp);
            }
        }

        private ContentDialog showCategoryWarningMessageDialog(List list)
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
                Text = "Do you want to remove list " + list.ListName + "\nand all of its contents ? "
            };

            panel.Children.Add(text);

            dialog.Content = panel;

            return dialog;
        }

        /*
         * ////////////////////// 
         * //// Fill Ellipse ////
         * //////////////////////
         * */

        //Called by redSlider
        private static void redSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var redslider = sender as Slider;
            var redpanel = redslider.Parent as RelativePanel;

            var panel = redpanel.Parent as RelativePanel;

            DependencyObject panelchild_green = VisualTreeHelper.GetChild(panel, 1);
            var greenpanel = panelchild_green as RelativePanel;
            DependencyObject greenpanelchild = VisualTreeHelper.GetChild(greenpanel, 1);
            var greenslider = greenpanelchild as Slider;

            DependencyObject panelchild_blue = VisualTreeHelper.GetChild(panel, 2);
            var bluepanel = panelchild_blue as RelativePanel;
            DependencyObject bluepanelchild = VisualTreeHelper.GetChild(bluepanel, 1);
            var blueslider = bluepanelchild as Slider;

            DependencyObject panelchild_circle = VisualTreeHelper.GetChild(panel, 3);
            var circlepanel = panelchild_circle as RelativePanel;
            DependencyObject circlepanelchild = VisualTreeHelper.GetChild(circlepanel, 0);
            var circle = circlepanelchild as Ellipse;

            Color color = Color.FromArgb(200, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
            circle.Fill = new SolidColorBrush(color);
        }

        //Called by greenSlider
        private static void greenSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var greenslider = sender as Slider;
            var greenpanel = greenslider.Parent as RelativePanel;

            var panel = greenpanel.Parent as RelativePanel;

            DependencyObject panelchild_red = VisualTreeHelper.GetChild(panel, 0);
            var redpanel = panelchild_red as RelativePanel;
            DependencyObject redpanelchild = VisualTreeHelper.GetChild(redpanel, 1);
            var redslider = redpanelchild as Slider;

            DependencyObject panelchild_blue = VisualTreeHelper.GetChild(panel, 2);
            var bluepanel = panelchild_blue as RelativePanel;
            DependencyObject bluepanelchild = VisualTreeHelper.GetChild(bluepanel, 1);
            var blueslider = bluepanelchild as Slider;

            DependencyObject panelchild_circle = VisualTreeHelper.GetChild(panel, 3);
            var circlepanel = panelchild_circle as RelativePanel;
            DependencyObject circlepanelchild = VisualTreeHelper.GetChild(circlepanel, 0);
            var circle = circlepanelchild as Ellipse;

            Color color = Color.FromArgb(200, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
            circle.Fill = new SolidColorBrush(color);
        }

        //Called by blueSlider
        private static void blueSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var blueslider = sender as Slider;
            var bluepanel = blueslider.Parent as RelativePanel;

            var panel = bluepanel.Parent as RelativePanel;

            DependencyObject panelchild_red = VisualTreeHelper.GetChild(panel, 0);
            var redpanel = panelchild_red as RelativePanel;
            DependencyObject redpanelchild = VisualTreeHelper.GetChild(redpanel, 1);
            var redslider = redpanelchild as Slider;

            DependencyObject panelchild_green = VisualTreeHelper.GetChild(panel, 1);
            var greenpanel = panelchild_green as RelativePanel;
            DependencyObject greenpanelchild = VisualTreeHelper.GetChild(greenpanel, 1);
            var greenslider = greenpanelchild as Slider;

            DependencyObject panelchild_circle = VisualTreeHelper.GetChild(panel, 3);
            var circlepanel = panelchild_circle as RelativePanel;
            DependencyObject circlepanelchild = VisualTreeHelper.GetChild(circlepanel, 0);
            var circle = circlepanelchild as Ellipse;

            Color color = Color.FromArgb(200, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value);
            circle.Fill = new SolidColorBrush(color);
        }

        /*
         * ///////////////////////// 
         * //// TextBox Changed ////
         * /////////////////////////
         * */

        //Check same listitem name
        private static void addListNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var panel = textbox.Parent as StackPanel;
            var contentdialog = panel.Parent as ContentDialog;
            DependencyObject child = VisualTreeHelper.GetChild(panel, 1);
            var innerpanel = child as StackPanel;
            innerpanel.Visibility = searchCurrentLists(textbox.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (textbox.Text == "" || innerpanel.Visibility == Visibility.Visible) contentdialog.IsPrimaryButtonEnabled = false;
            else contentdialog.IsPrimaryButtonEnabled = true;
        }

        //Check same listitem name
        private static void addCatNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var panel = textbox.Parent as StackPanel;
            var contentdialog = panel.Parent as ContentDialog;
            DependencyObject child = VisualTreeHelper.GetChild(panel, 1);
            var innerpanel = child as StackPanel;
            innerpanel.Visibility = searchCurrentCategories(textbox.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (textbox.Text == "" || innerpanel.Visibility == Visibility.Visible) contentdialog.IsPrimaryButtonEnabled = false;
            else contentdialog.IsPrimaryButtonEnabled = true;
        }

        private static void editCatNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var panel = textbox.Parent as StackPanel;
            var contentdialog = panel.Parent as ContentDialog;
            DependencyObject child = VisualTreeHelper.GetChild(panel, 1);
            var innerpanel = child as StackPanel;

            DependencyObject relpanelchild = VisualTreeHelper.GetChild(panel, 2);
            var relativepanel = relpanelchild as RelativePanel;
            DependencyObject redpanelchild = VisualTreeHelper.GetChild(relativepanel, 0);
            var redpanel = redpanelchild as RelativePanel;
            DependencyObject redsliderchild = VisualTreeHelper.GetChild(redpanel, 1);
            var redslider = redsliderchild as Slider;

            DependencyObject greenpanelchild = VisualTreeHelper.GetChild(relativepanel, 1);
            var greenpanel = greenpanelchild as RelativePanel;
            DependencyObject greensliderchild = VisualTreeHelper.GetChild(greenpanel, 1);
            var greenslider = greensliderchild as Slider;

            DependencyObject bluepanelchild = VisualTreeHelper.GetChild(relativepanel, 1);
            var bluepanel = bluepanelchild as RelativePanel;
            DependencyObject bluesliderchild = VisualTreeHelper.GetChild(bluepanel, 1);
            var blueslider = bluesliderchild as Slider;

            innerpanel.Visibility = searchCurrentCategoriesExcept(textbox.Text, (byte)redslider.Value, (byte)greenslider.Value, (byte)blueslider.Value) ? Visibility.Visible : Visibility.Collapsed;
            if (textbox.Text == "" || innerpanel.Visibility == Visibility.Visible) contentdialog.IsPrimaryButtonEnabled = false;
            else contentdialog.IsPrimaryButtonEnabled = true;
        }

        /*
         * //////////////////////////
         * ////  SEARCH ENGINES  ////
         * //////////////////////////
         * */

        public static bool searchCurrentCategories(string name)
        {
            bool found = false;
            for (int i = 0; i < filledCategories.Count() - 2; i++)
            {
                if (filledCategories.ElementAt(i).CatName.Equals(name)) { found = true; }
            }
            return found;
        }

        public static bool searchCurrentLists(string name)
        {
            bool found = false;
            for (int i = 0; i < lists.Count(); i++)
            {
                if (lists.ElementAt(i).ListName.Equals(name)) { found = true; }
            }
            return found;
        }

        public static bool searchCurrentCategoriesExcept(string name, byte r, byte g, byte b)
        {
            bool found = false;
            for (int i = 0; i < filledCategories.Count() - 2; i++)
            {
                Color color = Color.FromArgb(200, r, g, b);
                string hex = color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                if (filledCategories.ElementAt(i).CatName.Equals(name)
                    && filledCategories.ElementAt(i).Hex.Equals("#" + hex)) { found = true; }
            }
            return found;
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public StringToVisibilityConverter() { }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
    }

    public class StringToVisibilityConverterReverse : IValueConverter
    {
        public StringToVisibilityConverterReverse() { }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }

    public class IntToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int priority = (int)value;
            SolidColorBrush brush = null;
            if (priority == 0) brush = new SolidColorBrush(Color.FromArgb(255, 255, 69, 0));
            else if(priority == 1) brush = new SolidColorBrush(Color.FromArgb(255, 255, 165, 0));
            else if(priority == 2) brush = new SolidColorBrush(Color.FromArgb(255, 154, 205, 50));
            else if(priority == 3) brush = new SolidColorBrush(Color.FromArgb(255, 0, 139, 139));
            else if(priority == 4) brush = new SolidColorBrush(Color.FromArgb(255, 186, 85, 211));
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string priority = (string)value;
            return priority;
        }
    }

    public class IntToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int priority = (int)value;
            SolidColorBrush brush = null;  
            if (priority == 0) brush = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            else if (priority == 1) brush = new SolidColorBrush(Color.FromArgb(255, 56, 89, 102));
            else
            {
                DateTime currTime = DateTime.Now;
                DateTime timeChange = new DateTime(2015, 1, 1, 07, 0, 0);
                if (currTime.TimeOfDay.CompareTo(timeChange.TimeOfDay) > 0) brush = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
                else brush = new SolidColorBrush(Color.FromArgb(255, 56, 89, 102));
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string priority = (string)value;
            return priority;
        }
    }

    public class IntToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int priority = (int)value;
            SolidColorBrush brush = null;
            if (priority == 0) brush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            else if (priority == 1) brush = new SolidColorBrush(Color.FromArgb(255, 217, 233, 238));
            else
            {
                DateTime currTime = DateTime.Now;
                DateTime timeChange = new DateTime(2015, 1, 1, 07, 0, 0);
                if (currTime.TimeOfDay.CompareTo(timeChange.TimeOfDay) > 0) brush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                else brush = new SolidColorBrush(Color.FromArgb(255, 217, 233, 238));
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string priority = (string)value;
            return priority;
        }
    }

    public class IntToItemBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int priority = (int)value;
            SolidColorBrush brush = null;
            if (priority == 0) brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            else if (priority == 1) brush = new SolidColorBrush(Color.FromArgb(255, 79, 112, 125));
            else
            {
                DateTime currTime = DateTime.Now;
                DateTime timeChange = new DateTime(2015, 1, 1, 07, 0, 0);
                if (currTime.TimeOfDay.CompareTo(timeChange.TimeOfDay) > 0) brush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                else brush = new SolidColorBrush(Color.FromArgb(255, 79, 112, 125));
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string priority = (string)value;
            return priority;
        }
    }
}
