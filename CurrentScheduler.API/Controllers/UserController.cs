using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKrakus.CurrentScheduler.API
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]/{Action}")]
    public class UserController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;

        public ILogger<UserController> _logger { get; }
        public RoleManager<IdentityRole> _roleManager { get; }
        public UserManager<AppUser> _userManager { get; }

        public UserController(ILogger<UserController> logger, RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("UserController");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] InputModel userInput)
        {
            var x = userInput;
            var user = new AppUser { UserName = userInput.Email, Email = userInput.Email };
            var result = await _userManager.CreateAsync(user, userInput.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // SetupValidation Email
                //var callbackUrl = Url.Page(
                //    "/Account/ConfirmEmail",
                //    pageHandler: null,
                //    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                //    protocol: Request.Scheme);

                //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                //{
                //    return RedirectToPage("RegisterConfirmation", new { email = userInput.Email, returnUrl = returnUrl });
                //}
                //else
                //{
                    await _signInManager.SignInAsync(user, isPersistent: false);
                //    return LocalRedirect(returnUrl);
                //}
            }
            //foreach (var error in result.Errors)
            //{
            //    ModelState.AddModelError(string.Empty, error.Description);
            //}

            return Ok();
        }

        public async Task<IActionResult> GenerateRoles()
        {
            string[] roles = { "Admin" };

            foreach (var role in roles)
            {
                var roleExist = await _roleManager.RoleExistsAsync(role); ;

                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var user = await _userManager.FindByEmailAsync("ZKrakus@gmail.com");
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return Ok();
        }
    }
}
