using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prototype.Models
{
    [Table("SelectedFoodNames")]
    public class DisplayName
    {
        [Key]
        public string NDB_Number { get; set; }

        public string Name { get; set; }

        public string NiceName { get; set; }
    }
}
