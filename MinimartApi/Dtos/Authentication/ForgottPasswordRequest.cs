using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Authentication {
    public class ForgottPasswordRequest {
        [Required, EmailAddress]
        public string Email {  get; set; } = string.Empty;
    }
}
