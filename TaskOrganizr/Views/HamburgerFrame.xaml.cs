using Microsoft.WindowsAzure.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TaskOrganizr;
using TDLApi.DataModel;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    public sealed partial class HamburgerFrame : Page, INotifyPropertyChanged
    {
        /* Observable Collections for HamburgerMenu */
        public ObservableCollection<HamburgerItem> items;
        public ObservableCollection<HamburgerItem> optionItems;

        private int x1, x2;
        /* Enable/Disable Hamburger Button */
        private bool enabled = false;

        public static TimeTrigger trigger = null;

        public HamburgerFrame()
        {
            this.InitializeComponent();
            this.DataContext = this;

            /* Manipulation for enabling slide-open the hamburger menu */
            hamGrid.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            hamGrid.ManipulationStarted += HamGrid_ManipulationStarted;
            hamGrid.ManipulationCompleted += HamGrid_ManipulationCompleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            /* Add back button at titlebar only when he is on desktop */
            if (!ApiInformation.IsApiContractPresent("Windows.ApplicationModel.Calls.CallsPhoneContract", 1, 0))
            {
                var currentView = SystemNavigationManager.GetForCurrentView();
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                currentView.BackRequested += CurrentView_BackRequested;
            }

            items = new ObservableCollection<HamburgerItem>();

            items.Add(new HamburgerItem() { Icon = Symbol.Home, Name = "Home", PageType = typeof(ListPage), containsPanel = true });
            items.Add(new HamburgerItem() { Icon = Symbol.AllApps, Name = "Expired Tasks", PageType = typeof(ExpiredTasksPage), containsPanel = true });
            items.Add(new HamburgerItem() { Icon = Symbol.List, Name = "Categories", PageType = typeof(CategoriesPage), containsPanel = true });
            items.Add(new HamburgerItem() { Icon = Symbol.ContactInfo, Name = "Profile", PageType = typeof(ProfilePage), containsPanel = true });
            items.Add(new HamburgerItem() { Icon = Symbol.Setting, Name = "Settings", PageType = typeof(SettingsPage), containsPanel = true });

            optionItems = new ObservableCollection<HamburgerItem>();

            optionItems.Add(new HamburgerItem() { Image = API.currentUser.ImageUri == null ? "/Images/profile.png" : API.currentUser.ImageUri, Email = API.currentUser.Email, Line = "true" });
            optionItems.Add(new HamburgerItem() { Icon = Symbol.Help, Name = "About", PageType = typeof(AboutPage), containsPanel = true });

            
            /* Create a background Task that running every 15 mins to bring notifications to user */
            trigger = new TimeTrigger(15, false);
            bool found = false;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == BackgroundTask.TaskExpiredTaskName )
                {
                    AttachProgressAndCompletedHandlers(task.Value);
                    BackgroundTask.UpdateBackgroundTaskRegistrationStatus(true);
                    found = true;
                    break;
                }
            }
            /* Create new bg task only if a new user is logged in */
            if (found)
            {
                /* get userid value from background task */
                string userId = ApplicationData.Current.LocalSettings.Values["userid"].ToString();
                /* Check if logged in user has the same userId */
                /* If no, register a new task with its own userid */
                if (userId == null || !API.currentUser.Id.Equals(userId))
                {
                    UnregisterBackgroundTask();
                    RegisterBackgroundTask();
                }
            }
            else
            {
                RegisterBackgroundTask();
            }
        }

        private void RegisterBackgroundTask()
        {
            var task = BackgroundTask.RegisterBackgroundTask(trigger);
            AttachProgressAndCompletedHandlers(task);
        }

        private void UnregisterBackgroundTask()
        {
            BackgroundTask.UnregisterBackgroundTasks();
        }

        private void AttachProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
        }

        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var progress = "Progress: " + args.Progress + "%";
                BackgroundTask.TaskExpiredTaskProgress = progress;
            });
        }

        /* For later purpose in order to use WebJob */
        /*private async Task InitNotificationsAsync()
        {
            var hub = new NotificationHub(API.HubName, API.HubConnectionString);

            // Get a channel URI from WNS.
            var channel = await PushNotificationChannelManager
                .CreatePushNotificationChannelForApplicationAsync();

            string[] tags = { API.currentUser.Id };

            // Register the channel URI with Notification Hubs.
            //await App.MobileService.GetPush().RegisterAsync(channel.Uri);
            await hub.RegisterNativeAsync(channel.Uri, tags);

            channel.PushNotificationReceived += Channel_PushNotificationReceived;
        }

        private void Channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            //
        }*/

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
            }
        }

        private async void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = e.ClickedItem as HamburgerItem;

            /* If the item contains email, that means that the user clicked the logout button */
            if (menuItem.Email != null)
            {
                API.currentUser = null;
                
                /* If a user has logged in via facebook, remove his creds from vault */
                if (API.user != null)
                {
                    LoginPage.vault.Remove(LoginPage.credential);
                    LoginPage.credential = null;
                    await App.MobileService.LogoutAsync();
                }
                API.user = null;

                /* Reset rememberme.txt */
                var folder = ApplicationData.Current.LocalFolder;
                var newFolder = await folder.CreateFolderAsync("rememberme", CreationCollisionOption.OpenIfExists);
                var files = await newFolder.GetFilesAsync();
                var desiredFile = files.FirstOrDefault(x => x.Name == "rememberme.txt");
                if (desiredFile == null)
                {
                    desiredFile = await newFolder.CreateFileAsync("rememberme.txt");
                }
                string[] logincred = new string[2];
                logincred[0] = "";
                logincred[1] = "";
                await FileIO.WriteLinesAsync(desiredFile, logincred);
                
                Frame.Navigate(typeof(LoginPage));
            }
            else
            {
                for(int i=0; i<items.Count; i++)
                {
                    items.ElementAt(i).hasRect = null;
                    if (items.ElementAt(i).Name == menuItem.Name) items.ElementAt(i).hasRect = "asd";
                }

                for (int i = 0; i < optionItems.Count; i++)
                {
                    optionItems.ElementAt(i).hasRect = null;
                    if (optionItems.ElementAt(i).Name == menuItem.Name) optionItems.ElementAt(i).hasRect = "asd";
                }

                if (menuItem.containsPanel) contentFrame.Navigate(menuItem.PageType, 0);
                else Frame.Navigate(menuItem.PageType);
            }
        }

        private void hamGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (hamGrid.ActualWidth >= 1280-48) isEnabled = false;
            else isEnabled = true;
        }

        private void HamGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (hamGrid.ActualWidth < 1280-48)
            {
                x2 = (int)e.Position.X;
                if (Math.Abs(x1 - x2) >= 200)
                {
                    if (x1 <= 240 && x1 > x2)
                        hamburgerMenuControl.IsPaneOpen = false;
                    else if (x1 <= 48 && x1 < x2)
                        hamburgerMenuControl.IsPaneOpen = true;
                }
            }
        }

        private void HamGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            x1 = (int)e.Position.X;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool isEnabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                NotifyPropertyChanged("isEnabled");  // Trigger the change event if the value is changed!
            }
        }
    }
}