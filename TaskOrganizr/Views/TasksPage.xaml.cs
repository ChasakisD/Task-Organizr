using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TasksPage : Page
    {
        public static ObservableCollection<TaskItem> completedtasks = new ObservableCollection<TaskItem>();
        public static ObservableCollection<TaskItem> tasks = new ObservableCollection<TaskItem>();

        public TasksPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            BusyModal.IsModal = true;
            tasksList.ItemsSource = tasks;
            completedTasksList.ItemsSource = completedtasks;
            listsCatComboBox.ItemsSource = ListPage.lists;

            var param = (int)e.Parameter;
            listsCatComboBox.SelectedIndex = param;

            await getCurrTasks();
            if (tasks.Count == 0 && completedtasks.Count == 0)
            {
                foundTasks.Visibility = Visibility.Collapsed;
                notfoundTasks.Visibility = Visibility.Visible;
            }
            BusyModal.IsModal = false;
        }

        private async Task getCurrTasks()
        {
            BusyModal.IsModal = true;
            await API.getTasks();
            tasks.Clear();
            completedtasks.Clear();

            if (ListPage.lists.Count() > 0 && listsCatComboBox.Items.Count > 0 && listsCatComboBox.SelectedIndex >= 0)
            {
                for (int i = 0; i < API.totalTasks.Count; i++)
                    if (API.totalTasks.ElementAt(i).ListId.Equals(ListPage.lists.ElementAt(listsCatComboBox.SelectedIndex).Id)
                        && !API.totalTasks.ElementAt(i).Expired)
                        if (API.totalTasks.ElementAt(i).Completed == false) tasks.Add(API.totalTasks.ElementAt(i));
                        else completedtasks.Add(API.totalTasks.ElementAt(i));
            }
            if (completedtasks.Count > 0) completedTasksTextBox.Visibility = Visibility.Visible;
            else completedTasksTextBox.Visibility = Visibility.Collapsed;

            if (tasks.Count == 0 && completedtasks.Count == 0)
            {
                foundTasks.Visibility = Visibility.Collapsed;
                notfoundTasks.Visibility = Visibility.Visible;
            }
            else
            {
                notfoundTasks.Visibility = Visibility.Collapsed;
                foundTasks.Visibility = Visibility.Visible;
            }
            BusyModal.IsModal = false;
        }

        private async void listsCatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listsCatComboBox.SelectedIndex >= 0)
            {
                await getCurrTasks();
                addItemButton.Visibility = Visibility.Visible;
            }
        }

        //Edit Task Item
        private async void uncompletedItemTemplate_LeftCommandRequested(object sender, EventArgs e)
        {
            var selecteditem = (sender as SlidableListItem).DataContext as TaskItem;

            taskNameAddCDTextBox.Text = "";
            taskDescAddCDTextBox.Text = "";
            priorityList.SelectedIndex = 0;
            hasDeadLine.IsOn = false;
            deadlineDatePicker.Visibility = Visibility.Collapsed;
            deadlineTimePicker.Visibility = Visibility.Collapsed;
            taskNameAddCDTextBox.Text = selecteditem.TaskName;
            taskDescAddCDTextBox.Text = selecteditem.Description;

            var result = await addTaskContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                DateTime deadline = new DateTime(2015, 1, 1, 12, 0, 0);
                if (hasDeadLine.IsOn)
                {
                    var date = deadlineDatePicker.Date;
                    var time = deadlineTimePicker.Time;
                    deadline = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, time.Hours, time.Minutes, 0);
                }
                TaskItem newtask = new TaskItem()
                {
                    Id = selecteditem.Id,
                    TaskName = taskNameAddCDTextBox.Text,
                    Description = taskDescAddCDTextBox.Text,
                    DateCreated = new DateTime(2016, 10, 1),
                    Deadline = deadline,
                    Completed = false,
                    Notified = selecteditem.Notified,
                    Priority = priorityList.SelectedIndex,
                    UserId = API.currentUser.Id,
                    ListId = (listsCatComboBox.SelectedItem as List).Id
                };
                await API.editTask(newtask);
                await getCurrTasks();
            }
        }

        //Delete Completed TaskItem
        private async void uncompletedItemTemplate_RightCommandRequested(object sender, EventArgs e)
        {
            var deletedItem = (sender as SlidableListItem).DataContext as TaskItem;
            await API.removeTask(deletedItem);
            await getCurrTasks();
        }

        //Add taskitem from content dialog(event called by button:AddItem)
        private async void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (listsCatComboBox.SelectedIndex >= 0)
            {
                taskNameAddCDTextBox.Text = "";
                taskDescAddCDTextBox.Text = "";
                priorityList.SelectedIndex = 0;
                hasDeadLine.IsOn = false;
                deadlineDatePicker.Visibility = Visibility.Collapsed;
                deadlineTimePicker.Visibility = Visibility.Collapsed;
                var result = await addTaskContentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    DateTime deadline = new DateTime(2015,1,1,12,0,0);
                    bool success = true;
                    if (hasDeadLine.IsOn)
                    {
                        var date = deadlineDatePicker.Date;
                        if(date == null)
                        {
                            success = false;
                            var message = new MessageDialog("In Order To Add DeadLine, enter a Valid Date!");
                            await message.ShowAsync();
                        }
                        var time = deadlineTimePicker.Time;
                        if(time == null)
                        {
                            success = false;
                            var message = new MessageDialog("In Order To Add DeadLine, enter a Valid Date!");
                            await message.ShowAsync();
                        }
                        if(success) deadline = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, time.Hours, time.Minutes, 0);
                    }
                    if (success)
                    {
                        TaskItem newtask = new TaskItem()
                        {
                            TaskName = taskNameAddCDTextBox.Text,
                            Description = taskDescAddCDTextBox.Text,
                            DateCreated = DateTime.Now,
                            Deadline = deadline,
                            Completed = false,
                            Notified = false,
                            Priority = priorityList.SelectedIndex,
                            UserId = API.currentUser.Id,
                            ListId = (listsCatComboBox.SelectedItem as List).Id
                        };
                        await API.addTask(newtask);
                        await getCurrTasks();
                    }
                }
            }
            else
            {
                notSelectedListFlyout.ShowAt(addItemButton);
            }
        }

        //Hide flyout when click "OK"
        private void innerAddItemButtonFlyout_Click(object sender, RoutedEventArgs e)
        {
            notSelectedListFlyout.Hide();
        }

        //Check same taskitem name
        public void editTaskNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var panel = textbox.Parent as StackPanel;
            var contentdialog = panel.Parent as ContentDialog;
            DependencyObject child = VisualTreeHelper.GetChild(panel, 1);
            var innerpanel = child as StackPanel;
            innerpanel.Visibility = searchCurrentItems(textbox.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (textbox.Text == "" || innerpanel.Visibility == Visibility.Visible) contentdialog.IsPrimaryButtonEnabled = false;
            else contentdialog.IsPrimaryButtonEnabled = true;
        }

        private void hasDeadLine_Toggled(object sender, RoutedEventArgs e)
        {
            if (hasDeadLine.IsOn)
            {
                deadlineDatePicker.Visibility = Visibility.Visible;
                deadlineTimePicker.Visibility = Visibility.Visible;
                deadlineDatePicker.Date = DateTime.Now;
                deadlineTimePicker.Time = DateTime.Now.TimeOfDay;
            }
            else
            {
                deadlineDatePicker.Visibility = Visibility.Collapsed;
                deadlineTimePicker.Visibility = Visibility.Collapsed;
            }
        }

        public static bool searchCurrentItems(string name)
        {
            bool found = false;
            for (int i = 0; i < tasks.Count(); i++)
            {
                if (tasks.ElementAt(i).TaskName.Equals(name)) { found = true; }
            }
            return found;
        }

        private void tasksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = tasksList.SelectedItems;

            if (items.Count > 0)
            {
                GonnaCompletePanel.Visibility = Visibility.Visible;
                taskCount.Text = "" + items.Count;
            }
            else
            {
                GonnaCompletePanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void makeCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            BusyModal.IsModal = true;
            var items = tasksList.SelectedItems;

            foreach (var item in items)
            {
                TaskItem task = item as TaskItem;
                if(task.Completed == false)
                {
                    task.Completed = true;
                    await API.editTask(task);
                }
            }
            await getCurrTasks();
            BusyModal.IsModal = false;
        }

        private void unSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            tasksList.SelectedIndex = -1;
        }
      
        private void sortByAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            sortByContentDial();
        }

        public async void sortByContentDial()
        {
            var dialog = new ContentDialog()
            {
                Title = "Sort Tasks By: ",
                VerticalAlignment = VerticalAlignment.Stretch,
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var panel = new StackPanel();

            var byPriority = new RadioButton()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "By Priority",
                Margin = new Thickness(10, 10, 10, 0)
            };
            byPriority.Click += Bypriority_Click;

            var byAZ = new RadioButton()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "By A-Z",
                Margin = new Thickness(10, 10, 10, 0)
            };
            byAZ.Click += ByAZ_Click;

            var byDate = new RadioButton()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "By Date Created",
                Margin = new Thickness(10, 10, 10, 0)
            };
            byDate.Click += ByDate_Click;

            panel.Children.Add(byPriority);
            panel.Children.Add(byAZ);
            panel.Children.Add(byDate);

            dialog.Content = panel;

            var result = await dialog.ShowAsync();
        }

        private void ByDate_Click(object sender, RoutedEventArgs e)
        {
            sortByDateCreated();
        }

        private void ByAZ_Click(object sender, RoutedEventArgs e)
        {
            sortByAZ();
        }

        private void Bypriority_Click(object sender, RoutedEventArgs e)
        {
            sortByPriority();
        }

        public void sortByPriority()
        {
            List<TaskItem> sortedList = tasks.OrderBy(x => x.Priority).ToList();
            tasks.Clear();
            for(int i = 0; i < sortedList.Count; i++)
            {
                 tasks.Add(sortedList.ElementAt(i));
            }
        }

        public void sortByAZ()
        {
            List<TaskItem> sortedList = tasks.OrderBy(x => x.TaskName).ToList();
            tasks.Clear();
            for (int i = 0; i < sortedList.Count; i++)
            {
                tasks.Add(sortedList.ElementAt(i));
            }
        }

        public void sortByDateCreated()
        {
            List<TaskItem> sortedList = tasks.OrderBy(x => x.DateCreated).ToList();
            tasks.Clear();
            for (int i = 0; i < sortedList.Count; i++)
            {
                tasks.Add(sortedList.ElementAt(i));
            }
        }
    }
}
