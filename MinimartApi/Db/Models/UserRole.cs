using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class UserRole {
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
