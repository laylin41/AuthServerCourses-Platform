using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AuthServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Events;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace AuthServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IIdentityServerInteractionService interaction,
            IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _roleManager = roleManager;
            _events = events;
        }

        // -------------------- LOGIN --------------------
        [HttpGet]
        public IActionResult Login(string returnUrl, string expectedRole)
        {
            if (string.IsNullOrEmpty(expectedRole) && !string.IsNullOrEmpty(returnUrl))
            {
                var uri = new Uri("https://dummy" + returnUrl);
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                if (query.TryGetValue("expectedRole", out var roleFromUrl))
                {
                    expectedRole = roleFromUrl;
                }
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExpectedRole = expectedRole
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine("Inside POST Login");
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                Console.WriteLine($"Login attempt for user {user.Email}");
                Console.WriteLine($"ReturnUrl: {model.ReturnUrl}");
                Console.WriteLine($"User roles: {string.Join(",", userRoles)}");

                if (!string.IsNullOrEmpty(model.ExpectedRole) && !userRoles.Contains(model.ExpectedRole))
                {
                    ViewBag.LoginError = $"Авторизований користувач повинен бути \"{model.ExpectedRole}\".";
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));
                    return Redirect(model.ReturnUrl ?? "/");
                }
            }

            ViewBag.LoginError = "Невірний логін або пароль.";
            return View(model);
        }



        // -------------------- REGISTER --------------------
        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Username,
                FullName = model.FullName,
                PhoneNumber = model.Phone,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
                    await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));

                await _userManager.AddToRoleAsync(user, model.SelectedRole);
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));
                return Redirect(model.ReturnUrl ?? "/");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // -------------------- LOGOUT --------------------
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            return await LogoutConfirmed(logoutId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutConfirmed(string logoutId)
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await HttpContext.SignOutAsync("idsrv");

            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            if (!string.IsNullOrEmpty(logout?.PostLogoutRedirectUri))
            {
                return Redirect(logout.PostLogoutRedirectUri);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
