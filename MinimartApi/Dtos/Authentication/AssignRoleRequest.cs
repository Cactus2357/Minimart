using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Authentication {
    public class AssignRoleRequest {
        [Required, StringLength(50)]
        public string Role { get; set; }
    }
}
