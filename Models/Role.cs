namespace Golestan.Models
{
    public class Role
    {
        public int Id { get; set; }
        public RoleType Name { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
