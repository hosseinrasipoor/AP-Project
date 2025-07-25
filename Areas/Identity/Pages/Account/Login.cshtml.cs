// ... existing license header ...

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Golestan.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http; // Add this for cookie operations

namespace Golestan.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<SampleUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private const string RememberMeCookieName = "RememberMeEmail"; // Cookie name constant

        public LoginModel(SignInManager<SampleUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;

            // Initialize Input model and check for remember me cookie
            Input = new InputModel();
            if (Request.Cookies.TryGetValue(RememberMeCookieName, out var email))
            {
                Input.Email = email;
                Input.RememberMe = true; // Auto-check "Remember Me"
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    // Handle Remember Me cookie
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = Input.RememberMe
                            ? DateTime.Now.AddDays(30) // 30-day persistence
                            : DateTime.Now.AddMinutes(-1) // Expire immediately
                    };

                    Response.Cookies.Append(
                        RememberMeCookieName,
                        Input.Email,
                        cookieOptions
                    );

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }

                // ... existing login failure handling ...
            }

            return Page();
        }
    }
}