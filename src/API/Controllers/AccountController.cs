using Ardalis.GuardClauses;
using LetsWork.Domain.Exceptions;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using LetsWork.API.DTO;
using LetsWork.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using System.Security.Cryptography;

namespace LetsWork.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailManager;
        private readonly ITokenProviderService _tokenManager;
        private readonly ConfigSettings _configSettings;
        private readonly IReferralCodeService _referralCodeService;
        public AccountController(UserManager<ApplicationUser> UserManager,
                                 SignInManager<ApplicationUser> SignInManager,
                                 IEmailService EmailManager,
                                 IOptions<ConfigSettings> ConfigSettings,
                                 ITokenProviderService TokenProviderService,
                                 IReferralCodeService ReferralCodeService)

        {
            this._userManager = UserManager;
            this._signInManager = SignInManager;
            this._emailManager = EmailManager; 
            this._configSettings = ConfigSettings.Value;
            this._tokenManager = TokenProviderService;
            this._referralCodeService = ReferralCodeService;
        }

        /// <summary>
        /// Register the user
        /// </summary>
        /// <param name="RegisterUserModelDTO">RegisterUserModelDTO</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserModelDTO RegisterUserModelDTO)
        {
            Guard.Against.NullItem(RegisterUserModelDTO);

            //Check for duplicate user-names and e-mail
            ApplicationUser foundUser = await _userManager.Users.Where(x => x.Email == RegisterUserModelDTO.UserEmail || x.UserName == RegisterUserModelDTO.UserName).FirstOrDefaultAsync();

            //Throw HTTP 409 Conflict then
            if (foundUser != null)
                return StatusCode(409, new { message = $"The username / email is already taken and is conflicting with other records, please give an unique username / email." });

            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = RegisterUserModelDTO.UserName,
                Email = RegisterUserModelDTO.UserEmail,
                FirstName = RegisterUserModelDTO.FirstName,
                LastName = RegisterUserModelDTO.LastName,
                PhoneNumber = RegisterUserModelDTO.PhoneNumber,
                EmailConfirmed = false
            };
            
            IdentityResult createResult = await _userManager.CreateAsync(applicationUser, RegisterUserModelDTO.Password);

            //User creation failed because of some constraints
            if (!createResult.Succeeded)
                return BadRequest(new { message = createResult.GetIdentityResultErrorMessage() });

            await _signInManager.SignInAsync(applicationUser, false);
            await _userManager.AddToRoleAsync(applicationUser, UserType.User.ToString());

            //Generate JWT now
            string jwtToken = await _tokenManager.GenerateJWTAsync(applicationUser);

            //Send verify email now
            EmailModel emailModel = new EmailModel
            {
                EmailTo = RegisterUserModelDTO.UserEmail,
                Body = $"<html><body><a href='{_configSettings.URL.VerifyEmailURL}/{jwtToken}'>Click here to verify Email</a><br></body></html>",
                Subject = "Verify your Email"
            };
            await _emailManager.SendEmailAsync(emailModel);

            ReferralCode referralCode = GenerateReferralCode(applicationUser.UserName, applicationUser.Id);
            await _referralCodeService.AddReferralCode(referralCode);

            //Send Referral code mail now
            emailModel.Body = $"<html><body><fieldset><legend> Referral code for User - {applicationUser.UserName} </legend> {referralCode.RefCode} </fieldset></body></html>";
            emailModel.Subject = $"Visneto Referral Code for new user - {applicationUser.UserName}";
            await _emailManager.SendEmailAsync(emailModel);

            //Return HTTP 201 Created for new user
            return StatusCode(201, new 
            {
                role = UserType.User.ToString(),
                access_token = jwtToken,
                expires = 3600,
                email =  string.Empty,
                user_name = RegisterUserModelDTO.UserName
            });
        }

        /// <summary>
        /// Signs in the user
        /// </summary>
        /// <param name="SignInModelDTO">SignInModelDTO</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> LoginAsync([FromBody] SignInModelDTO SignInModelDTO)
        {
            Guard.Against.NullItem<SignInModelDTO>(SignInModelDTO);

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(SignInModelDTO.UserName, SignInModelDTO.Password, true, false);

            if (!signInResult.Succeeded)
                return StatusCode(401, new { message = $"Invalid credentials. The username/password entered does not match any records, please re-enter." });

            ApplicationUser loggedInUser = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == SignInModelDTO.UserName);
            IList<string> userRoles = await _userManager.GetRolesAsync(loggedInUser);

            JObject responseBody = new JObject();
            
            //Checks if the user role is set and the email is verified
            if (userRoles.Count > 0 && loggedInUser.EmailConfirmed)
            {
                responseBody.Add("role", JToken.FromObject(userRoles[0]));
                responseBody.Add("email", JToken.FromObject(loggedInUser.Email));
            }

            //Checks if the user role is set but the email is not verified
            else if (userRoles.Count > 0 && !loggedInUser.EmailConfirmed)
            {
                responseBody.Add("role", JToken.FromObject(userRoles[0]));
                responseBody.Add("email", JToken.FromObject(string.Empty));
            }

            //If none is set
            else
                return StatusCode(401, new { error = $"Unauthorized to login!!" });

            string login_jwt = await _tokenManager.GenerateJWTAsync(loggedInUser);
            responseBody.Add("access_token", JToken.FromObject(login_jwt));
            responseBody.Add("expires", JToken.FromObject(3600));
            responseBody.Add("user_name", JToken.FromObject(SignInModelDTO.UserName));
            return Ok(responseBody);
        }
        /// <summary>
        /// Signs in the user using Google Accounts
        /// </summary>
        /// <param name="AuthDTO">AuthDTO</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("external-login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ExternalLoginAsync([FromBody] OAuthDTO AuthDTO)
        {
            Guard.Against.NullItem<OAuthDTO>(AuthDTO);

            JObject responseBody = new JObject();

            ApplicationUser applicationUserRegistered = await _userManager.FindByEmailAsync(AuthDTO.Email);
            ApplicationUser applicationExternalUser = await _userManager.FindByLoginAsync("Google", AuthDTO.ProviderKey);

            //user registered with same email id of Visneto registration
            if (applicationUserRegistered != null && applicationExternalUser == null)
                return StatusCode(401, new { message = "Already registered with Visneto. Cannot use Google SignIn credentials to login. Please use Visneto credentials to login." });

            
            //User is already registered with Visneto and Google
            if (applicationUserRegistered != null && applicationExternalUser != null)
            {
                string access_token = await _tokenManager.GenerateJWTAsync(applicationUserRegistered);
                IList<string> externalLoginUserRolesList = await _userManager.GetRolesAsync(applicationUserRegistered);

                if (externalLoginUserRolesList.Count > 0)
                    responseBody.Add("role", JToken.FromObject(externalLoginUserRolesList[0]));

                else
                    return StatusCode(401, new { message = $"Unauthorized, no roles for the user specified." });

                responseBody.Add("access_token", JToken.FromObject(access_token));
                responseBody.Add("expires", JToken.FromObject(3600));
                responseBody.Add("user_name", JToken.FromObject(applicationExternalUser.FirstName));
                responseBody.Add("provider", JToken.FromObject("Google"));
                return Ok(responseBody);
            }

            //User is not registered with Visneto but Google
            ApplicationUser user = new ApplicationUser
            {
                UserName = AuthDTO.Email,
                Email = AuthDTO.Email,
                FirstName = AuthDTO.DisplayName,
                EmailConfirmed = true
            };
                
            IdentityResult createUser = await _userManager.CreateAsync(user);

            if(!createUser.Succeeded)
                return BadRequest(new { message = createUser.GetIdentityResultErrorMessage() });
                  
            user.ProfileImage = new ProfileImage
            {
                ProfileImageID = Guid.NewGuid(),
                ProfileImageUrl = AuthDTO.ImageUrl,
                ContainerName = String.Empty,
                ResourceName = String.Empty
            };

            await _userManager.AddToRoleAsync(user, UserType.User.ToString());
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", AuthDTO.ProviderKey, AuthDTO.DisplayName));
            string token = await _tokenManager.GenerateJWTAsync(user);

            //Generation of referral code
            ReferralCode referralCode = GenerateReferralCode(user.UserName, user.Id);
            await _referralCodeService.AddReferralCode(referralCode);

            //Send Referral code mail now
            EmailModel emailModel = new EmailModel
            {
                EmailTo = user.Email,
                Body = $"<html><body><fieldset><legend> Referral code for User - {user.UserName} </legend> {referralCode.RefCode} </fieldset></body></html>",
                Subject = $"Visneto Referral Code for new user - {user.UserName}"
            };

            await _emailManager.SendEmailAsync(emailModel);

            responseBody.Add("access_token", JToken.FromObject(token));
            responseBody.Add("expires", JToken.FromObject(3600));
            responseBody.Add("user_name", JToken.FromObject(user.Email));
            responseBody.Add("provider", JToken.FromObject("Google"));
            responseBody.Add("role", JToken.FromObject(UserType.User.ToString()));
            return StatusCode(201, responseBody);
        }

        /// <summary>
        /// Add more admins
        /// </summary>
        /// <param name="RegisterModelDTO">RegisterModelDTO</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("AddAdmin")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> AddAdminAsync([FromBody] RegisterUserModelDTO RegisterModelDTO)
        {
            Guard.Against.NullItem(RegisterModelDTO);

            //Check for duplicate user-names and e-mail
            ApplicationUser foundUser = await _userManager.Users.Where(x => x.Email == RegisterModelDTO.UserEmail || x.UserName == RegisterModelDTO.UserName).FirstOrDefaultAsync();

            //Throw HTTP 409 Conflict then
            if (foundUser != null)
                return StatusCode(409, new { message = $"The username / email is already taken and is conflicting with other records, please give an unique username / email." });

            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = RegisterModelDTO.UserName,
                Email = RegisterModelDTO.UserEmail,
                FirstName = RegisterModelDTO.FirstName,
                LastName = RegisterModelDTO.LastName,
                PhoneNumber = RegisterModelDTO.PhoneNumber,
                EmailConfirmed = false
            };

            //Create the admin now
            IdentityResult createResult = await _userManager.CreateAsync(applicationUser, RegisterModelDTO.Password);

            if (!createResult.Succeeded)
                return BadRequest(new { message = createResult.GetIdentityResultErrorMessage() });

            //Add admin to role
            await _userManager.AddToRoleAsync(applicationUser, UserType.Admin.ToString());

            //Generate JWT now
            string token = await _tokenManager.GenerateJWTAsync(applicationUser);

            EmailModel emailModel = new EmailModel
            {
                EmailTo = RegisterModelDTO.UserEmail,
                Body = $"<html><body><a href='{_configSettings.URL.VerifyEmailURL}/{token}'>Click here to verify Email</a></body></html>",
                Subject = "Verify your Email"
            };

            //Send verify email to admin now
            await _emailManager.SendEmailAsync(emailModel);

            //Return HTTP 201 now
            return StatusCode(201, new { message = $"Successfully added admin {RegisterModelDTO.UserName}." });  
        }

        #region Referral Code Generation
        [NonAction]
        string CreateCryptoRandomString()
        {
            //Initialize RNGCryptoServiceProvider 
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] byteArray = new byte[4];
            provider.GetBytes(byteArray);

            //convert 4 bytes to an integer
            string randomInteger = BitConverter.ToUInt32(byteArray, 0).ToString().Substring(0, 4);
            return randomInteger;
        }
        [NonAction]
        public ReferralCode GenerateReferralCode(string UserName, Guid UserId)
        {
            string userIdString = UserId.ToString();
            string referralCode = $"{UserName.Substring(0,4)}_{CreateCryptoRandomString()}";
            ReferralCode referralCodeDetails = new ReferralCode
            {
                ReferralCodeId = Guid.NewGuid(),
                UserId = UserId,
                ReferralCodeTransactionCount = 0,
                RefCode = referralCode
            };
            return referralCodeDetails;
        }
        #endregion
    }
}