using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Authentication {
    public class ResendConfirmEmailRequest {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
