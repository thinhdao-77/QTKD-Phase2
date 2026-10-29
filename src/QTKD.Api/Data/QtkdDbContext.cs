using Microsoft.EntityFrameworkCore;
using QTKD.Api.Entities;

namespace QTKD.Api.Data;

public class QtkdDbContext : DbContext
{
    public QtkdDbContext(DbContextOptions<QtkdDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceItem> PriceItems => Set<PriceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Bảng customers
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.CustomerCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(c => c.CustomerCode).IsUnique();

            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(c => c.TaxCode)
                  .HasMaxLength(50);

            entity.Property(c => c.Address)
                  .HasMaxLength(300);

            entity.Property(c => c.ContactEmail)
                  .HasMaxLength(150);

            entity.Property(c => c.Status)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(c => c.CreatedAt)
                  .HasDefaultValueSql("now()");
        });

        // 2. Bảng contracts và quan hệ Contract – Customer (N - 1)
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("contracts");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.ContractNo)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(c => c.ContractNo).IsUnique();

            entity.Property(c => c.StartDate).IsRequired();
            entity.Property(c => c.EndDate).IsRequired();

            entity.Property(c => c.ServiceGroup)
                  .HasMaxLength(100);

            entity.Property(c => c.ContentSummary)
                  .HasMaxLength(1000);

            entity.Property(c => c.AttachmentUrl)
                  .HasMaxLength(500);

            entity.Property(c => c.Status)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(c => c.CreatedAt)
                  .HasDefaultValueSql("now()");

            entity.HasOne(c => c.Customer)
                  .WithMany(cu => cu.Contracts)
                  .HasForeignKey(c => c.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Bảng price_lists
        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.ToTable("price_lists");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(p => p.EffectiveDate).IsRequired();

            entity.Property(p => p.Status)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(p => p.CreatedAt)
                  .HasDefaultValueSql("now()");
        });

        // 4. Bảng price_items và quan hệ PriceList – PriceItem (1 - N)
        modelBuilder.Entity<PriceItem>(entity =>
        {
            entity.ToTable("price_items");

            entity.HasKey(pi => pi.Id);

            entity.Property(pi => pi.ServiceCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(pi => pi.ServiceName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(pi => pi.Unit)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(pi => pi.UnitPrice)
                  .HasPrecision(18, 2)
                  .IsRequired();

            entity.HasIndex(pi => new { pi.PriceListId, pi.ServiceCode }).IsUnique();

            entity.HasOne(pi => pi.PriceList)
                  .WithMany(pl => pl.Items)
                  .HasForeignKey(pi => pi.PriceListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Dữ liệu mẫu (Seed Data) cho Tuần 05:
        //    - Ít nhất 2 Customer (trong đó có Công ty Minh Phát theo kịch bản xuyên suốt)
        //    - 2 Contract ở trạng thái Draft (trong đó có HD-2026-018 của Công ty Minh Phát)
        //    - 1 PriceList có 2 PriceItem
        var seedTime = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                CustomerCode = "KH-001",
                Name = "Công ty TNHH Logistics Minh Phát",
                TaxCode = "0312345678",
                Address = "123 Nguyễn Văn Linh, Quận 7, TP.HCM",
                ContactEmail = "contact@minhphatlogistics.vn",
                Status = CustomerStatus.Active,
                CreatedAt = seedTime
            },
            new Customer
            {
                Id = 2,
                CustomerCode = "KH-002",
                Name = "Công ty Cổ phần Vận tải Biển Đông",
                TaxCode = "0398765432",
                Address = "45 Lê Thánh Tôn, Quận 1, TP.HCM",
                ContactEmail = "info@biendongshipping.vn",
                Status = CustomerStatus.Active,
                CreatedAt = seedTime
            }
        );

        modelBuilder.Entity<Contract>().HasData(
            new Contract
            {
                Id = 1,
                ContractNo = "HD-2026-018",
                CustomerId = 1,
                StartDate = new DateOnly(2026, 10, 1),
                EndDate = new DateOnly(2027, 9, 30),
                ServiceGroup = "Dịch vụ cảng",
                ContentSummary = "Hợp đồng cung cấp dịch vụ bốc xếp và lưu kho bãi container tại cảng Cát Lái",
                AttachmentUrl = "https://storage.qtkd.local/contracts/HD-2026-018.pdf",
                Status = ContractStatus.Draft,
                SubmittedAt = null,
                CreatedAt = seedTime
            },
            new Contract
            {
                Id = 2,
                ContractNo = "HD-2026-019",
                CustomerId = 2,
                StartDate = new DateOnly(2026, 11, 1),
                EndDate = new DateOnly(2027, 10, 31),
                ServiceGroup = "Vận chuyển nội địa",
                ContentSummary = null,
                AttachmentUrl = null,
                Status = ContractStatus.Draft,
                SubmittedAt = null,
                CreatedAt = seedTime
            }
        );

        modelBuilder.Entity<PriceList>().HasData(
            new PriceList
            {
                Id = 1,
                Name = "Bảng giá dịch vụ khai thác cảng tiêu chuẩn 2026",
                EffectiveDate = new DateOnly(2026, 1, 1),
                ExpiryDate = new DateOnly(2026, 12, 31),
                Status = PriceListStatus.Effective,
                CreatedAt = seedTime
            }
        );

        modelBuilder.Entity<PriceItem>().HasData(
            new PriceItem
            {
                Id = 1,
                PriceListId = 1,
                ServiceCode = "SVC-CONT-20",
                ServiceName = "Bốc xếp container 20 feet",
                Unit = "Container",
                UnitPrice = 1250000m
            },
            new PriceItem
            {
                Id = 2,
                PriceListId = 1,
                ServiceCode = "SVC-STORAGE-20",
                ServiceName = "Lưu kho bãi container 20 feet",
                Unit = "Ngày",
                UnitPrice = 180000m
            }
        );
    }
}

