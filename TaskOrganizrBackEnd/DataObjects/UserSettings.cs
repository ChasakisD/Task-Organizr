using Microsoft.Azure.Mobile.Server;

namespace TaskOrganizrBackEnd.DataObjects
{
    public class UserSettings:EntityData
    {
        public int themeId { get; set; }

        public bool NotifyTasksOneDayBefore { get; set; }

        public string UserId { get; set; }
    }
}