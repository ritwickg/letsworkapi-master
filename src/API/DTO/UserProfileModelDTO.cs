using System;

namespace LetsWork.API.DTO
{
    public class UserProfileModelDTO
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public Guid ProfileImageID { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
