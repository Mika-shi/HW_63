using System.ComponentModel.DataAnnotations;

namespace MyChat.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите дату рождения")]
    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Display(Name = "Аватар")]
    public IFormFile? Avatar { get; set; }

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = "";
}