using Microsoft.Extensions.Options;
using SinovadMediaServer.Application.Configuration;
using SinovadMediaServer.Transversal.Interface;
using System.Globalization;
using System.Text;

namespace SinovadMediaServer.Application.Shared
{
    public class SharedService
    {
        public IAppLogger<SharedService> _tracer;

        public MyConfig _config;

        public SharedService(IAppLogger<SharedService> logger, IOptions<MyConfig> config)
        {
            _config = config.Value;
            _tracer = logger;
        }

        public string GetFormattedText(string input)
        {
            return Simplify(input).Replace(" ", "").Replace(".", "").Replace(":", "").Replace(",", "").Replace("-", "").Replace("!", "").Replace("¡", "").Replace("?", "").Replace("¿", "").Trim().ToUpper();
        }

        private static string Simplify(string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

    }
}
