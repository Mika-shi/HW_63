using System.ComponentModel.DataAnnotations;

namespace MyChat.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя или email")]
    [Display(Name = "Имя пользователя или Email")]
    public string Login { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}