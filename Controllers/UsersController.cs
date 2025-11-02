using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Bio,
                user.MediaUrl,
                user.SocialLinks
            });
        }

        // PUT api/users/{id}
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UserProfileUpdateModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Bio = model.Bio;
            user.SocialLinks = model.SocialLinks;

            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                // Завантаження на Cloudinary
                var cloudinaryService = HttpContext.RequestServices.GetRequiredService<CloudinaryService>();
                user.MediaUrl = await cloudinaryService.UploadFileAsync(model.PhotoFile);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Bio,
                user.MediaUrl,
                user.SocialLinks
            });
        }

        public class UserProfileUpdateModel
        {
            public string FullName { get; set; } = null!;
            public string? Bio { get; set; }
            public string? SocialLinks { get; set; }
            public IFormFile? PhotoFile { get; set; }
        }
    }
}
