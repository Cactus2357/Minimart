namespace MinimartApi.Models {
    public class Role {
        public const string Admin = "Admin";
        public const string User = "User";

        public int RoleId { get; set; }
        public required string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
