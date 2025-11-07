using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Applet.Nat.Api.DC
{
    public class NatContext : DbContext
    {
        public NatContext(DbContextOptions options) : base(options)
        {
        }
        public string ivstrCnn { get; set; }
        public static NatContext GetContext(IConfiguration mioConfiguration)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NatContext>();
            optionsBuilder.UseSqlServer(mioConfiguration.GetConnectionString("sqlserver"));
            return new NatContext(optionsBuilder.Options);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //PK compuestas
            modelBuilder.Entity<ListModel>().HasKey(x => new { x.ivcodType, x.ivcodId });
            modelBuilder.Entity<DocumentTrackingModel>().HasKey(x => new { x.ivlngDoc, x.ivnumTrack });
            modelBuilder.Entity<UserCuitModel>().HasKey(x => new { x.ivnumUser, x.ivlngCuit });
        }
        public DbSet<IdentityProviderModel> IdentityProviders { get; set; }
        public DbSet<CuitModel> Cuits { get; set; }
        public DbSet<DocumentModel> Documents { get; set; }
        public DbSet<DocumentTrackingModel> DocumentTrackings { get; set; }
        public DbSet<ListModel> Lists { get; set; }
        public DbSet<UserCuitModel> UserCuits { get; set; }
        public DbSet<UserModel> Users { get; set; }
    }
}