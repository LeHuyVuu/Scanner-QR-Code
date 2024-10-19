using BT3_Day2.Entities;
using Microsoft.EntityFrameworkCore;

public class QRContext : DbContext
{
    public DbSet<QRCodeScan> QRCodeScans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Sử dụng kết nối từ appsettings.json hoặc chuỗi kết nối trực tiếp
        optionsBuilder.UseSqlServer("Server=VU\\HUYVU;Database=ScanQRCode;User Id=sa;Password=12345;TrustServerCertificate=True;");
    }

    DbSet<QRCodeScan> qrCodesScan;
}
