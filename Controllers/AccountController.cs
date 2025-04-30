using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IActionResult Login()
    {
        return View();
    }

}