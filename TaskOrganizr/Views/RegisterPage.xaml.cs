using Microsoft.Toolkit.Uwp.Services.Facebook;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using TDLApi.DataModel;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            makeTextBoxestoDefault();
        }

        private async void registerButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkNullTextBoxes())
            {
                if (!passwordRegBox.Password.Equals(confirmPasswordRegBox.Password))
                {
                    displayMsgDial("Unable to confirm Passwords!");
                    passwordRegBox.Password = "";
                    confirmPasswordRegBox.Password = "";
                }
                else if(passwordRegBox.Password.Length < 8)
                {
                    displayMsgDial("Password must contain at least 8 characters");
                    passwordRegBox.Password = "";
                    confirmPasswordRegBox.Password = "";
                }
                else
                {
                    bool found = searchCurrentName(usernameRegBox.Text);
                    if (found)
                    {
                        displayMsgDial("Username already exists!");
                        usernameRegBox.Text = "";
                    }
                    else
                    {
                        found = searchCurrentEmail(emailRegBox.Text);
                        if (found)
                        {
                            displayMsgDial("Email already exists!");
                            emailRegBox.Text = "";
                        }
                        else
                        {
                            registerButtonRing.IsActive = true;
                            registerButtonRing.Visibility = Visibility.Visible;
                            string pass = LoginPage.GetHashString(passwordRegBox.Password);
                            User user = new User()
                            {
                                Username = usernameRegBox.Text,
                                Password = pass,
                                Email = emailRegBox.Text,
                                FirstJoin = true
                            };
                            await API.addUser(user);
                            registerButtonRing.IsActive = false;
                            registerButtonRing.Visibility = Visibility.Collapsed;
                            displayMsgDial("Register Complete");

                            makeTextBoxestoDefault();

                            Frame.Navigate(typeof(LoginPage));
                        }
                    }
                }
            }
        }

        public bool searchCurrentName(string name)
        {
            bool found = false;
            for (int i = 0; i < API.users.Count(); i++)
            {
                if (name.Equals(API.users.ElementAt(i).Username)) { found = true; }
            }
            return found;
        }

        public bool searchCurrentEmail(string email)
        {
            bool found = false;
            for (int i = 0; i < API.users.Count(); i++)
            {
                if (email.Equals(API.users.ElementAt(i).Email)) { found = true; }
            }
            return found;
        }

        public async static void displayMsgDial(string msg)
        {
            var message_diag = new MessageDialog(msg);
            await message_diag.ShowAsync();
        }

        public void makeTextBoxestoDefault()
        {
            usernameRegBox.Text = "";
            passwordRegBox.Password = "";
            confirmPasswordRegBox.Password = "";
            emailRegBox.Text = "";
            userNameStackWarning.Visibility = Visibility.Collapsed;
            emailStackWarning.Visibility = Visibility.Collapsed;
        }

        public bool checkNullTextBoxes()
        {
            if (usernameRegBox.Text == ""
                || passwordRegBox.Password == ""
                || confirmPasswordRegBox.Password == ""
                || emailRegBox.Text == "")
            {
                displayMsgDial("All fields are necessary");
                return false;
            }
            return true;
        }

        private void usernameRegBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            userNameStackWarning.Visibility = searchCurrentName(usernameRegBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void emailRegBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            emailStackWarning.Visibility = searchCurrentEmail(emailRegBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void regBackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void registerButton_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            registerButton_Click(this, new RoutedEventArgs());
        }

    }
}
