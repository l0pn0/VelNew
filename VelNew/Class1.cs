using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VelNew
{
    internal class Class1
    {
        public static CompVeloEntities1 db = new CompVeloEntities1();
        public static Users LoggedUser;

        // Таблицы из базы данных
        public DbSet<Clients> Clients { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<EquipmentTypes> EquipmentTypes { get; set; }
        public DbSet<Equipments> Equipments { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<MaintenanceLogs> MaintenanceLogs { get; set; }
        public DbSet<Feedbacks> Feedbacks { get; set; }
        public DbSet<Reservations> Reservations { get; set; }
    }
}
