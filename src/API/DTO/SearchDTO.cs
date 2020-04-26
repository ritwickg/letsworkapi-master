using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace LetsWork.API.DTO
{
    public class SearchDTO
    {
        [Required]
        public string FromDate;
        [Required]
        public string ToDate;
        [Required]
        public string City;
    }
}
