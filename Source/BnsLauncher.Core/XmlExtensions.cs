using System;
using System.Linq;
using System.Xml;

namespace BnsLauncher.Core
{
    public static class XmlExtensions
    {
        private static readonly string[] _trueValues = {"true", "1", "yes", "ok"};

        public static string GetNodeText(this XmlNode node, string path, string defaultValue)
        {
            if (node == null)
                return defaultValue;

            var foundNode = node.SelectSingleNode(path);

            return foundNode == null ? defaultValue : foundNode.InnerText.Trim();
        }

        public static bool GetNodeBoolean(this XmlNode node, string path, bool defaultValue)
        {
            if (node == null)
                return defaultValue;

            var foundNode = node.SelectSingleNode(path);

            return foundNode == null
                ? defaultValue
                : _trueValues.Contains(foundNode.InnerText.Trim(), StringComparer.OrdinalIgnoreCase);
        }
    }
}