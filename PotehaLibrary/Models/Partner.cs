using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PotehaLibrary.Models
{
    [Table("partners_poteha", Schema = "app")]
    public class Partner
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Column("director_fullname")]
        [MaxLength(200)]
        public string DirectorFullname { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string Email { get; set; }

        // ДОБАВЛЯЕМ ПОЛЕ АДРЕС
        [Column("address")]
        public string Address { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("discount")]
        public int Discount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual PartnerType PartnerType { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}