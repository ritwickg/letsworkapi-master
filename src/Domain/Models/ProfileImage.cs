using System;
using System.Collections.Generic;
using System.Text;

namespace LetsWork.Domain.Models
{
   public class ProfileImage
   {
        public Guid ProfileImageID { get; set; }
        public string ContainerName { get; set; }
        public string ResourceName { get; set; }
        public string ProfileImageUrl { get; set; }
        public Guid UserID { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
   }
}
