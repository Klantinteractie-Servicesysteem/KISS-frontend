using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Kiss.Bff.Beheer.Verwerking;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Kiss.Bff.ZaakGerichtWerken.Contactverzoeken;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Kiss.Bff.Beheer.Data
{
    public class BeheerDbContext : DbContext, IDataProtectionKeyContext
    {

        public BeheerDbContext(DbContextOptions<BeheerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bericht>(b =>
            {
                b.Property(x => x.Type).HasMaxLength(200).IsRequired();
                b.HasIndex(x => x.Type);
                b.Property(x => x.Inhoud).IsRequired();
                b.Property(x => x.Titel).IsRequired();
            });

            modelBuilder.Entity<BerichtGelezen>(g =>
            {
                g.HasKey(x => new { x.UserId, x.BerichtId });
            });

            modelBuilder.Entity<Gespreksresultaat>(r =>
            {
                r.HasIndex(x => x.Definitie).IsUnique();
            });

            modelBuilder.Entity<VerwerkingsLog>(r => r.Property(l => l.InsertedAt).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd());

            modelBuilder.Entity<ContactmomentDetails>(l =>
            {
                l.HasIndex(x => x.Vraag);
            });

            modelBuilder.Entity<ContactVerzoekVragenSet>()
                .Property(p => p.JsonVragen)
                .HasColumnType("json");
        } 

        public DbSet<Bericht> Berichten { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<BerichtGelezen> Gelezen { get; set; } = null!;
        public DbSet<Link> Links { get; set; } = null!;
        public DbSet<Gespreksresultaat> Gespreksresultaten { get; set; } = null!;
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<VerwerkingsLog> VerwerkingsLogs { get; set; } = null!;
        public DbSet<ContactmomentDetails> ContactMomentDetails { get; set; } = null!;
        public DbSet<ContactVerzoekVragenSet> ContactVerzoekVragenSets { get; set; } = null!;
    }
}
