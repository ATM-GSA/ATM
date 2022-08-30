using Microsoft.EntityFrameworkCore;

namespace TABS.Data
{
    public class TABSDBContext : DbContext
    {
        public TABSDBContext(DbContextOptions options) : base(options) { }

        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationSubscription> ApplicationSubscriptions { get; set; }
        public DbSet<ApplicationIdentification> ApplicationIdentifications { get; set; }
        public DbSet<Architecture> Architectures { get; set; }
        public DbSet<Security> Securities { get; set; }
        public DbSet<FortifyScan> FortifyScans { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<BASMOnboarding> BASMOnboardings { get; set; }

        public DbSet<ServerEnvironment> ServerEnvironments { get; set; }
        public DbSet<Server> Servers { get; set; }

        public DbSet<DatabaseEnvironment> DatabaseEnvironments { get; set; }
        public DbSet<Database> Databases { get; set; }

        public DbSet<AuditLogging> AuditLogs { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ModuleType> ModuleTypes { get; set; }

        public DbSet<Dependency> Dependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>().ToTable("Applications");
            modelBuilder.Entity<Application>()
                .HasOne(a => a.CreateByUser)
                .WithMany()
                .HasForeignKey(a => a.CreateByUserID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationSubscription>().ToTable("ApplicationSubscriptions");
            modelBuilder.Entity<ApplicationSubscription>()
                .HasKey(appSub => new { appSub.ApplicationID, appSub.UserID });

            modelBuilder.Entity<ServerEnvironment>().ToTable("ServerEnvironments");
            modelBuilder.Entity<Server>().ToTable("Servers");
            modelBuilder.Entity<Server>()
                .Property(s => s.Type)
                .HasConversion<int>();
            modelBuilder.Entity<Server>()
                .HasOne(s => s.ServerEnvironment)
                .WithMany(s => s.Servers)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DatabaseEnvironment>().ToTable("DatabaseEnvironments");
            modelBuilder.Entity<Database>().ToTable("Databases");
            modelBuilder.Entity<Database>()
                .Property(d => d.Type)
                .HasConversion<int>();
            modelBuilder.Entity<Database>()
                .HasOne(d => d.DatabaseEnvironment)
                .WithMany(s => s.Databases)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dependency>().ToTable("Dependencies");
            modelBuilder.Entity<Dependency>()
                .HasKey(ad => new { ad.DependeeID, ad.DependentID });
            modelBuilder.Entity<Dependency>()
                .HasOne(d => d.Dependent)
                .WithMany(a => a.Dependees)
                .HasForeignKey(d => d.DependentID)
                .OnDelete(DeleteBehavior.Restrict); // Need this because it is a self-referencing relationship
            modelBuilder.Entity<Dependency>()
                .HasOne(d => d.Dependee)
                .WithMany(a => a.Dependents)
                .HasForeignKey(d => d.DependeeID)
                .OnDelete(DeleteBehavior.Restrict); // Need this because it is a self-referencing relationship

            modelBuilder.Entity<ApplicationIdentification>().ToTable("ApplicationIdentifications");

            modelBuilder.Entity<Architecture>().ToTable("Architectures");

            modelBuilder.Entity<Security>().ToTable("Securities");
            modelBuilder.Entity<FortifyScan>().ToTable("FortifyScans");
            modelBuilder.Entity<FortifyScan>()
                .Property(f => f.ScanType)
                .HasConversion<int>();
            modelBuilder.Entity<FortifyScan>()
                .HasOne(s => s.Security)
                .WithMany(s => s.FortifyScans)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contact>().ToTable("Contacts");

            modelBuilder.Entity<Report>().ToTable("Reports");

            modelBuilder.Entity<BASMOnboarding>().ToTable("BASMOnboardings");

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithOne(r => r.User)
                .HasForeignKey<Role>(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>().ToTable("Roles");

            modelBuilder.Entity<ModuleType>().ToTable("ModuleTypes");

            modelBuilder.Entity<AuditLogging>().HasKey(a => a.Id).IsClustered(true);
        }
    }
}
