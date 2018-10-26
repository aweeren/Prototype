namespace Prototype.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vwSelectedProductsWide")]
    public partial class FoodItem
    {
        [Key]
        public string NDB_No { get; set; }

        public double Cholesterol { get; set; }

        public double Fiber { get; set; }

        public double Fat { get; set; }

        public double Protein { get; set; }

        public double Sugars { get; set; }

        public double Energy { get; set; }

        [Column(name:"long_name")]
        public string Name { get; set; }

        public int MaxServings { get; set; }
    }
}
