using System;
using System.Collections.Generic;
using System.Text;

namespace zenvy.application.DTOs.Users
{
    public class UserDto
    {
        public string UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
