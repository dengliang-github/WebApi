using Microsoft.EntityFrameworkCore;

namespace DotNetCoreWebApi
{
    public class CoreDbContext : DbContext
    {
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseMySQL("server=81.68.138.171;port=3306;database=MYDB;uid=sa;pwd=123456;");
        // }
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {

        }
        public virtual DbSet<SYS_USER> SYS_USER { get; set; }
        public virtual DbSet<Attachments> Attachments { get; set; }
    }
}