using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CasterUIAutomation.Communication
{
    public static class HttpUtility
    {
        public static Dictionary<string, string> ParseQueryStringParameters(string content)
        {
            // TODO: Doesn't handle parameters with multiple values. See for example here: https://weblog.west-wind.com/posts/2014/Sep/08/A-NET-QueryString-and-Form-Data-Parser

            if (string.IsNullOrWhiteSpace(content))
                return new Dictionary<string, string>();

            // TODO: this will fail if there's an HTML entity in the content (eg &amp;)
            string[] rawParameters = content.Split('&');
            if (rawParameters == null)
                return new Dictionary<string, string>();
            else
                return ParseQueryStringParameters(rawParameters);
        }

        public static Dictionary<string, string> ParseQueryStringParameters(string[] rawParameters)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (rawParameters != null)
            {
                foreach (string parameter in rawParameters)
                {
                    if (string.IsNullOrWhiteSpace(parameter))
                        continue;

                    string[] keyValuePair = parameter.Split('=');
                    if (keyValuePair == null || keyValuePair.Length < 1 || keyValuePair.Length > 2)
                        throw new ArgumentOutOfRangeException("Unable to parse the following HTTP request parameter: " + parameter);

                    string key = WebUtility.UrlDecode(keyValuePair[0]);
                    string value = keyValuePair.Length > 1 ? WebUtility.UrlDecode(keyValuePair[1]) : null;
                    parameters.Add(key, value);
                }
            }
            return parameters;
        }
    }
}

