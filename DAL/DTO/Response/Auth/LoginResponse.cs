using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.DAL.DTO.Response.Auth
{
    public class LoginResponse
    {
        public List<string>? Errors {  get; set; }
        public string Message { get; set; }

        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public List<string>? Roles { get; set; }

        public string? AccessToken { get; set; }

    }
}
