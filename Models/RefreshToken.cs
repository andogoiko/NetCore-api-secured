

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apiSecurizada.Models;

namespace apiSecurizada.Models{

    public class RefreshToken{

         //CAMPOS
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public required string Token { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime ExpireDate { get; set; }

        //PROPIEDADES DE NAVEGACION

            [System.Text.Json.Serialization.JsonIgnore]
            public User User { get; set; }
    }

}
