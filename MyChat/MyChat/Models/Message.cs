namespace MyChat.Models;

public class Message
{
    public int Id { get; set; }

    public string Text { get; set; } = "";

    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public string UserId { get; set; } = "";

    public User? User { get; set; }
}