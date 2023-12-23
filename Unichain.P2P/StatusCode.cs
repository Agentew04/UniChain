using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public enum StatusCode {
        Invalid = 0,
        OK = 200,
        BadRequest = 400,
        NotFound = 404,
        InternalServerError = 500
    }
}
