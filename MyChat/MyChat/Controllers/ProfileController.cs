using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models;

namespace MyChat.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        UserManager<User> userManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Details(string? id)
    {
        User? user;

        if (string.IsNullOrEmpty(id))
        {
            user = await _userManager.GetUserAsync(User);
        }
        else
        {
            user = await _userManager.FindByIdAsync(id);
        }

        if (user == null)
        {
            return NotFound();
        }

        string? currentUserId = _userManager.GetUserId(User);

        ViewBag.IsMyProfile = currentUserId == user.Id;

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        string userName,
        string email,
        DateTime birthDate,
        IFormFile? avatar)
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        User? userWithSameName =
            await _userManager.FindByNameAsync(userName);

        if (userWithSameName != null &&
            userWithSameName.Id != user.Id)
        {
            ModelState.AddModelError(
                "userName",
                "Пользователь с таким именем уже существует"
            );
        }

        User? userWithSameEmail =
            await _userManager.FindByEmailAsync(email);

        if (userWithSameEmail != null &&
            userWithSameEmail.Id != user.Id)
        {
            ModelState.AddModelError(
                "email",
                "Пользователь с таким email уже существует"
            );
        }

        DateTime today = DateTime.Today;
        int age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        if (age < 18)
        {
            ModelState.AddModelError(
                "birthDate",
                "Пользователь должен быть старше 18 лет"
            );
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        if (avatar != null)
        {
            string uploadsFolder =
                Path.Combine(_environment.WebRootPath, "uploads");

            Directory.CreateDirectory(uploadsFolder);

            string fileName =
                Guid.NewGuid() + Path.GetExtension(avatar.FileName);

            string filePath =
                Path.Combine(uploadsFolder, fileName);

            using FileStream stream =
                new FileStream(filePath, FileMode.Create);

            await avatar.CopyToAsync(stream);

            user.AvatarPath = "/uploads/" + fileName;
        }

        user.UserName = userName;
        user.Email = email;
        user.BirthDate = birthDate;

        await _userManager.UpdateAsync(user);

        return RedirectToAction("Details");
    }
}