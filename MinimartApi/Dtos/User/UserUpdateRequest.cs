using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.User
{
    public class UserUpdateRequest
    {
        public string? Password { get; set; }
        [StringLength(100)]
        public string? FullName { get; set; }
        [StringLength(250)]
        public string? Address { get; set; }
    }
}
