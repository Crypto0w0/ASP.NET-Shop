using System.ComponentModel.DataAnnotations;

namespace ASP.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Identifier { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RestoreDate { get; set; }
    }

}
