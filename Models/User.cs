using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using apiSecurizada.Models.DTO;
using apiSecurizada.Models;
using Microsoft.EntityFrameworkCore;

namespace apiSecurizada.Models{

    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User{
    
    //CAMPOS
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        //PROPIEDADES DE NAVEGACION

        [System.Text.Json.Serialization.JsonIgnore]
        public RefreshToken RefreshToken { get; set; }


    }
}
