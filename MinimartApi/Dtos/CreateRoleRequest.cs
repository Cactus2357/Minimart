using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos {
    public class CreateRoleRequest {
        [Required, StringLength(50)]
        public required string Name { get; set; }
    }
}
