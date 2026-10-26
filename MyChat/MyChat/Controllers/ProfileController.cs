using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models;
using MyChat.Services;

namespace MyChat.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly EmailService _emailService;

    public ProfileController(
        UserManager<User> userManager,
        IWebHostEnvironment environment, 
        EmailService emailService)
    {
        _userManager = userManager;
        _environment = environment;
        _emailService = emailService;
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
            "userName", "Пользователь с таким именем уже существует"
        );
    }

    User? userWithSameEmail =
        await _userManager.FindByEmailAsync(email);

    if (userWithSameEmail != null &&
        userWithSameEmail.Id != user.Id)
    {
        ModelState.AddModelError(
            "email", "Пользователь с таким email уже существует"
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
            "birthDate", "Пользователь должен быть старше 18 лет"
        );
    }

    if (!ModelState.IsValid)
    {
        return View(user);
    }

    string? oldUserName = user.UserName;
    string? oldEmail = user.Email;
    DateTime oldBirthDate = user.BirthDate;
    string? oldAvatarPath = user.AvatarPath;

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

    IdentityResult result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(user);
    }

    List<string> changes = new List<string>();

    if (oldUserName != user.UserName)
    {
        changes.Add(
            $"Имя пользователя: <b>{oldUserName}</b> → <b>{user.UserName}</b>"
        );
    }

    if (oldEmail != user.Email)
    {
        changes.Add(
            $"Email: <b>{oldEmail}</b> → <b>{user.Email}</b>"
        );
    }

    if (oldBirthDate != user.BirthDate)
    {
        changes.Add(
            $"Дата рождения: <b>{oldBirthDate:dd.MM.yyyy}</b> → <b>{user.BirthDate:dd.MM.yyyy}</b>"
        );
    }

    if (oldAvatarPath != user.AvatarPath)
    {
        changes.Add("Аватар профиля был изменён");
    }

    if (changes.Count > 0)
    {
        string changesText = string.Join("<br>", changes);

        string message =
            $"<h2>Данные профиля изменены</h2>" +
            $"<p>Здравствуйте, {user.UserName}!</p>" +
            $"<p>В вашем профиле были изменены следующие данные:</p>" +
            $"<p>{changesText}</p>";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Изменение данных профиля",
            message
        );
    }

    return RedirectToAction("Details");
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMyData()
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        string profileLink = Url.Action(
            "Details",
            "Profile",
            new { id = user.Id },
            Request.Scheme
        )!;

        string message =
            $"<h2>Ваши данные MyChat</h2>" +
            $"<p><b>Имя пользователя:</b> {user.UserName}</p>" +
            $"<p><b>Email:</b> {user.Email}</p>" +
            $"<p><b>Дата рождения:</b> {user.BirthDate:dd.MM.yyyy}</p>" +
            $"<p><b>Количество отправленных сообщений:</b> {user.MessagesCount}</p>" +
            $"<p><a href=\"{profileLink}\">Перейти в профиль</a></p>";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Ваши данные MyChat",
            message
        );

        return RedirectToAction("Details");
    }
}