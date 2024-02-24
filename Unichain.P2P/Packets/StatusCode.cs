namespace Unichain.P2P;

public enum StatusCode : ushort {
    Invalid = 0,
    OK = 200,
    BadRequest = 400,
    NotFound = 404,
    InternalServerError = 500
}
