using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace TaskOrganizr
{
    class BackgroundTask
    {
        public static string BackgroundTaskEntryPoint = "TaskOrganizrBackgroundTask.BackgroundTaskExpired";
        public static string TaskExpiredTaskName = "TaskExpired";
        public static string TaskExpiredTaskProgress = "";
        public static bool TaskExpiredTaskRegistered = false;

        public static BackgroundTaskRegistration RegisterBackgroundTask(IBackgroundTrigger trigger)
        {
            var requestTask = BackgroundExecutionManager.RequestAccessAsync();

            var builder = new BackgroundTaskBuilder();

            builder.Name = TaskExpiredTaskName;
            builder.TaskEntryPoint = BackgroundTaskEntryPoint;
            builder.SetTrigger(trigger);

            BackgroundTaskRegistration task = builder.Register();

            UpdateBackgroundTaskRegistrationStatus(true);

            //
            // Remove previous completion status.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(TaskExpiredTaskName);
            settings.Values.Remove("userid");
            settings.Values["userid"] = TDLApi.API.currentUser.Id;

            return task;
        }

        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        public static void UnregisterBackgroundTasks()
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == TaskExpiredTaskName)
                {
                    cur.Value.Unregister(true);
                }
            }

            UpdateBackgroundTaskRegistrationStatus(false);
        }

        /// <summary>
        /// Store the registration status of a background task with a given name.
        /// </summary>
        /// <param name="name">Name of background task to store registration status for.</param>
        /// <param name="registered">TRUE if registered, FALSE if unregistered.</param>
        public static void UpdateBackgroundTaskRegistrationStatus(bool registered)
        {
            TaskExpiredTaskRegistered = registered;
        }

        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = TaskExpiredTaskRegistered;

            var status = registered ? "Registered" : "Unregistered";

            object taskStatus;
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue(name, out taskStatus))
            {
                status += " - " + taskStatus.ToString();
            }

            return status;
        }

        /// <summary>
        /// Determine if task with given name requires background access.
        /// </summary>
        /// <param name="name">Name of background task to query background access requirement.</param>
        public static bool TaskRequiresBackgroundAccess()
        {
            return true;
        }
    }
}
