using Rovaya.DAL.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class ResetPasswordRequest 
    {
        public string Code { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }


    }
}
