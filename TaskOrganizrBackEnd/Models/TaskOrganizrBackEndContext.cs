using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using TaskOrganizrBackEnd.DataObjects;

namespace TaskOrganizrBackEnd.Models
{
    public class TaskOrganizrBackEndContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to alter your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        private const string connectionStringName = "Name=MS_TableConnectionString";

        public TaskOrganizrBackEndContext() : base(connectionStringName)
        {
        } 

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
            /*modelBuilder.Entity<Category>()
                        .HasRequired<User>(s => s.User)
                        .WithMany(s => s.Categories)
                        .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<List>()
                        .HasRequired<Category>(s => s.Category)
                        .WithMany(s => s.Lists)
                        .HasForeignKey(s => s.CategoryId);
            modelBuilder.Entity<TaskItem>()
                        .HasRequired<List>(s => s.List)
                        .WithMany(s => s.TaskItems)
                        .HasForeignKey(s => s.ListId);*/
        }

        public System.Data.Entity.DbSet<TaskOrganizrBackEnd.DataObjects.User> Users { get; set; }

        public System.Data.Entity.DbSet<TaskOrganizrBackEnd.DataObjects.Category> Categories { get; set; }

        public System.Data.Entity.DbSet<TaskOrganizrBackEnd.DataObjects.List> Lists { get; set; }

        public System.Data.Entity.DbSet<TaskOrganizrBackEnd.DataObjects.TaskItem> TaskItems { get; set; }

        public System.Data.Entity.DbSet<TaskOrganizrBackEnd.DataObjects.UserSettings> UserSettings { get; set; }
    }

}
