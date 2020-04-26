using Ardalis.GuardClauses;
using LetsWork.API.DTO;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using LetsWork.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LetsWork.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ConfigSettings _configSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailManager;
        private readonly IBlobService _blobManager;
        private readonly IProfileService _profileManager;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly IVenueService _venueService;
        private readonly IBookingService _bookingService;
        public ProfileController(IOptions<ConfigSettings> ConfigurationSettings,  
                                         SignInManager<ApplicationUser> SignInManager,
                                         UserManager<ApplicationUser> UserManager, 
                                         IEmailService EmailService,
                                         IProfileService ProfileService,
                                         IBlobService BlobService,
                                         ITokenProviderService TokenProviderService,
                                         IVenueService VenueService,
                                         IBookingService BookingService)
        {
            _configSettings = ConfigurationSettings.Value;
            _userManager = UserManager;
            _signInManager = SignInManager;
            _emailManager = EmailService;
            _blobManager = BlobService;
            _profileManager = ProfileService;
            _tokenProviderService = TokenProviderService;
            _bookingService = BookingService;
            _venueService = VenueService;
        }

        
        /// <summary>
        /// Provides the list of all admins
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllAdminDataAsync()
        {   
            IList<ApplicationUser> adminRolesList = await _userManager.GetUsersInRoleAsync(UserType.Admin.ToString());
            List<AdminModelDTO> adminList = adminRolesList.Select(x => new AdminModelDTO
            {
                UserName = x.UserName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber
            }).ToList();
            return Ok(adminList);
        }
  
        /// <summary>
        /// Provides the user details based on user id provided
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpGet("user/{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserDetailsByIDAsync(Guid id)
        {
            ApplicationUser foundUser = await _userManager.Users
                                                            .Include(x => x.ProfileImage)
                                                            .FirstOrDefaultAsync(x => x.Id == id);
            if (foundUser == null)
                return NotFound(new { message = $"No user found with ID: {id}." });

            UserProfileModelDTO userProfile = new UserProfileModelDTO
            {
                UserName = foundUser.UserName,
                FirstName = foundUser.FirstName,
                LastName = foundUser.LastName,
                PhoneNumber = foundUser.PhoneNumber,
                EmailConfirmed = foundUser.EmailConfirmed
            };

            if (foundUser.ProfileImage != null)
            {
                userProfile.ProfileImageID = foundUser.ProfileImage.ProfileImageID;
                userProfile.ProfileImageUrl = foundUser.ProfileImage.ProfileImageUrl;
            }

            return Ok(userProfile);
        }

        /// <summary>
        /// Updates the passowrd of the user with the new password provided
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="ChangePasswordModel">ChangePasswordModel</param>
        /// <returns></returns>
        [HttpPost("change-password")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePasswordAsync(string token, [FromBody] ChangePasswordDTO ChangePasswordModel)
        {
            Guard.Against.NullString(token);
            Guard.Against.NullItem<ChangePasswordDTO>(ChangePasswordModel);

            //Decode the token
            JwtSecurityToken decodedToken = _tokenProviderService.DecodeJWTAsync(token);

            if (decodedToken == null)
                return BadRequest(new { message = "Invalid token, please resend the change password request again to continue!" });

            string email = decodedToken.Claims.First(claims => claims.Type == "sub").Value;

            ApplicationUser applicationUser = await _userManager.FindByEmailAsync(email);
            if (applicationUser == null)
                return NotFound(new { message = "No user found for this email." });

            //Checking if the user can change password
            IList<UserLoginInfo> userLogins = await _userManager.GetLoginsAsync(applicationUser);
            if (userLogins.Count > 0)
                return StatusCode(401, new { message = "External accounts not authorized to change password in Visneto, please change password with your login-provider." });

            IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(applicationUser, ChangePasswordModel.OldPassword, ChangePasswordModel.NewPassword);

            if (!changePasswordResult.Succeeded)
                return BadRequest(new { message = changePasswordResult.GetIdentityResultErrorMessage() });

            return Ok(new
            {
                message = "Password Changed Successfully!!",
                email
            });
        }

        /// <summary>
        /// Resets the password of the user and sets it to the new password that is provided
        /// </summary>
        /// <param name="ForgotPasswordDTO">ForgotPasswordDTO</param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDTO ForgotPasswordDTO)
        {

            Guard.Against.NullItem(ForgotPasswordDTO);

            ApplicationUser applicationUser = await _userManager.FindByNameAsync(ForgotPasswordDTO.UserName);
            if (applicationUser == null)
                return NotFound(new { message = "No user found with such email, provide a valid email" });

            IList<UserLoginInfo> externalUserList = await _userManager.GetLoginsAsync(applicationUser);
            if (externalUserList.Count > 0)
                return StatusCode(401, new { message = "External users not authorized to change password, change password with the login-provider" });

            await _userManager.RemovePasswordAsync(applicationUser);
            IdentityResult changePasswordResult = await _userManager.AddPasswordAsync(applicationUser, ForgotPasswordDTO.NewPassword);

            if (!changePasswordResult.Succeeded)
                return BadRequest(new { message = changePasswordResult.GetIdentityResultErrorMessage() });

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Uploads the profile image of the user registered with Visneto
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpPut("profile-image/{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddProfileImageAsync(Guid id)
        {
            IFormFileCollection file = Request.Form.Files;

            if (file.Count == 0)
                return BadRequest(new { message = $"Please upload at least 1 image file as profile picture to continue." });

            if (file.Count > 1)
                return BadRequest(new { message = "Only one image can be uploaded" });

            //Check if uploaded file is an image file or not
            if (!new string[] { "image/jpeg", "image/jpg", "image/png" }.Contains(file[0].ContentType))
                return BadRequest("Invalid format. The file is not an image. Only types .jpeg, .jpg or .png are allowed.");

            ApplicationUser foundUser = await _userManager
                                                    .Users
                                                    .Include(x => x.ProfileImage)
                                                    .Where(x => x.Id == id)
                                                    .FirstOrDefaultAsync();


            //Delete the old photo from both Azure Blob and SQL
            if (foundUser.ProfileImage != null)
            {
                await _blobManager.DeleteFileFromBlobAsync(foundUser.ProfileImage.ContainerName, foundUser.ProfileImage.ResourceName);
                await _profileManager.DeleteProfileImageAsync(foundUser.ProfileImage.ProfileImageID);
            }

            string fileName = ContentDispositionHeaderValue.Parse(file[0].ContentDisposition).FileName.Trim('"');
            string hostedURL = await _blobManager.UploadFileToBlobAsync(foundUser.Id.ToString(), fileName, file[0].OpenReadStream());
            foundUser.ProfileImage = new ProfileImage
            {
                ApplicationUser = foundUser,
                ContainerName = foundUser.UserName,
                ProfileImageID = Guid.NewGuid(),
                ProfileImageUrl = hostedURL,
                ResourceName = fileName,
                UserID = foundUser.Id
            };

            //Update the database
            await _userManager.UpdateAsync(foundUser);
            return Ok(new { message = "Profile image successfully updated" });
        }

        /// <summary>
        /// Removes the profile image for users registered with Visneto
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete("profile-image/{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProfileImageAsync(Guid id)
        {
            ApplicationUser foundUser = await _userManager.Users
                                                          .Include(x => x.ProfileImage)
                                                          .FirstOrDefaultAsync(x => x.Id == id);
            if (foundUser == null)
                return NotFound(new { message = $"No user found with id: {id}" });

            //Delete both from Azure Blob and SQL
            await _blobManager.DeleteFileFromBlobAsync(foundUser.ProfileImage.ContainerName, foundUser.ProfileImage.ResourceName);
            await _profileManager.DeleteProfileImageAsync(foundUser.ProfileImage.ProfileImageID);

            //Return HTTP 204
            return NoContent();
        }
   
        /// <summary>
        /// Updates the profile detials based on the id of the user 
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="ReplacementUserProfileObject"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUserDetailsAsync(Guid id, [FromBody] UserProfileModelDTO ReplacementUserProfileObject)
        {

            ApplicationUser foundUser = await _userManager.Users
                                                          .Include(x => x.ProfileImage)
                                                          .FirstOrDefaultAsync(x => x.Id == id);

            if (foundUser == null)
                return NotFound(new { message = $"No user found with id: {id}" });

            foundUser.UserName = ReplacementUserProfileObject.UserName;
            foundUser.FirstName = ReplacementUserProfileObject.FirstName;
            foundUser.LastName = ReplacementUserProfileObject.LastName;
            foundUser.PhoneNumber = ReplacementUserProfileObject.PhoneNumber;

            await _userManager.UpdateAsync(foundUser);
            return Ok(new { message = "User details updated successfully!" });
        }
        
        /// <summary>
        /// Provides the data that are loaded in the admin dashboard
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin-dashboard")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAdminDashboardDataAsync()
        {
            IList<ApplicationUser> adminUsersList = await _userManager.GetUsersInRoleAsync(UserType.Admin.ToString());
            List<Booking> bookingList = await _bookingService.AllBookingDetailsAsync();
            List<VenueDetail> venueList = await _venueService.GetVenueDetailsAsync();

            var bookings = (from booking in bookingList
                            join venue in venueList
                            on booking.VenueID equals venue.VenueID
                            group booking.VenueDetail by booking.VenueID into venueBookingGroup
                            select new
                            {
                               venueBookingGroup.ElementAt(0).VenueName,
                               venueBookingGroup.ElementAt(0).VenueCity,
                               BookingCount = venueBookingGroup.Count()
                            }).ToList();
                         
            return Ok(new
            {
                AdminCount = adminUsersList.Count,
                BookingCount = bookingList.Count,
                VenueCount = venueList.Count,
                VenueBookingMapping = bookings
            });
        }
    }
}