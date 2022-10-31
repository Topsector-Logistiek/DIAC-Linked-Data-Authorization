namespace ApiDiac.Data
{
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Configuration;

    public class ConfigTemplate
    {
        private const string VariablePattern = @"{{(\w+)}}";
        private static readonly Regex Regex = new (VariablePattern);

        public static string? Expand(string key, IConfiguration config)
        {
            var value = config[key];

            if (value == null)
            {
                return null;
            }

            var result = Regex.Replace(value, match =>
            {
                var refKey = match.Groups[1];
                return Expand(refKey.Value, config) !;
            });

            return result;
        }
    }
}