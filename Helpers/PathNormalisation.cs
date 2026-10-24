using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectContextAnalyser
{
    static class PathNormalisation
    {
        public static string NormaliseIgnorePath(string input)
        {
            string returnString = GeneralPathNormalise(input);

            if (!returnString.EndsWith(Path.DirectorySeparatorChar))
            {
                returnString += Path.DirectorySeparatorChar;
            }

            return returnString;
        }

        public static string GeneralPathNormalise(string input)
        {
            string returnString;
            returnString = Regex.Replace(input, @"\\+","\\");
            returnString = Regex.Replace(returnString, @"/+", "/");

            returnString = returnString.Replace('\\', Path.DirectorySeparatorChar);
            returnString = returnString.Replace('/', Path.DirectorySeparatorChar);
            return returnString;
        }
    }
}