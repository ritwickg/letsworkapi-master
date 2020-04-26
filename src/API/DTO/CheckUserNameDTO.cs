using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace LetsWork.API.DTO
{
    public class CheckUserNameDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public Guid Id { get; set; }
    }
}
