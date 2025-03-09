using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Settings
{
    public class JwtSettings
    {
        [Required]
        [MinLength(32)]
        public string Secret { get; set; }

        [Range(1, 1440)]
        public int ExpiryMinutes { get; set; } = 60;

        [Required]
        public string Issuer { get; set; }

        [Required]
        public string Audience { get; set; }
    }
}
