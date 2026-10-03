using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models;
using MyChat.ViewModels;

namespace MyChat.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        User? userWithSameName = await _userManager.FindByNameAsync(model.UserName);

        if (userWithSameName != null)
        {
            ModelState.AddModelError(
                "UserName",
                "Пользователь с таким именем уже существует"
            );

            return View(model);
        }

        User? userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);

        if (userWithSameEmail != null)
        {
            ModelState.AddModelError(
                "Email",
                "Пользователь с таким email уже существует"
            );

            return View(model);
        }

        DateTime today = DateTime.Today;
        int age = today.Year - model.BirthDate.Year;

        if (model.BirthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        if (age < 18)
        {
            ModelState.AddModelError(
                "BirthDate",
                "Регистрация доступна только пользователям старше 18 лет"
            );

            return View(model);
        }

        string? avatarPath = null;

        if (model.Avatar != null)
        {
            string uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads"
            );

            Directory.CreateDirectory(uploadsFolder);

            string fileName =
                Guid.NewGuid() +
                Path.GetExtension(model.Avatar.FileName);

            string filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            using FileStream stream = new FileStream(
                filePath,
                FileMode.Create
            );

            await model.Avatar.CopyToAsync(stream);

            avatarPath = "/uploads/" + fileName;
        }

        User user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            BirthDate = model.BirthDate,
            AvatarPath = avatarPath,
            MessagesCount = 0
        };

        IdentityResult result =
            await _userManager.CreateAsync(
                user,
                model.Password
            );

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "user");
            
            await _signInManager.SignInAsync(
                user,
                isPersistent: false
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
        
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(
                "",
                GetRussianError(error.Code)
            );
        }

        return View(model);
    }

    private string GetRussianError(string code)
    {
        if (code == "PasswordTooShort")
            return "Пароль должен содержать минимум 6 символов";

        if (code == "PasswordRequiresUpper")
            return "Пароль должен содержать хотя бы одну заглавную букву";

        if (code == "PasswordRequiresLower")
            return "Пароль должен содержать хотя бы одну строчную букву";

        if (code == "PasswordRequiresDigit")
            return "Пароль должен содержать хотя бы одну цифру";

        return "Ошибка регистрации";
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        User? user = null;

        if (model.Login.Contains("@"))
        {
            user = await _userManager.FindByEmailAsync(model.Login);
        }
        else
        {
            user = await _userManager.FindByNameAsync(model.Login);
        }

        if (user == null)
        {
            ModelState.AddModelError("", "Неверное имя пользователя, email или пароль");
            return View(model);
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Неверное имя пользователя, email или пароль");

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }
}