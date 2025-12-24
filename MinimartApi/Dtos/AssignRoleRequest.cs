using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos {
    public class AssignRoleRequest {
        [Required]
        public int UserId { get; set; }
    }
}
