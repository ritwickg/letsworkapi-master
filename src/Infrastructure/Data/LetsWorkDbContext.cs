using LetsWork.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace LetsWork.Infrastructure.Data
{
    public class LetsWorkDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly ConfigSettings _applicationConfiguration;
        public LetsWorkDbContext(IOptions<ConfigSettings> ApplicationConfigurationOptions, DbContextOptions<LetsWorkDbContext> Options) : base(Options)
        {
            _applicationConfiguration = ApplicationConfigurationOptions.Value;
        }

        #region DbSet(s)
        public DbSet<VenueDetail> VenueDetails { get; set; }
        public DbSet<InventoryDetail> InventoryDetails { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ProfileImage> ProfileImages { get; set; }
        public DbSet<VenueImage> VenueImages { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }
        public DbSet<ReferralCodeTransaction> ReferralCodeTransactions { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder ModelBuilder)
        {
            //Venue Details Properties
            ModelBuilder.Entity<VenueDetail>().HasKey(x => x.VenueID);
            ModelBuilder.Entity<VenueDetail>().Property<string>(x => x.VenueName).IsRequired();
            ModelBuilder.Entity<VenueDetail>().Property<string>(x => x.VenueCity).IsRequired();
            ModelBuilder.Entity<VenueDetail>().Property<string>(x => x.VenueState).IsRequired();
            ModelBuilder.Entity<VenueDetail>().Property<bool>(x => x.IsActive).IsRequired();
            ModelBuilder.Entity<VenueDetail>().HasOne<InventoryDetail>(venue => venue.InventoryDetails).WithOne(x => x.VenueDetails).HasForeignKey<InventoryDetail>(iDetails => iDetails.VenueID);

            //Inventory Details Properties
            ModelBuilder.Entity<InventoryDetail>().HasKey(x => x.InventoryID);
            ModelBuilder.Entity<InventoryDetail>().Property<int>(x => x.NumberOfProjectors).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<int>(x => x.NumberOfPhones).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<int>(x => x.NumberOfMicroPhones).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<string>(x => x.Description).HasMaxLength(500).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<string>(x => x.RoomType).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<string>(x => x.WirelessNetworkType).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<bool>(x => x.IsWaterVendingMachineAvailable).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<bool>(x => x.IsFoodVendingMachineAvailable).IsRequired();
            ModelBuilder.Entity<InventoryDetail>().Property<bool>(x => x.IsCoffeeVendingMachineAvailable).IsRequired();

            //Booking properties
            ModelBuilder.Entity<Booking>().HasKey(x => x.BookingID);
            ModelBuilder.Entity<Booking>().Property<DateTime>(x => x.BookingFromDate).IsRequired();
            ModelBuilder.Entity<Booking>().Property<DateTime>(x => x.BookingToDate).IsRequired();
            ModelBuilder.Entity<Booking>().HasOne<ApplicationUser>(x => x.ApplicationUser).WithMany(x => x.Bookings).HasForeignKey(z => z.UserID);
            ModelBuilder.Entity<Booking>().HasOne(x => x.VenueDetail).WithMany(y => y.BookingDetails).HasForeignKey(z => z.VenueID);


            //Profile image properties
            ModelBuilder.Entity<ProfileImage>().HasKey(x => x.ProfileImageID);
            ModelBuilder.Entity<ProfileImage>().Property<string>(x => x.ResourceName).IsRequired();
            ModelBuilder.Entity<ProfileImage>().Property<string>(x => x.ContainerName).IsRequired();
            ModelBuilder.Entity<ProfileImage>().Property<string>(x => x.ProfileImageUrl).IsRequired();
            ModelBuilder.Entity<ProfileImage>().Property<Guid>(x => x.UserID).IsRequired();
            ModelBuilder.Entity<ProfileImage>().HasOne<ApplicationUser>(x => x.ApplicationUser).WithOne(x => x.ProfileImage).HasForeignKey<ProfileImage>(x => x.UserID);

            //ReferralCode table properties
            ModelBuilder.Entity<ReferralCode>().HasKey(x => x.ReferralCodeId);
            ModelBuilder.Entity<ReferralCode>().Property(x => x.UserId).IsRequired();
            ModelBuilder.Entity<ReferralCode>().Property(x => x.RefCode).IsRequired();
            ModelBuilder.Entity<ReferralCode>().HasOne(x => x.User).WithOne(x => x.ReferralCode).HasForeignKey<ReferralCode>(x => x.UserId);

            //ReferralCode transactions properties
            ModelBuilder.Entity<ReferralCodeTransaction>().HasKey(x => x.ReferralTransactionId);
            ModelBuilder.Entity<ReferralCodeTransaction>().Property(x => x.ReferralCodeId).IsRequired();
            ModelBuilder.Entity<ReferralCodeTransaction>().Property(x => x.IssuerId).IsRequired();
            ModelBuilder.Entity<ReferralCodeTransaction>().Property(x => x.BenificiaryId).IsRequired();
            ModelBuilder.Entity<ReferralCodeTransaction>().HasOne<ApplicationUser>(x => x.User).WithMany(x => x.ReferralCodeTransactions).HasForeignKey(x => x.IssuerId);
            ModelBuilder.Entity<ReferralCodeTransaction>().HasOne<ApplicationUser>(x => x.User).WithMany(x => x.ReferralCodeTransactions).HasForeignKey(x => x.BenificiaryId);
            ModelBuilder.Entity<ReferralCodeTransaction>().HasOne<ReferralCode>(x => x.ReferralCode).WithMany(x => x.ReferralCodeTransactions).HasForeignKey(x => x.ReferralCodeId).OnDelete(DeleteBehavior.Restrict);
            ModelBuilder.Entity<ReferralCodeTransaction>().HasOne<Booking>(x => x.Booking).WithOne(x => x.ReferralCodeTransaction).HasForeignKey<ReferralCodeTransaction>(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(ModelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_applicationConfiguration.SQLProvider.ConnectionString, options =>
            {
                options.MigrationsHistoryTable("_LetsWorkMigrationHistory", "LetsWork");
            });
        }
    }
}
