using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models;

namespace MyChat.Controllers;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly UserManager<User> _userManager;

    public AdminController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Users()
    {
        List<User> users = _userManager.Users.ToList();

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Block(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        
        string? currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId)
        {
            TempData["Error"] = "Вы не можете заблокировать собственный аккаунт.";
            return RedirectToAction("Users");
        }

        await _userManager.SetLockoutEnabledAsync(user, true);

        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        await _userManager.SetLockoutEndDateAsync(
            user,
            null
        );

        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, string userName, string email, DateTime birthDate)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        User? userWithSameName = await _userManager.FindByNameAsync(userName);

        if (userWithSameName != null && userWithSameName.Id != user.Id)
        {
            ModelState.AddModelError("userName", "Пользователь с таким именем уже существует");
        }

        User? userWithSameEmail = await _userManager.FindByEmailAsync(email);

        if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
        {
            ModelState.AddModelError("email", "Пользователь с таким email уже существует");
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        user.UserName = userName;
        user.Email = email;
        user.BirthDate = birthDate;

        await _userManager.UpdateAsync(user);

        return RedirectToAction("Users");
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string userName, string email, DateTime birthDate, string password)
    {
        User? userWithSameName = await _userManager.FindByNameAsync(userName);

        if (userWithSameName != null)
        {
            ModelState.AddModelError("userName", "Пользователь с таким именем уже существует");
        }

        User? userWithSameEmail = await _userManager.FindByEmailAsync(email);

        if (userWithSameEmail != null)
        {
            ModelState.AddModelError("email", "Пользователь с таким email уже существует");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        User user = new User
        {
            UserName = userName,
            Email = email,
            BirthDate = birthDate,
            MessagesCount = 0
        };

        IdentityResult result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        await _userManager.AddToRoleAsync(user, "user");

        return RedirectToAction("Users");
    }
}