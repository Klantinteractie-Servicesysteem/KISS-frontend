﻿// <auto-generated />
using System;
using Kiss.Bff.NieuwsEnWerkinstructies.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    [DbContext(typeof(NieuwsEnWerkinstructiesDbContext))]
    [Migration("20230508072508_berichtenSkillManyToMany")]
    partial class berichtenSkillManyToMany
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BerichtSkill", b =>
                {
                    b.Property<int>("BerichtenId")
                        .HasColumnType("integer");

                    b.Property<int>("SkillsId")
                        .HasColumnType("integer");

                    b.HasKey("BerichtenId", "SkillsId");

                    b.HasIndex("SkillsId");

                    b.ToTable("BerichtSkill");
                });

            modelBuilder.Entity("Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities.Bericht", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Inhoud")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsBelangrijk")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("PublicatieDatum")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Titel")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Berichten");
                });

            modelBuilder.Entity("Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities.Skill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("DateUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Naam")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Skills");
                });

            modelBuilder.Entity("BerichtSkill", b =>
                {
                    b.HasOne("Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities.Bericht", null)
                        .WithMany()
                        .HasForeignKey("BerichtenId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities.Skill", null)
                        .WithMany()
                        .HasForeignKey("SkillsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
