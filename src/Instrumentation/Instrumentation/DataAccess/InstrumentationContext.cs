using Instrumentation.Model;
using Microsoft.EntityFrameworkCore;

namespace Instrumentation.DataAccess;

public class InstrumentationContext : DbContext
{
    public InstrumentationContext(DbContextOptions<InstrumentationContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<InstrumentCategory> Categories => Set<InstrumentCategory>();
    public DbSet<Instument> Instuments => Set<Instument>();
    public DbSet<Subinstument> Subinstuments => Set<Subinstument>();
    public DbSet<Sound> Sounds => Set<Sound>();
    public DbSet<Subsound> Subsounds => Set<Subsound>();
    public DbSet<SoundData> SoundsDatas => Set<SoundData>();
    public DbSet<SoundNoteImage> NoteImages => Set<SoundNoteImage>();
    public DbSet<InstrumentImage> InstrumentImages => Set<InstrumentImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstrumentCategory>(e =>
        {
            e.ToTable(nameof(Categories));
            
            e.HasKey(x => x.Id);
            
            e.HasMany(x => x.Instuments)
                .WithOne(x => x.Category)
                .IsRequired();
        });

        modelBuilder.Entity<Instument>(e =>
        {
            e.ToTable(nameof(Instuments));

            e.HasKey(x => x.Id);

            e.HasOne(x => x.Category)
                .WithMany(x => x.Instuments)
                .IsRequired();

            e.HasMany(x => x.Subinstuments)
                .WithOne(x => x.Instument)
                .IsRequired(false);

            e.HasMany(x => x.Sounds)
                .WithOne(x => x.Instument)
                .IsRequired(false);

            e.HasMany(x => x.Images)
                .WithOne(x => x.Instument)
                .IsRequired(false);

            e.Ignore(x => x.IsInstumentGroup);
        });

        modelBuilder.Entity<Subinstument>(e =>
        {
            e.ToTable(nameof(Subinstuments));

            e.HasKey(x => x.Id);

            e.HasMany(x => x.Sounds)
                .WithOne(x => x.Subinstument)
                .IsRequired(false);

            e.HasOne(x => x.Instument)
                .WithMany(x => x.Subinstuments);

            e.HasMany(x => x.Images)
                .WithOne(x => x.Subinstument)
                .IsRequired(false);
        });

        modelBuilder.Entity<Sound>(e =>
        {
            e.ToTable(nameof(Sounds));

            e.HasKey(x => x.Id);

            e.HasOne(x => x.SoundData)
                .WithOne(x => x.Sound)
                .IsRequired(false);

            e.HasMany(x => x.Subsounds)
                .WithOne(x => x.Sound)
                .IsRequired(false);

            e.HasOne(x => x.Instument)
                .WithMany(x => x.Sounds)
                .IsRequired(false);

            e.HasOne(x => x.Subinstument)
                .WithMany(x => x.Sounds)
                .IsRequired(false);

            e.HasMany(x => x.SoundNoteImages)
                .WithOne(x => x.Sound)
                .IsRequired(false);

            e.Ignore(x => x.IsSoundGroup);
        });

        modelBuilder.Entity<Subsound>(e =>
        {
            e.ToTable(nameof(Subsounds));

            e.HasKey(x => x.Id);

            e.HasOne(x => x.SoundData)
                .WithOne(x => x.Subsound)
                .IsRequired(false);

            e.HasOne(x => x.Sound)
                .WithMany(x => x.Subsounds)
                .IsRequired(false);

            e.HasMany(x => x.SoundNoteImages)
                .WithOne(x => x.Subsound)
                .IsRequired(false);
        });

        modelBuilder.Entity<SoundData>(e =>
        {
            e.ToTable(nameof(SoundsDatas));

            e.HasKey(x => x.Id);

            e.HasOne(x => x.Sound)
                .WithOne(x => x.SoundData)
                .IsRequired(false);

            e.HasOne(x => x.Subsound)
                .WithOne(x => x.SoundData)
                .IsRequired(false);

            e.Ignore(x => x.NoteImages);
        });

        modelBuilder.Entity<SoundNoteImage>(e =>
        {
            e.ToTable(nameof(NoteImages));

            e.HasKey(x => x.Id);

            e.HasOne(x => x.Sound)
                .WithMany(x => x.SoundNoteImages)
                .IsRequired(false);

            e.HasOne(x => x.Subsound)
                .WithMany(x => x.SoundNoteImages)
                .IsRequired(false);
        });

        modelBuilder.Entity<InstrumentImage>(e =>
        {
            e.ToTable("InstrumentImages");

            e.HasKey(x => x.Id);

            e.HasOne(x => x.Instument)
                .WithMany(x => x.Images)
                .IsRequired(false);

            e.HasOne(x => x.Subinstument)
                .WithMany(x => x.Images)
                .IsRequired(false);
        });
    }
}