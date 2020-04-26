using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LetsWork.API.DTO
{
    public class OAuthDTO
    {
        public string ProviderName { get; set; }
        public string DisplayName { get; set; }
        public string TokenID { get; set; }
        public string ProviderKey { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
    }
}
