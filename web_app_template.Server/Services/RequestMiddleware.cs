using Newtonsoft.Json;

namespace web_app_template.Server.Services
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ResponseMessages _responseMessages;

        public RequestMiddleware(RequestDelegate next, ResponseMessages responseMessages)
        {
            _next = next;
            _responseMessages = responseMessages;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Stream originalBody = context.Response.Body;
            string responseBody;
            try
            {
                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;

                    await _next(context);

                    memStream.Position = 0;
                    responseBody = new StreamReader(memStream).ReadToEnd();

                    dynamic responseData = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    int code = responseData?.message ?? 0;

                    if (code > 0)
                    {
                        if (!(code >= 200 && code <= 299))
                            context.Response.StatusCode = code;

                        responseData["message"] = _responseMessages.GetMessage(code);
                        responseData["status"] = code;
                        responseData["success"] = code >= 200 && code <= 299 ? true : false;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            var writer = new StreamWriter(ms);
                            writer.Write(JsonConvert.SerializeObject(responseData));
                            writer.Flush();
                            ms.Position = 0;
                            await ms.CopyToAsync(originalBody);
                        }
                    }
                    else
                    {
                        memStream.Position = 0;
                        await memStream.CopyToAsync(originalBody);
                    }
                }
            }
            catch
            {
                context.Response.StatusCode = 500;
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }
}
