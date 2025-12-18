using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos {
    public class ResendConfirmEmailRequest {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
