using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Razor_RolesDemo.Pages
{
    [BindProperties]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;

        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }

        public RegisterModel(SignInManager<IdentityUser> signInManager)
        {
            this.signInManager = signInManager;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if(ModelState.IsValid)
            {
                IdentityUser newUser = new()
                {
                    UserName = Username,
                };

                var createUserResult = await signInManager.UserManager.CreateAsync(newUser, Password);

                if(createUserResult.Succeeded)
                {
                    var signInResult = await signInManager.PasswordSignInAsync(Username, Password, false, false);
                    
                    if(signInResult.Succeeded)
                    {
                        return RedirectToPage("/Member/Index");
                    }
                }
            }

            return Page();
        }
    }
}
