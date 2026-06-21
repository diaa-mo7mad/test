using ARGI.DAL.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.DTO.Response
{
    public class LoginResponse : BaseResponse
    {

        public string? AccessToken { get; set; }

    }
}