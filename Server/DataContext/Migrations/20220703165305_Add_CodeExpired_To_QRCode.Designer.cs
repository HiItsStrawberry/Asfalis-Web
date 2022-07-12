﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using asfalis.Server.DataContext;

#nullable disable

namespace asfalis.Server.DataContext.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220703165305_Add_CodeExpired_To_QRCode")]
    partial class Add_CodeExpired_To_QRCode
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("asfalis.Shared.Models.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("image_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)")
                        .HasColumnName("name");

                    b.HasKey("ImageId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("asfalis.Shared.Models.QRCode", b =>
                {
                    b.Property<int>("CodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("code_id");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)")
                        .HasColumnName("code");

                    b.Property<int>("CodeExpired")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0)
                        .HasColumnName("code_expired");

                    b.Property<DateTime>("ExpiryTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("expiry_time");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("CodeId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("QRCodes");
                });

            modelBuilder.Entity("asfalis.Shared.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.Property<int>("AccessFailedTime")
                        .HasColumnType("int")
                        .HasColumnName("access_failed_time");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("email");

                    b.Property<bool>("EmailConfirmed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false)
                        .HasColumnName("email_confirmed");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("varchar(1)")
                        .HasColumnName("gender");

                    b.Property<DateTime?>("LockoutEnd")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("lockout_end");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("password");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("varchar(25)")
                        .HasColumnName("username");

                    b.HasKey("UserId");

                    b.HasIndex("Username", "Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("asfalis.Shared.Models.UserImage", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.Property<int>("ImageId")
                        .HasColumnType("int")
                        .HasColumnName("image_id");

                    b.HasKey("UserId", "ImageId");

                    b.HasIndex("ImageId");

                    b.ToTable("UserImage");
                });

            modelBuilder.Entity("asfalis.Shared.Models.QRCode", b =>
                {
                    b.HasOne("asfalis.Shared.Models.User", "User")
                        .WithOne("QRCode")
                        .HasForeignKey("asfalis.Shared.Models.QRCode", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("asfalis.Shared.Models.UserImage", b =>
                {
                    b.HasOne("asfalis.Shared.Models.Image", "Image")
                        .WithMany("UserImages")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("asfalis.Shared.Models.User", "User")
                        .WithMany("UserImages")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");

                    b.Navigation("User");
                });

            modelBuilder.Entity("asfalis.Shared.Models.Image", b =>
                {
                    b.Navigation("UserImages");
                });

            modelBuilder.Entity("asfalis.Shared.Models.User", b =>
                {
                    b.Navigation("QRCode");

                    b.Navigation("UserImages");
                });
#pragma warning restore 612, 618
        }
    }
}
