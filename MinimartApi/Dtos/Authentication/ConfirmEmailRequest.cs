using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Authentication
{
    public class ConfirmEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
