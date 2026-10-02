using Microsoft.AspNetCore.Identity;

namespace MyChat.Models;

public class User : IdentityUser
{
    public DateTime BirthDate { get; set; }

    public string? AvatarPath { get; set; }

    public int MessagesCount { get; set; }
}