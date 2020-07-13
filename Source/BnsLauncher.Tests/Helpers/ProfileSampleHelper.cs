using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;

namespace BnsLauncher.Tests.Helpers
{
    public static class ProfileSampleHelper
    {
        static ProfileSampleHelper()
        {
            SolutionDirectory = AppContext.BaseDirectory.Split(
                new[] {"BnsLauncher.Tests"},
                StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public static string SolutionDirectory { get; }
        
        public static void WriteSampleProfile(this MockFileSystem fs, string testProfileRoot, string profile)
        {
            fs.Directory.CreateDirectory(testProfileRoot);
            
            var sampleDirectory = $"{SolutionDirectory}BnsLauncher.Core\\Samples\\{profile}";

            foreach (var file in Directory.EnumerateFiles(sampleDirectory))
            {
                var content = File.ReadAllBytes(file);
                fs.File.WriteAllBytes($"{testProfileRoot}\\{Path.GetFileName(file)}", content);
            }
        }
    }
}