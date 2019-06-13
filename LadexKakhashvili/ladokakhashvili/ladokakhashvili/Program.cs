using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadoKakhashvili
{
    public class SphoneContext : DbContext
    {
        public SphoneContext() : base("SphoneContext")
        {

        }
        public DbSet<Sphone> Sphones { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<OS> OSs { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sphone>().HasKey(s => s.Id).HasRequired(sp => sp.Manufacturer).WithMany(m => m.Sphones);
            modelBuilder.Entity<Manufacturer>().HasKey(m => m.Id);
            modelBuilder.Entity<OS>().HasKey(os => os.Id).HasMany(os => os.Sphones).WithMany(sp => sp.OS);

        }
    }

    public class Manufacturer
    {
        [Key]
        public int Id { get; set; }
        public string ManufacturerName { get; set; }
        public virtual ICollection<Sphone> Sphones { get; set; }
    }

    public class Sphone
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public DateTime ReleaseDate { get; set; }
        public virtual ICollection<OS> OS { get; set; }
    }

    public class OS
    {
        public int Id { get; set; }
        public string OSName { get; set; }
        public virtual ICollection<Sphone> Sphones { get; set; }
    }
    public class Solutions
    {
        public static List<Sphone> GetPhones()
        {
            using (var context = new SphoneContext())
            {
                var q = context.Set<Sphone>().Include(s => s.Manufacturer).Include(x => x.OS).ToList();

                return q;
            }
        }

        public static List<Manufacturer> GetManifacturers()
        {
            using (var context = new SphoneContext())
            {
                var q = context.Set<Manufacturer>().ToList();

                return q;

            }
        }

        public static List<OS> GetOss()
        {
            using (var context = new SphoneContext())
            {
                var q = context.Set<OS>().ToList();

                return q;

            }
        }
        
        public static bool AddPhone(int manufacturerId, int[] osIdList, string phoneName, DateTime phoneReleaseDate)
        {
            if (manufacturerId == 0 || osIdList.Count() == 0 || string.IsNullOrEmpty(phoneName) || phoneReleaseDate == null)
            {
                return false;
            }

            using (var context = new SphoneContext())
            {
                var osList = context.Set<OS>().Where(x => osIdList.Contains(x.Id)).ToList();

                var newPhone = new Sphone()
                {
                    Name = phoneName,
                    ReleaseDate = phoneReleaseDate,
                    ManufacturerId = manufacturerId,
                    OS = osList
                };

                context.Entry(newPhone).State = EntityState.Added;

                context.SaveChanges();

                return true;
            }
        }

        public static bool UpdatePhone(int id, string name, DateTime date)
        {
            if (id == 0 || string.IsNullOrEmpty(name) || date == null)
            {
                return false;
            }

            using (var context = new SphoneContext())
            {
                var phone = context.Set<Sphone>().FirstOrDefault(p => p.Id == id);

                phone.Name = name;
                phone.ReleaseDate = date;

                context.Entry(phone).State = EntityState.Modified;

                context.SaveChanges();

                return true;
            }
        }

        public static bool RemovePhone(int id)
        {
            if (id == 0)
            {
                return false;
            }

            using (var context = new SphoneContext())
            {
                var phone = context.Set<Sphone>().FirstOrDefault(p => p.Id == id);

                if (phone == null)
                {
                    return false;
                }

                context.Entry(phone).State = EntityState.Deleted;

                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            var cycle = true;

            while (cycle)
            {
                Console.WriteLine("\nenter a character: ");
                var x = Console.ReadKey().KeyChar;

                switch (x)
                {
                    case 'R':
                        ConsoleRead();

                        break;
                    case 'D':
                        ConsoleRemove();

                        Console.WriteLine();
                        break;
                    case 'C':
                        ConsoleCreate();

                        Console.WriteLine();
                        break;
                    case 'U':
                        ConsoleUpdate();

                        Console.WriteLine();
                        break;
                    case 'E':
                        cycle = false;
                        
                        break;
                }

            }

            
           
        }

        private static void ConsoleRead()
        {
            //Solutions solutions = new Solutions();
            var phones = Solutions.GetPhones();

            foreach (var item in phones)
            {
                Console.WriteLine("phone ID: " + item.Id);
                Console.WriteLine("phone name: " + item.Name);
                Console.WriteLine("phone manifacturer: " + item.Manufacturer?.ManufacturerName ?? "");
                Console.WriteLine("phone OS: " + (item.OS.Count != 0 ? item.OS.FirstOrDefault().OSName : ""));
                if (item.OS.Count != 0)
                {
                    Console.WriteLine("List of phone OS:");
                    foreach (var i in item.OS)
                    {
                        Console.WriteLine("\tos: " + i.OSName);
                    }
                }
                else
                {
                    Console.WriteLine("This phone has not configured os yet.");
                }
                Console.WriteLine("--------------------------");
            }
        }

        private static void ConsoleRemove()
        {
            Console.WriteLine("\nEnter the phone ID you want to delete: ");
            var num = Console.ReadLine();

            var result = Solutions.RemovePhone(int.Parse(num));

            if (result == false)
            {
                Console.WriteLine($"Couldn't delete phone by ID {num}.");
            }
            else
            {
                Console.WriteLine($"Phone successfully deleted.");
            }
        }

        private static void ConsoleCreate()
        {
            //-------------------------------------------------------------- Manufacturer ---------------\\

            Console.WriteLine("\nList of available manifacturers:");
            var manfsList = Solutions.GetManifacturers();

            foreach (var item in manfsList)
            {
                Console.WriteLine($"\t {item.Id} - {item.ManufacturerName}");
            }

            Console.WriteLine("Select one:");
            var manufacturerId = int.Parse(Console.ReadLine());

            //-------------------------------------------------------------- OS -------------------------\\

            Console.WriteLine("\n\nList of available OSs: ");
            var osList = Solutions.GetOss();

            foreach (var item in osList)
            {
                Console.WriteLine($"\t {item.Id} - {item.OSName}");
            }

            Console.WriteLine("\n\nSelect which ones do you want (separated by commas): ");
            var osIdList = Array.ConvertAll(Console.ReadLine().Split(','), int.Parse);


            Console.WriteLine("\nEnter the name of a new phone: ");
            var phoneName = Console.ReadLine();

            Console.WriteLine("\nEnter the phone release date (yyyy-mm-dd):");
            var phoneReleaseDate = DateTime.Parse(Console.ReadLine());

            var res = Solutions.AddPhone(manufacturerId, osIdList, phoneName, phoneReleaseDate);

            if (res == false)
            {
                Console.WriteLine("\nAdding a phone failed.");
            }
            else
            {
                Console.WriteLine("\nPhone successfully added!");
            }
        }

        private static void ConsoleUpdate()
        {
            Console.WriteLine("\n\nWhich phone do you want to edit?");
            var currentPhones = Solutions.GetPhones();

            Console.WriteLine("\nEnter ID: ");
            var phoneId = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter a new name for the phone: ");
            var newPhoneName = Console.ReadLine();

            Console.WriteLine("Change the release date (yyyy-mm-dd): ");
            var newPhoneReleaseDate = DateTime.Parse(Console.ReadLine());

            var r = Solutions.UpdatePhone(phoneId, newPhoneName, newPhoneReleaseDate);

            if (r == false)
            {
                Console.WriteLine("\nUpdating phone failed.");
            }
            else
            {
                Console.WriteLine("\nPhone successfully updated!");
            }
        }
    }
}
