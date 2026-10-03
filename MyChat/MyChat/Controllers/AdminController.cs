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

        await _userManager.SetLockoutEnabledAsync(user, true);

        await _userManager.SetLockoutEndDateAsync(
            user,
            DateTimeOffset.MaxValue
        );

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
}