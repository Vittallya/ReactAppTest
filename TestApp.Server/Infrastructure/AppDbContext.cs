using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TestApp.Server.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

public class OrderSchema : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderCity).HasMaxLength(50);
        builder.Property(x => x.SenderAddress).HasMaxLength(150);
        builder.Property(x => x.RecieverCity).HasMaxLength(50);
        builder.Property(x => x.RecieverAddress).HasMaxLength(150);

        builder.HasData(new[]
        {
            new Order(1, "Moscow", "Kremlin", "SPb", "petrogradskaya naberezhnaya", 8
                , new DateTimeOffset(new DateTime(2009, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc))),
            new Order(2, "Kazan", "Kremlin", "SPb", "Vasilievskiy Island", 2
                , new DateTimeOffset(new DateTime(2020, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc))),
            new Order(3, "Krasnodar", "Vokzal", "Moscow", "City Moll", 15
                , new DateTimeOffset(new DateTime(2019, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc))),
            new Order(4, "Astana", "Vokzal", "Moscow", "City Moll", 12
                , new DateTimeOffset(new DateTime(2020, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc))),
            new Order(5, "Orel", "Vokzal", "Moscow", "City Moll", 12
                , new DateTimeOffset(new DateTime(2021, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc))),
        });
    }
}