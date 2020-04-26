using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.DTO
{
    public class BookingSearchUserModelDTO
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Date { get; set; }
    }
}
