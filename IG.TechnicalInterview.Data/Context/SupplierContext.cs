using Microsoft.EntityFrameworkCore;
using IG.TechnicalInterview.Model.Supplier;

namespace IG.TechnicalInterview.Data.Context
{
    public class SupplierContext : DbContext
    {
        public SupplierContext (DbContextOptions<SupplierContext> options)
            : base(options)
        {
        }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Email> Emails { get; set; }

        public DbSet<Phone> Phones { get; set; }
    }
}
