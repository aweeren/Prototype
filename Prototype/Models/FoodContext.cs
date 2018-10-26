namespace Prototype.Models
{
    using System.Data.Entity;

    public partial class FoodContext : DbContext
    {
        public FoodContext()
            : base("name=FoodContext")
        {
        }

        public virtual DbSet<FoodItem> FoodItems { get; set; }
        public virtual DbSet<DisplayName> DisplayNames { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
