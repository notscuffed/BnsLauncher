using System.Xml;

namespace BnsLauncher.Core
{
    public static class XmlExtensions
    {
        public static string GetNodeText(this XmlNode node, string path, string defaultValue)
        {
            if (node == null)
                return defaultValue;

            var foundNode = node.SelectSingleNode(path);

            return foundNode == null ? defaultValue : foundNode.InnerText.Trim();
        }
    }
}