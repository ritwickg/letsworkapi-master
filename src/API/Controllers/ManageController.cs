using Ardalis.GuardClauses;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using LetsWork.API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LetsWork.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ManageController : ControllerBase
    {
        private readonly ConfigSettings _configSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailManager;
        private readonly ITokenProviderService _tokenProviderService;
        public ManageController(IOptions<ConfigSettings> ConfigurationSettings,
                                         UserManager<ApplicationUser> UserManager,
                                         IEmailService EmailService,
                                         ITokenProviderService TokenProviderService)
        {
            _configSettings = ConfigurationSettings.Value;
            _userManager = UserManager;
            _emailManager = EmailService;
            _tokenProviderService = TokenProviderService;
        }

        /// <summary>
        /// Verifies the emails of the users registered with Visneto
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpPut("verify-email/{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> VerifyUserEmailAsync(string id)
        {
            Guard.Against.NullString(id);

            ApplicationUser user = await _userManager.FindByEmailAsync(id);

            if (user == null)
                return NotFound(new { message = $"No user found with email: {id}!" });

            //If the email is already verified
            if (user.EmailConfirmed)
                return Ok(new { message = $"Email -> {id} is already verified!" });
                   
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                email = id,
                id = user.Id.ToString(),
                message = $"Successfully verified user with email: {id}."
            });
        }

        /// <summary>
        ///  Checks if an username is available or not
        /// </summary>
        /// <param name="CheckUserNameModel">CheckUserNameModel</param>
        /// <returns></returns>
        [Authorize(Roles = "User,Admin")]
        [HttpPost("check-username")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CheckUserNameAvailibilityAsync([FromBody] CheckUserNameDTO CheckUserNameModel)
        {
            Guard.Against.NullItem(CheckUserNameModel);

            ApplicationUser duplicateUsernameUser = await _userManager.FindByNameAsync(CheckUserNameModel.UserName);

            if (duplicateUsernameUser == null || duplicateUsernameUser.Id == CheckUserNameModel.Id)
                return Ok(new { message = "true" });

            return Ok(new { message = "false" });
        }

        /// <summary>
        /// Sends an email to the registered email for change password link
        /// </summary>
        /// <param name="email">email</param>
        /// <returns></returns>
        [HttpGet("changepassword-request/{email}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePasswordSendEmailUserAsync(string email)
        {
            Guard.Against.NullString(email);

            ApplicationUser applicationUser = await _userManager.FindByEmailAsync(email);

            if (applicationUser == null)
                return NotFound(new { message = $"User not found with the specified email." });

            //Checking if the user can change password
            IList<UserLoginInfo> userLogins = await _userManager.GetLoginsAsync(applicationUser);

            if (userLogins.Count > 0)
                return StatusCode(401, new { message = $"Account not authorized to change password, please update the password with your login provider." });
            
            string token = await _tokenProviderService.GenerateJWTAsync(applicationUser);

            EmailModel emailModel = new EmailModel
            {
                EmailTo = applicationUser.Email,
                Body = $"<html><body><a href='{_configSettings.URL.ChangePasswordURL}/{token}'>Click here to Change Password</a><br></body></html>",
                Subject = "Visneto - Change Password"
            };

            await _emailManager.SendEmailAsync(emailModel);
            return Ok(new { message = $"Email sent for change password request, please check your registered email's inbox." });
        }

        /// <summary>
        /// Sends an email to the registered email with a link to reset the password
        /// </summary>
        /// <param name="Username">Username</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("forgotpassword-request/{Username}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ForgotPasswordRequestAsync(string Username)
        {
            Guard.Against.NullString(Username);

            ApplicationUser applicationUser = await _userManager.FindByNameAsync(Username);

            if (applicationUser == null)
                return BadRequest(new { message = "User email is not registered" });

            IList<UserLoginInfo> userLogins = await _userManager.GetLoginsAsync(applicationUser);
            if (userLogins.Count > 0)
                return BadRequest(new { message = $"User not authorised to reset password with Visneto, please change password with the registered provider" });

            string token = await _tokenProviderService.GenerateJWTAsync(applicationUser);

            EmailModel emailModel = new EmailModel
            {
                EmailTo = applicationUser.Email,
                Body = $"<html><body><a href='{_configSettings.URL.ForgotPasswordURL}/{token}'>Click here to reset password</a><br></body></html>",
                Subject = "Visneto - Password Recovery"
            };

            await _emailManager.SendEmailAsync(emailModel);
            return Ok(new { message = $"Forget password link sent to registered email" });
        }
    }
}