﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using VismaBugBountySelfServicePortal.Database;

namespace VismaBugBountySelfServicePortal.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    // ReSharper disable once PartialTypeWithSinglePart
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VismaBugBountySelfServicePortal.Models.Entity.AssetEntity", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnName("AssetName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Columns")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsOnHackerOne")
                        .HasColumnType("bit");

                    b.Property<bool>("IsOnPublicProgram")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("bit");

                    b.HasKey("Key");

                    b.ToTable("Asset");
                });

            modelBuilder.Entity("VismaBugBountySelfServicePortal.Models.Entity.CredentialEntity", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnName("SetId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AssetName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("HackerName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Key", "AssetName");

                    b.ToTable("Credential");
                });

            modelBuilder.Entity("VismaBugBountySelfServicePortal.Models.Entity.CredentialValueEntity", b =>
                {
                    b.Property<string>("AssetName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Key")
                        .HasColumnName("SetId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("RowNumber")
                        .HasColumnType("int");

                    b.Property<string>("ColumnName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ColumnValue")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AssetName", "Key", "RowNumber", "ColumnName");

                    b.ToTable("CredentialValue");
                });

            modelBuilder.Entity("VismaBugBountySelfServicePortal.Models.Entity.UserEntity", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnName("Email")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Key");

                    b.ToTable("User");
                });

            modelBuilder.Entity("VismaBugBountySelfServicePortal.Models.Entity.UserSessionEntity", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnName("Email")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("LoginDateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Key");

                    b.ToTable("UserSession");
                });
#pragma warning restore 612, 618
        }
    }
}
