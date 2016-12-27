using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Composition;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using TDLApi.DataModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        // Use the PasswordVault to securely store and access credentials.
        public static PasswordVault vault = new PasswordVault();
        public static PasswordCredential credential = null;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        public async void getUsers()
        {
            loginButtonRing.Visibility = Visibility.Visible;
            loginButtonRing.IsActive = true;
            loginButton.IsEnabled = false;
            await API.getUsers();
            loginButtonRing.Visibility = Visibility.Collapsed;
            loginButtonRing.IsActive = false;
            loginButton.IsEnabled = true;

            loadLogin();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            getUsers();

            try
            {
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(MobileServiceAuthenticationProvider.Facebook.ToString()).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }

            if(credential != null)
            {
                bool result = false;
                try
                {
                    result = await logInFacebookUser();
                }
                catch (MobileServiceInvalidOperationException)
                {
                    var message = new MessageDialog("Login is required to continue!");
                    await message.ShowAsync();
                }

                if (result) Frame.Navigate(typeof(HamburgerFrame));
                else
                {
                    await clearUser();
                    Frame.Navigate(typeof(LoginPage));
                }
            }
        }

        private void registerNowButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (usernameTextBox.Text == "" || passwordPassBox.Password == "")
            {
                var message_diag = new MessageDialog("Enter Credentials First");
                await message_diag.ShowAsync();
            }
            else
            {
                bool found = false;
                for (int i = 0; i < API.users.Count(); i++)
                {
                    var pass = GetHashString(passwordPassBox.Password);
                    if (usernameTextBox.Text.Equals(API.users.ElementAt(i).Username)
                        && pass.Equals(API.users.ElementAt(i).Password)
                        && API.users.ElementAt(i).Provider == null)
                    {

                        found = true;
                        //var message_diag = new MessageDialog("Login Success");
                        //await message_diag.ShowAsync();
                        API.currentUser = API.users.ElementAt(i);
                        UserSettings setting = await SettingsPage.getUserSettingsfromUser();
                        if (setting == null)
                        {
                            setting = new UserSettings()
                            {
                                UserId = API.currentUser.Id,
                                themeId = 0,
                                NotifyTasksOneDayBefore = false
                            };
                            await API.addUserSettings(setting);
                        }
                        saveLogin();
                        Frame.Navigate(typeof(HamburgerFrame));
                    }
                }
                if (!found)
                {
                    var message_diag = new MessageDialog("Login Failed");
                    await message_diag.ShowAsync();
                }
            }
        }

        private void passwordPassBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                loginButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void saveLogin()
        {
            if (rememberMeCheckBox.IsChecked == true)
            {
                var folder = ApplicationData.Current.LocalFolder;
                var newFolder = await folder.CreateFolderAsync("rememberme", CreationCollisionOption.OpenIfExists);
                var files = await newFolder.GetFilesAsync();
                var desiredFile = files.FirstOrDefault(x => x.Name == "rememberme.txt");
                if (desiredFile == null)
                {
                    desiredFile = await newFolder.CreateFileAsync("rememberme.txt");
                }
                string[] logincred = new string[2];
                logincred[0] = usernameTextBox.Text;
                logincred[1] = passwordPassBox.Password;
                await FileIO.WriteLinesAsync(desiredFile, logincred);
            }
        }

        public async void loadLogin()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var newFolder = await folder.CreateFolderAsync("rememberme", CreationCollisionOption.OpenIfExists);
            var files = await newFolder.GetFilesAsync();
            var desiredFile = files.FirstOrDefault(x => x.Name == "rememberme.txt");
            if (desiredFile != null)
            {
                IList<string> textContent = await FileIO.ReadLinesAsync(desiredFile);
                string username = "";
                string password = "";
                for (int i = 0; i < textContent.Count; i++)
                {
                    if (i == 0) username = textContent[i];
                    else if (i == 1) password = textContent[i];
                }
                if (!username.Equals("") && !password.Equals(""))
                {
                    bool found = false;
                    for (int i = 0; i < API.users.Count(); i++)
                    {
                        if (username.Equals(API.users.ElementAt(i).Username)
                        && password.Equals(API.users.ElementAt(i).Password)
                        && API.users.ElementAt(i).Provider == null)
                        {
                            found = true;
                            //var message_diag = new MessageDialog("Login Success");
                            //await message_diag.ShowAsync();
                            API.currentUser = API.users.ElementAt(i);
                            Frame.Navigate(typeof(HamburgerFrame));
                        }
                    }
                    if (!found)
                    {
                        var message_diag = new MessageDialog("Login Failed");
                        await message_diag.ShowAsync();
                    }
                }
            }
        }

        private async void fbLogin_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            try
            {
                result = await AuthenticateUserAsync();
            }
            catch (InvalidOperationException)
            {
                var message = new MessageDialog("An error has been occured!");
                await message.ShowAsync();
                await clearUser();
                Frame.Navigate(typeof(LoginPage));
            }
            catch (FileNotFoundException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }

            if (result) Frame.Navigate(typeof(HamburgerFrame));
            else
            {
                await clearUser();
                Frame.Navigate(typeof(LoginPage));
            }
        }

        public async Task<bool> AuthenticateUserAsync()
        {
            bool success = false;
            try
            {
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(MobileServiceAuthenticationProvider.Facebook.ToString()).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }
            if (credential != null)
            {
                try
                {
                    success = await logInFacebookUser();
                }
                catch (MobileServiceInvalidOperationException)
                {
                    var message = new MessageDialog("Login is required to continue!");
                    await message.ShowAsync();
                }
            }
            else
            {
                try
                {
                    // Login with the identity provider.
                    API.user = await App.MobileService
                        .LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                    // Create and store the user credentials.
                    credential = new PasswordCredential(MobileServiceAuthenticationProvider.Facebook.ToString(),
                        API.user.UserId, API.user.MobileServiceAuthenticationToken);
                    vault.Add(credential);
                    UserInfo userInfo = await App.MobileService.InvokeApiAsync<UserInfo>("UserInfo", System.Net.Http.HttpMethod.Get, null);
                    if (await isUserFoundinDatabase(userInfo))
                    {
                        await updateUserToken(API.currentUser, API.user.MobileServiceAuthenticationToken);
                        success = true;
                    }
                    else if (await addUserFromFacebook(userInfo))
                    {
                        Frame.Navigate(typeof(HamburgerFrame));
                        success = true;
                    }
                }
                catch (MobileServiceInvalidOperationException)
                {
                    var message = new MessageDialog("Login is required to continue!");
                    await message.ShowAsync();
                }
            }
            return success;
        }

        public async static Task clearUser()
        {
            if(API.user != null)
            {
                vault.Remove(credential);
                credential = null;
                await App.MobileService.LogoutAsync();
            }
        }

        public async Task<bool> logInFacebookUser()
        {
            bool success = false;
            API.user = new MobileServiceUser(credential.UserName);
            credential.RetrievePassword();
            API.user.MobileServiceAuthenticationToken = credential.Password;
            App.MobileService.CurrentUser = API.user;
            if (isTokenExpired(API.user.MobileServiceAuthenticationToken))
            {
                await clearUser();
                if (await AuthenticateUserAsync()) Frame.Navigate(typeof(HamburgerFrame));
                else
                {
                    var message = new MessageDialog("An error has been occured!");
                    await message.ShowAsync();
                    await clearUser();
                    Frame.Navigate(typeof(LoginPage));
                }
            }
            else
            {
                var userInfo = await App.MobileService.InvokeApiAsync<UserInfo>("UserInfo", System.Net.Http.HttpMethod.Get, null);
                if (await isUserFoundinDatabase(userInfo)) success = true;
                else if (await addUserFromFacebook(userInfo)) success = true;
            }
            return success;
        }

        public static async Task<bool> isUserFoundinDatabase(UserInfo userInfo)
        {
            await API.getUsers();
            MobileServiceCollection<User, User> fbUsers = await API.usersTable.ToCollectionAsync();
            foreach (var user in API.users)
            {
                if (user.Provider == null) continue;
                if (!user.Provider.Equals("facebook"))
                {
                    fbUsers.Remove(user);
                }
            }

            bool found = false;
            foreach (var fbuser in fbUsers)
            {
                if (userInfo.Id == fbuser.UserProviderId)
                {
                    found = true;
                    API.currentUser = fbuser;
                }
            }

            return found;
        }

        public static async Task updateUserToken(User user, string token)
        {
            if (user.AccessToken != token)
            {
                user.AccessToken = token;
                await API.editUser(user);
            }
        }

        public static async Task<bool> addUserFromFacebook(UserInfo userInfo, string username = null, string userid = null)
        {
            bool success = false;
            if (searchCurrentEmail(userInfo.EmailAddress))
            {
                var msg = new MessageDialog("Email already Exists!");
                await msg.ShowAsync();
                success = false;
            }
            else if(username == null)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Add an username",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "Cancel"
                };

                var panel = new StackPanel();

                var addlname = new TextBox()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(10, 10, 10, 10),
                    PlaceholderText = "UserName Name here.."
                };
                addlname.TextChanged += new TextChangedEventHandler(addUserNameTextBox_TextChanged);

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
                    Text = "UserName name already exists!",
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
                    string[] words = userInfo.Name.Split(' ');
                    int i = 0;
                    string firstname = "", lastname = "";
                    foreach (string word in words)
                    {
                        if (i == 0) { firstname = word; i++; }
                        else if (i == 1) { lastname = word; i++; }
                    }

                    //new user and add new
                    User newuser = new User()
                    {
                        Username = addlname.Text,
                        FirstName = firstname,
                        LastName = lastname,
                        Provider = "facebook",
                        Gender = userInfo.Gender,
                        Email = userInfo.EmailAddress,
                        UserProviderId = userInfo.Id,
                        ImageUri = userInfo.ImageUri,
                        FirstJoin = true,
                        AccessToken = API.user.MobileServiceAuthenticationToken
                    };

                    await API.addUser(newuser);
                    API.currentUser = newuser;
                    UserSettings setting = new UserSettings()
                    {
                        UserId = API.currentUser.Id,
                        themeId = 0,
                        NotifyTasksOneDayBefore = true
                    };
                    await API.addUserSettings(setting);
                    success = true;
                }
            }
            else
            {
                string[] words = userInfo.Name.Split(' ');
                int i = 0;
                string firstname = "", lastname = "";
                foreach (string word in words)
                {
                    if (i == 0) { firstname = word; i++; }
                    else if (i == 1) { lastname = word; i++; }
                }

                API.currentUser.Username = username;
                API.currentUser.FirstName = firstname;
                API.currentUser.LastName = lastname;
                API.currentUser.Password = null;
                API.currentUser.Provider = "facebook";
                API.currentUser.Gender = userInfo.Gender;
                API.currentUser.Email = userInfo.EmailAddress;
                API.currentUser.UserProviderId = userInfo.Id;
                API.currentUser.ImageUri = userInfo.ImageUri;
                API.currentUser.FirstJoin = false;
                API.currentUser.AccessToken = API.user.MobileServiceAuthenticationToken;

                await API.editUser(API.currentUser);
                success = true;
            }
            return success;
        }

        private static void addUserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var panel = textbox.Parent as StackPanel;
            var contentdialog = panel.Parent as ContentDialog;
            DependencyObject child = VisualTreeHelper.GetChild(panel, 1);
            var innerpanel = child as StackPanel;
            innerpanel.Visibility = searchCurrentName(textbox.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (textbox.Text == "" || innerpanel.Visibility == Visibility.Visible) contentdialog.IsPrimaryButtonEnabled = false;
            else contentdialog.IsPrimaryButtonEnabled = true;
        }

        private bool isTokenExpired(string token)
        {
            // Get just the JWT part of the token.
            var jwt = token.Split(new Char[] { '.' })[1];

            // Undo the URL encoding.
            jwt = jwt.Replace('-', '+');
            jwt = jwt.Replace('_', '/');
            switch (jwt.Length % 4)
            {
                case 0: break;
                case 2: jwt += "=="; break;
                case 3: jwt += "="; break;
                default:
                    throw new System.Exception(
               "The base64url string is not valid.");
            }

            // Decode the bytes from base64 and write to a JSON string.
            var bytes = Convert.FromBase64String(jwt);
            string jsonString = UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            // Parse as JSON object and get the exp field value, 
            // which is the expiration date as a JavaScript primative date.
            JObject jsonObj = JObject.Parse(jsonString);
            var exp = Convert.ToDouble(jsonObj["exp"].ToString());

            // Calculate the expiration by adding the exp value (in seconds) to the 
            // base date of 1/1/1970.
            DateTime minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expire = minTime.AddSeconds(exp);

            return expire < DateTime.UtcNow ? true : false;
        }

        public static bool searchCurrentName(string name)
        {
            bool found = false;
            for (int i = 0; i < API.users.Count(); i++)
            {
                if (name.Equals(API.users.ElementAt(i).Username)) { found = true; }
            }
            return found;
        }

        public static bool searchCurrentEmail(string email)
        {
            bool found = false;
            for (int i = 0; i < API.users.Count(); i++)
            {
                if (email.Equals(API.users.ElementAt(i).Email)) { found = true; }
            }
            return found;
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
