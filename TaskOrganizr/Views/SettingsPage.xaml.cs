using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            UserSettings setting = await getUserSettingsfromUser();
            if (setting != null)
            {
                themeComboBox.SelectedIndex = setting.themeId;
                notify1dayBef.IsOn = setting.NotifyTasksOneDayBefore;
            }
        }

        public async static Task<UserSettings> getUserSettingsfromUser()
        {
            await API.getUserSettings();
            foreach (UserSettings x in API.userSettings)
            {
                if (API.currentUser.Id.Equals(x.UserId))
                {
                    return x;
                }
            }
            return null;
        }

        private async void themeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserSettings setting = await getUserSettingsfromUser();
            if(setting != null)
            {
                setting.themeId = themeComboBox.SelectedIndex;
                await API.editUserSettings(setting);
            }
        }

        private async void notify1dayBef_Toggled(object sender, RoutedEventArgs e)
        {
            UserSettings setting = await getUserSettingsfromUser();
            if (setting != null)
            {
                setting.NotifyTasksOneDayBefore = notify1dayBef.IsOn;
                await API.editUserSettings(setting);
            }
        }
    }
}
