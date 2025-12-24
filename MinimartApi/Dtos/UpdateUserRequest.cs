using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos {
    public class UpdateUserRequest {
        public string? Password { get; set; }
        [StringLength(100)]
        public string? FullName { get; set; }
        [StringLength(250)]
        public string? Address { get; set; }
    }
}
