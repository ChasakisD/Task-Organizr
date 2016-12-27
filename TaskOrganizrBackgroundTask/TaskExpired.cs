using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.WindowsAzure.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Notifications;

namespace TaskOrganizrBackgroundTask
{
    class TaskExpired
    {
        private static string TaskUrl = "https://tdlapi.azurewebsites.net/tables/taskitem";
        private static string UserUrl = "https://tdlapi.azurewebsites.net/tables/user";
        private static string UserSettingsUrl = "https://tdlapi.azurewebsites.net/tables/usersettings";

        public static async Task taskExpired(string userId)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");

            var taskresponse = await client.GetStringAsync(TaskUrl);
            var taskResult = JsonConvert.DeserializeObject<List<TaskItem>>(taskresponse);

            var userresponse = await client.GetStringAsync(UserUrl);
            var userResult = JsonConvert.DeserializeObject<List<User>>(userresponse);

            var userSettingsresponse = await client.GetStringAsync(UserSettingsUrl);
            var userSettingResult = JsonConvert.DeserializeObject<List<UserSettings>>(userSettingsresponse);

            NotificationHub hub = new NotificationHub("TaskExpired",
                 "Endpoint=sb://taskscheduler.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=XmFzPf7vz8veXiG51sIZikQkM6GCP3JmYPS/95ZAsLY=");

            foreach (TaskItem task in taskResult)
            {
                if (!task.UserId.Equals(userId)) continue;

                string imagepath = "";
                switch (task.Priority)
                {
                    case 0: { imagepath = "https://s30.postimg.org/rghwrikkt/red.png"; break; }
                    case 1: { imagepath = "https://s30.postimg.org/5ssyh2k6l/orange.png"; break; }
                    case 2: { imagepath = "https://s30.postimg.org/5dhoo1y99/green.png"; break; }
                    case 3: { imagepath = "https://s30.postimg.org/5oz50tep9/cyan.png"; break; }
                    case 4: { imagepath = "https://s30.postimg.org/6tt767165/magenta.png"; break; }
                }


                if (task.Deadline.Year == 2015) continue;
                else if (getUserSettingByTask(userResult, userSettingResult, task.UserId)
                         && (task.Deadline.Day == DateTime.Now.AddDays(1).Day)
                         && !task.Notified)
                {
                    sendToastNotification("Task: " + task.TaskName, "is About to Expire!", imagepath);
                   
                    //var result = hub.SendNotificationAsync(notification, task.UserId);

                    task.Notified = true;

                    var taskJson = JsonConvert.SerializeObject(task);
                    var HttpContent = new StringContent(taskJson);
                    HttpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    await PatchAsync(client, new Uri(TaskUrl + "/" + task.Id), HttpContent);
                }
                else if ((DateTimeOffset.Now.CompareTo(task.Deadline) > 0)
                         && !task.Expired
                         && !task.Completed)
                {
                    sendToastNotification("Task: " + task.TaskName, "Expired!", imagepath);

                    task.Expired = true;

                    var taskJson = JsonConvert.SerializeObject(task);
                    var HttpContent = new StringContent(taskJson);
                    HttpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    await PatchAsync(client, new Uri(TaskUrl + "/" + task.Id), HttpContent);
                }
            }

        }

        public static bool getUserSettingByTask(List<User> users, List<UserSettings> usersettings, string userId)
        {
            foreach (User user in users)
            {
                foreach (UserSettings usersetting in usersettings)
                {
                    if (user.Id.Equals(userId)
                        && user.Id.Equals(usersetting.UserId)
                        && usersetting.NotifyTasksOneDayBefore) return true;
                }
            }
            return false;
        }

        public static async Task<HttpResponseMessage> PatchAsync(HttpClient client, Uri requestUri, HttpContent iContent)
        {
            var method = new HttpMethod("PATCH");

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            HttpResponseMessage response = new HttpResponseMessage();
            // In case you want to set a timeout
            //CancellationToken cancellationToken = new CancellationTokenSource(60).Token;

            try
            {
                response = await client.SendAsync(request);
                // If you want to use the timeout you set
                //response = await client.SendRequestAsync(request).AsTask(cancellationToken);
            }
            catch (TaskCanceledException e)
            {
                //
            }

            return response;
        }

        public static void sendToastNotification(string name, string action, string imagepath)
        {
            ToastContent content = new ToastContent()
            {
                Launch = "app-defined-string",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = name },
                            new AdaptiveText(){ Text = action }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = imagepath,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        }
                    }
                }
            };

            var notification = new ToastNotification(content.GetXml());
            notification.ExpirationTime = DateTime.Now.AddDays(2);

            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
