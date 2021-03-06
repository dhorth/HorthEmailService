// <auto-generated />
using System;
using Horth.Service.Email.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Horth.Service.Email.Migrations
{
    [DbContext(typeof(EmailServiceDbContext))]
    [Migration("20210605120925_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.5");

            modelBuilder.Entity("Horth.Service.Email.Model.EmailStat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Client")
                        .HasColumnType("TEXT");

                    b.Property<int>("FailedExternal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FailedInternal")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<int>("RetriesExternal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RetriesInternal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SentExternal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SentInternal")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StatDay")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("EmailStat");
                });
#pragma warning restore 612, 618
        }
    }
}
