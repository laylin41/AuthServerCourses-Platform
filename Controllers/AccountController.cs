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
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));
                    return Redirect(model.ReturnUrl ?? "/");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
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
            // build a view with a "Logout" confirmation if you want, or logout directly:
            var context = await _interaction.GetLogoutContextAsync(logoutId);

            // you can skip confirmation for simplicity:
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
