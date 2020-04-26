using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.DTO
{
    public class BookingSearchModelDTO
    {
        [Required]
        public string SearchDateTime;

        [Required]
        public string SearchLogic;
    }
}
