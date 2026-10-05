using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MyChat.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMemoryCache _cache;
    
    private class CachedMessage
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? AvatarPath { get; set; }
    }

    public ChatController(ApplicationDbContext context, UserManager<User> userManager, IMemoryCache cache)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
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
        
        const string cacheKey = "last_30_messages";
        
        if (!_cache.TryGetValue(cacheKey, out List<CachedMessage>? messages))
        {
            messages = await _context.Messages
                .Include(message => message.User)
                .OrderByDescending(message => message.CreatedOn)
                .Take(30)
                .OrderBy(message => message.CreatedOn)
                .AsNoTracking()
                .Select(message => new CachedMessage
                {
                    Id = message.Id,
                    Text = message.Text,
                    CreatedOn = message.CreatedOn,
                    UserId = message.UserId,
                    UserName = message.User != null && message.User.UserName != null
                        ? message.User.UserName
                        : "Неизвестный пользователь",
                    AvatarPath = message.User != null
                        ? message.User.AvatarPath
                        : null
                }).ToListAsync();

            _cache.Set(
                cacheKey,
                messages,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }
            );
        }

        var result =messages!.Select(message => new
            {
                id = message.Id,
                text = message.Text,
                createdOn = message.CreatedOn.ToString("dd.MM.yyyy HH:mm"),
                userId = message.UserId,
                userName = message.UserName ?? "Неизвестный пользователь",
                avatarPath = message.AvatarPath,
                isMine = message.UserId == currentUserId
            });

        return Json(result);
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
        
        _cache.Remove("last_30_messages");

        return Json(new { success = true });
    }
}