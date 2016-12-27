using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Popups;
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
    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();
            refreshText();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            BusyModal.IsModal = true;
            await API.getCategories();
            await API.getLists();
            await API.getTasks();
            totalCategoriesText.Text = API.categories.Count.ToString() + " Categories.";
            totalListsText.Text = API.totalLists.Count.ToString() + " Lists.";     
            totalTasksText.Text = (TasksPage.tasks.Count+TasksPage.completedtasks.Count).ToString() + " Tasks.";
            refreshText();
            BusyModal.IsModal = false;
        }

        private async void deleteAccButton_Click(object sender, RoutedEventArgs e)
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
                Text = "Do you want to remove your account with\nusername: " + API.currentUser.Username + "\nCompletely?"
            };

            panel.Children.Add(text);

            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await API.removeUser(API.currentUser);
                API.currentUser = new User()
                {
                    Username = ""
                };
                await LoginPage.clearUser();
                API.user = null;
                Frame rootFrame = Window.Current.Content as Frame;
                Window.Current.Activate();
                rootFrame.Navigate(typeof(LoginPage));
            }
        }

        private async void editProfileButton_Click(object sender, RoutedEventArgs e)
        {
            FirstNameTextBox.Text = API.currentUser.FirstName == null ? "" : API.currentUser.FirstName;
            LastNameTextBox.Text = API.currentUser.LastName == null ? "" : API.currentUser.LastName;
            genderToggle.IsOn = API.currentUser.Gender == null ? false : (API.currentUser.Gender.Equals("male") ? false : true);
            var result = await editProfileContentDial.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                API.currentUser.FirstName = FirstNameTextBox.Text;
                API.currentUser.LastName = LastNameTextBox.Text;
                if (genderToggle.IsOn) API.currentUser.Gender = "female";
                else API.currentUser.Gender = "male";
                await API.editUser(API.currentUser);
                refreshText();
            }
        }

        private void refreshText()
        {
            emailText.Text = API.currentUser.Email;
            userText.Text = API.currentUser.Username;
            firstNameText.Text = API.currentUser.FirstName == null ? "" : API.currentUser.FirstName;
            lastNameText.Text = API.currentUser.LastName == null ? "" : API.currentUser.LastName;
            genderText.Text = API.currentUser.Gender == null ? "" : API.currentUser.Gender;
            if (API.currentUser.ImageUri == null) API.imageUri = "/Images/profile.png";
            else API.imageUri = API.currentUser.ImageUri;
        }

        private async void associatewithFbLogin_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            try
            {
                result = await AssociateUserAsync();
            }
            catch (InvalidOperationException)
            {
                var message = new MessageDialog("An error has been occured!");
                await message.ShowAsync();
                await LoginPage.clearUser();
                Frame.Navigate(typeof(LoginPage));
            }
            catch (FileNotFoundException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }

            if (result) Frame.Navigate(typeof(ProfilePage));
            else
            {
                await new MessageDialog("An Error has occured!").ShowAsync();
            }
        }

        private async Task<bool> AssociateUserAsync()
        {
            bool success = false;
            try
            {
                // Login with the identity provider.
                API.user = await App.MobileService
                    .LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                // Create and store the user credentials.
                LoginPage.credential = new PasswordCredential(MobileServiceAuthenticationProvider.Facebook.ToString(),
                API.user.UserId, API.user.MobileServiceAuthenticationToken);
                LoginPage.vault.Add(LoginPage.credential);
                UserInfo userInfo = await App.MobileService.InvokeApiAsync<UserInfo>("UserInfo", System.Net.Http.HttpMethod.Get, null);
                if (await LoginPage.addUserFromFacebook(userInfo, API.currentUser.Username, API.currentUser.Id)) success = true;
            }
            catch (MobileServiceInvalidOperationException)
            {
                var message = new MessageDialog("Login is required to continue!");
                await message.ShowAsync();
            }
            return success;
        }
    }
}
