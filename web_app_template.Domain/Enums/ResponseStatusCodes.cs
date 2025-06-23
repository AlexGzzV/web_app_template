namespace web_app_template.Domain.Enums
{
    public enum ResponseStatusCodes
    {
        //Success codes
        Ok = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,

        //client codes
        BadRequest = 400,
        Unauthorized = 401,
        Forbiden = 403,
        NotFound = 404,

        //Server codes
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502
    }
}
