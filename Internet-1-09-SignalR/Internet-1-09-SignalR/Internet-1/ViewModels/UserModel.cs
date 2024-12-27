using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Internet_1.ViewModels
{
    public class UserModel
    {
        public string Id { get; set; }

        [Display(Name = "Adı Soyadı")]
        [Required(ErrorMessage = "Adı Soyadı Giriniz!")]
        public string FullName { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı Adı Giriniz!")]
        public string UserName { get; set; }

        [Display(Name = "E-Posta")]
        [Required(ErrorMessage = "E-Posta Giriniz!")]
        public string Email { get; set; }

        [Display(Name = "Parola")]
        [Required(ErrorMessage = "Parola Giriniz!")]
        public string Password { get; set; }

        [Display(Name = "Roller")]
        public List<string> Role { get; set; } = new List<string>();

        [Display(Name = "Seçilen Rol")]
        public string SelectedRole { get; set; }

        [Display(Name = "Fotoğraf")]
        public string PhotoUrl { get; set; }
    }
}