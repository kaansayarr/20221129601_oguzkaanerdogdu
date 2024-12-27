using System.ComponentModel.DataAnnotations;

namespace Internet_1.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public string PhotoUrl { get; set; }

        [Display(Name = "Mevcut Şifre")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Display(Name = "Yeni Şifre")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; }

        [Display(Name = "Yeni Şifre (Tekrar)")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Rol")]
        public string Role { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime JoinDate { get; set; }
    }
}