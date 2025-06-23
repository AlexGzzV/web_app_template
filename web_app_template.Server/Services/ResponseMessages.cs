using Microsoft.Extensions.Localization;

namespace web_app_template.Server.Services
{
    public class ResponseMessages
    {
        private readonly IStringLocalizer<ResponseMessages> _localizer;
        public ResponseMessages(IStringLocalizer<ResponseMessages> localizer) => _localizer = localizer;
        public string GetMessage(int code) { return _localizer[code.ToString()]; }
    }
}
