using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;

namespace MyChat.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public ChatController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        string? currentUserId = _userManager.GetUserId(User);

        var messages = await _context.Messages
            .Include(message => message.User)
            .OrderByDescending(message => message.CreatedOn)
            .Take(30)
            .OrderBy(message => message.CreatedOn)
            .Select(message => new
            {
                id = message.Id,
                text = message.Text,
                createdOn = message.CreatedOn.ToString("dd.MM.yyyy HH:mm"),
                userId = message.UserId,
                userName = message.User != null ? message.User.UserName : "Неизвестный пользователь",
                avatarPath = message.User != null ? message.User.AvatarPath : null,
                isMine = message.UserId == currentUserId
            })
            .ToListAsync();

        return Json(messages);
    }
    
    [HttpPost]
    public async Task<IActionResult> SendMessage(string text)
    {
        string? currentUserId = _userManager.GetUserId(User);

        if (currentUserId == null)
        {
            return Json(new { success = false });
        }

        User? user = await _userManager.FindByIdAsync(currentUserId);

        if (user == null)
        {
            return Json(new { success = false });
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now)
        {
            return Json(new
            {
                success = false,
                error = "Заблокированный пользователь не может отправлять сообщения"
            });
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return Json(new
            {
                success = false,
                error = "Введите сообщение"
            });
        }

        Message message = new Message
        {
            Text = text,
            CreatedOn = DateTime.Now,
            UserId = currentUserId
        };

        _context.Messages.Add(message);

        user.MessagesCount++;

        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}