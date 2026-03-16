using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotehaLibrary.Models
{
    [Table("products_poteha", Schema = "app")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Column("article")]
        [MaxLength(50)]
        public string Article { get; set; }

        [Column("price")]
        public decimal Price { get; set; }
    }
}