using System.ComponentModel.DataAnnotations;

namespace LetsWork.API.DTO
{
    public class SignInModelDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
