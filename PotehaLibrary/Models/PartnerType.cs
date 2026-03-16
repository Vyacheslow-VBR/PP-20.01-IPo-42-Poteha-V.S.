using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotehaLibrary.Models
{
    [Table("partner_types_poteha", Schema = "app")]
    public class PartnerType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}