using System.Text.RegularExpressions;

namespace Web.MainApplication.Service.System
{
    public class HtmlValidationHelper
    {
        public static bool IsEmailFormat(string format)
        {
            bool isValid = false;
            string pattern = "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
            if (string.IsNullOrWhiteSpace(format)) {
                isValid = false;
                return isValid;
            }
            if (Regex.IsMatch(format, pattern))
            {
                isValid = true;
            }
            return isValid;
        }

    }
}