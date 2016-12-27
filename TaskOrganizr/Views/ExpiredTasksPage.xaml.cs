using Microsoft.Toolkit.Uwp.UI.Controls;
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
    public sealed partial class ExpiredTasksPage : Page
    {
        private static ObservableCollection<TaskItemExtented> expiredTasks = new ObservableCollection<TaskItemExtented>();

        public ExpiredTasksPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            expiredTasksList.ItemsSource = expiredTasks;
            BusyModal.IsModal = true;
            getExpiredTasks();
            BusyModal.IsModal = false;
        }

        private async void getExpiredTasks()
        {
            expiredTasks.Clear();
            await API.getTasks();
            foreach (TaskItem task in API.totalTasks) if (task.Expired) expiredTasks.Add(new TaskItemExtented(task));
            if (expiredTasks.Count == 0)
            {
                noExpiredTasksPanel.Visibility = Visibility.Visible;
                expiredTasksList.Visibility = Visibility.Collapsed;
            }
            else
            {
                noExpiredTasksPanel.Visibility = Visibility.Collapsed;
                expiredTasksList.Visibility = Visibility.Visible;
            }
        }

        private async void taskItemExpiredTemplate_RightCommandRequested(object sender, EventArgs e)
        {
            var deletedext = (sender as SlidableListItem).DataContext as TaskItemExtented;
            await API.removeTask(deletedext.task);
            getExpiredTasks();
        }
    }
}
