using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace TDLApi
{
    class API
    {
        public static MobileServiceUser user;
        public static User currentUser;

        public static string imageUri;

        public static ObservableCollection<User> users = new ObservableCollection<User>();
        public static ObservableCollection<UserSettings> userSettings = new ObservableCollection<UserSettings>();
        public static ObservableCollection<Category> categories = new ObservableCollection<Category>();
        public static ObservableCollection<List> totalLists = new ObservableCollection<List>();
        public static ObservableCollection<TaskItem> totalTasks = new ObservableCollection<TaskItem>();

        public static IMobileServiceTable<User> usersTable = App.MobileService.GetTable<User>();
        public static IMobileServiceTable<UserSettings> userSettingsTable = App.MobileService.GetTable<UserSettings>();
        public static IMobileServiceTable<Category> categoriesTable = App.MobileService.GetTable<Category>();
        public static IMobileServiceTable<List> totalListsTable = App.MobileService.GetTable<List>();
        public static IMobileServiceTable<TaskItem> totalTasksTable = App.MobileService.GetTable<TaskItem>();

        #region Users
        public static async Task getUsers()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                MobileServiceCollection<User, User> wanted = await usersTable.ToCollectionAsync();

                foreach (User user in wanted)
                    users.Add(user);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }
            catch (HttpRequestException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }
        }
        public static async Task addUser(User user)
        {
            await usersTable.InsertAsync(user);
            users.Add(user);
        }
        public static async Task editUser(User user)
        {
            await usersTable.UpdateAsync(user);
            await getUsers();
        }
        public static async Task removeUser(User user)
        {
            await usersTable.DeleteAsync(user);
            await getUsers();
        }
        #endregion

        #region UserSettings
        public static async Task getUserSettings()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                MobileServiceCollection<UserSettings, UserSettings> wanted = await userSettingsTable.ToCollectionAsync();

                foreach (UserSettings user in wanted)
                    userSettings.Add(user);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }
            catch (HttpRequestException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }
        }
        public static async Task addUserSettings(UserSettings usersetting)
        {
            await userSettingsTable.InsertAsync(usersetting);
            userSettings.Add(usersetting);
        }
        public static async Task editUserSettings(UserSettings usersetting)
        {
            await userSettingsTable.UpdateAsync(usersetting);
            await getUserSettings();
        }
        public static async Task removeUserSettings(UserSettings usersetting)
        {
            await userSettingsTable.DeleteAsync(usersetting);
            await getUserSettings();
        }
        #endregion

        #region Categories
        public static async Task getCategories()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                MobileServiceCollection<Category, Category> wanted  = await categoriesTable.ToCollectionAsync();

                categories.Clear();
                foreach (var category in wanted) categories.Add(category);

                List<Category> gonnadelete = new List<Category>();

                foreach (var category in categories)
                {
                    if (!category.UserId.Equals(currentUser.Id)) gonnadelete.Add(category);
                    category.Hex = "#" + category.Hex;
                }

                foreach (Category i in gonnadelete) categories.Remove(i);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }
            catch (HttpRequestException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }
        }
        public static async Task addCategory(Category category)
        {
            await categoriesTable.InsertAsync(category);
            await getCategories();
        }
        public static async Task editCategory(Category category)
        {
            await categoriesTable.UpdateAsync(category);
            await getCategories();
        }
        public static async Task removeCategory(Category category)
        {
            List<List> lists_to_delete = new List<List>();
            lists_to_delete = await totalListsTable
                .Where(x => x.CategoryId == category.Id)
                .ToListAsync();
            foreach (var list in lists_to_delete)
            {
                await removeList(list);
            }
            await categoriesTable.DeleteAsync(category);
            categories.Remove(category);
            await getCategories();
        }
        #endregion

        #region Lists
        public static async Task getLists()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                MobileServiceCollection<List, List> wanted = await totalListsTable.ToCollectionAsync();

                totalLists.Clear();
                foreach (var list in wanted) totalLists.Add(list);

                List<List> gonnadelete = new List<List>();

                foreach (var list in totalLists)
                {
                    if (!list.UserId.Equals(currentUser.Id))
                    {
                        gonnadelete.Add(list);
                    }
                }

                foreach (List i in gonnadelete) totalLists.Remove(i);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }
            catch (HttpRequestException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }
        }
        public static async Task addList(List list)
        {
            await totalListsTable.InsertAsync(list);
            await getLists();
        }
        public static async Task editList(List list)
        {
            await totalListsTable.UpdateAsync(list);
            await getLists();
        }
        public static async Task removeList(List list)
        {
            List<TaskItem> tasks_to_delete = new List<TaskItem>();
            tasks_to_delete = await totalTasksTable
                .Where(x => x.ListId == list.Id)
                .ToListAsync();
            foreach(var task in tasks_to_delete)
            {
                await removeTask(task);
            }
            await totalListsTable.DeleteAsync(list);
            totalLists.Remove(list);
            await getLists();
        }
        #endregion

        #region Tasks
        public static async Task getTasks()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                MobileServiceCollection<TaskItem, TaskItem> wanted = await totalTasksTable.ToCollectionAsync();

                totalTasks.Clear();
                foreach (var task in wanted) totalTasks.Add(task);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }
            catch (HttpRequestException)
            {
                var msg = new MessageDialog("You need an Internet Connection to continue");
                await msg.ShowAsync();
            }
        }
        public static async Task addTask(TaskItem task)
        {
            await totalTasksTable.InsertAsync(task);
            await getTasks();
        }
        public static async Task editTask(TaskItem task)
        {
            await totalTasksTable.UpdateAsync(task);
            await getTasks();
        }
        public static async Task removeTask(TaskItem task)
        {
            await totalTasksTable.DeleteAsync(task);
            await getTasks();
        }
        #endregion
    }
}
