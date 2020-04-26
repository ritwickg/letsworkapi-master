﻿// <auto-generated />
using System;
using LetsWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LetsWork.Infrastructure.Migrations
{
    [DbContext(typeof(LetsWorkDbContext))]
    [Migration("20181222205051_LetsWorkMigration")]
    partial class LetsWorkMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LetsWork.Domain.Models.ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.Booking", b =>
                {
                    b.Property<Guid>("BookingID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("BookingFromDate");

                    b.Property<string>("BookingStatus");

                    b.Property<DateTime>("BookingToDate");

                    b.Property<double>("Price");

                    b.Property<Guid>("UserID");

                    b.Property<Guid>("VenueID");

                    b.HasKey("BookingID");

                    b.HasIndex("UserID");

                    b.HasIndex("VenueID");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.InventoryDetail", b =>
                {
                    b.Property<Guid>("InventoryID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AirConditioningType");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500);

                    b.Property<double>("HourlyRate");

                    b.Property<bool>("IsCoffeeVendingMachineAvailable");

                    b.Property<bool>("IsFoodVendingMachineAvailable");

                    b.Property<bool>("IsWaterVendingMachineAvailable");

                    b.Property<int>("NumberOfMicroPhones");

                    b.Property<int>("NumberOfPhones");

                    b.Property<int>("NumberOfProjectors");

                    b.Property<string>("RoomType")
                        .IsRequired();

                    b.Property<int>("SeatCapacity");

                    b.Property<Guid>("VenueID");

                    b.Property<string>("WirelessNetworkType")
                        .IsRequired();

                    b.HasKey("InventoryID");

                    b.HasIndex("VenueID")
                        .IsUnique();

                    b.ToTable("InventoryDetails");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ProfileImage", b =>
                {
                    b.Property<Guid>("ProfileImageID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContainerName")
                        .IsRequired();

                    b.Property<string>("ProfileImageUrl")
                        .IsRequired();

                    b.Property<string>("ResourceName")
                        .IsRequired();

                    b.Property<Guid>("UserID");

                    b.HasKey("ProfileImageID");

                    b.HasIndex("UserID")
                        .IsUnique();

                    b.ToTable("ProfileImages");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ReferralCode", b =>
                {
                    b.Property<Guid>("ReferralCodeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("RefCode")
                        .IsRequired();

                    b.Property<int>("ReferralCodeTransactionCount");

                    b.Property<Guid>("UserId");

                    b.HasKey("ReferralCodeId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("ReferralCodes");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ReferralCodeTransaction", b =>
                {
                    b.Property<Guid>("ReferralTransactionId")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("BenificiaryId");

                    b.Property<Guid>("BookingId");

                    b.Property<Guid>("IssuerId");

                    b.Property<Guid>("ReferralCodeId");

                    b.HasKey("ReferralTransactionId");

                    b.HasIndex("BenificiaryId");

                    b.HasIndex("BookingId")
                        .IsUnique();

                    b.HasIndex("ReferralCodeId");

                    b.ToTable("ReferralCodeTransactions");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.VenueDetail", b =>
                {
                    b.Property<Guid>("VenueID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContactNumber");

                    b.Property<bool>("IsActive");

                    b.Property<string>("VenueCity")
                        .IsRequired();

                    b.Property<string>("VenueName")
                        .IsRequired();

                    b.Property<string>("VenueState")
                        .IsRequired();

                    b.HasKey("VenueID");

                    b.ToTable("VenueDetails");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.VenueImage", b =>
                {
                    b.Property<Guid>("VenueImageID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContainerName");

                    b.Property<string>("HostedImageURL");

                    b.Property<string>("ResourceName");

                    b.Property<Guid>("VenueID");

                    b.HasKey("VenueImageID");

                    b.HasIndex("VenueID");

                    b.ToTable("VenueImages");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<Guid>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<Guid>("UserId");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId");

                    b.Property<Guid>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("LetsWork.Domain.Models.Booking", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser", "ApplicationUser")
                        .WithMany("Bookings")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LetsWork.Domain.Models.VenueDetail", "VenueDetail")
                        .WithMany("BookingDetails")
                        .HasForeignKey("VenueID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LetsWork.Domain.Models.InventoryDetail", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.VenueDetail", "VenueDetails")
                        .WithOne("InventoryDetails")
                        .HasForeignKey("LetsWork.Domain.Models.InventoryDetail", "VenueID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ProfileImage", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser", "ApplicationUser")
                        .WithOne("ProfileImage")
                        .HasForeignKey("LetsWork.Domain.Models.ProfileImage", "UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ReferralCode", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser", "User")
                        .WithOne("ReferralCode")
                        .HasForeignKey("LetsWork.Domain.Models.ReferralCode", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LetsWork.Domain.Models.ReferralCodeTransaction", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser", "User")
                        .WithMany("ReferralCodeTransactions")
                        .HasForeignKey("BenificiaryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LetsWork.Domain.Models.Booking", "Booking")
                        .WithOne("ReferralCodeTransaction")
                        .HasForeignKey("LetsWork.Domain.Models.ReferralCodeTransaction", "BookingId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("LetsWork.Domain.Models.ReferralCode", "ReferralCode")
                        .WithMany("ReferralCodeTransactions")
                        .HasForeignKey("ReferralCodeId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("LetsWork.Domain.Models.VenueImage", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.VenueDetail")
                        .WithMany("VenueImages")
                        .HasForeignKey("VenueID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LetsWork.Domain.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("LetsWork.Domain.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}