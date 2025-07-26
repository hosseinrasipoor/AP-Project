using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Golestan.Models
{
    public class User
    {
        [Key,Required]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress , MaxLength(50)]
        public string Email { get; set; }

        [Required , MaxLength(50)]
        public string HashedPassword { get; set; }

       
        public ICollection<UserRole>? UserRoles { get; set; }
        public ICollection<Student>? StudentProfiles { get; set; }
        public ICollection<Instructor>? InstructorProfiles { get; set; }
    }

}
