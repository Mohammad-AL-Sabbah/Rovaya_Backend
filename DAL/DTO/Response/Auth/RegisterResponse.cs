using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.DAL.DTO.Response.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string>? Errors { get; set; }

        public string? UserId { get; set; }
        public string? Email { get; set; }


    }
}
