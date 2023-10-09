using System.Text.RegularExpressions;

namespace Util {
    public class ConstInfo {
        public static readonly Regex ContainerRegex = new Regex(@"(?<var>\w+)\[(?<number>[0-9]+)\]|(?<var>\w+)\[""?(?<key>\w+)""?\]");
    }
}