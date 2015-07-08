using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Diagnostics;

namespace Sitecore.Ship.Infrastructure.Update.InstallComitter
{
    public class ConfigFilesCommitter
    {
        private readonly string _pattern;
        private readonly Regex _renamer;

        public ConfigFilesCommitter()
        {
            _pattern = "*.config.*";
            _renamer = new Regex(@"\.config\.[\w\d]{8}-[\w\d]{4}-[\w\d]{4}-[\w\d]{4}-[\w\d]{12}");
        }

        public ConfigFilesCommitter(string packageName)
        {
            Assert.ArgumentNotNullOrEmpty(packageName, "packageName");

            _pattern = string.Format("*.config.{0}", packageName);
            _renamer = new Regex(_pattern.Replace("*", "").Replace(".", "\\."));
        }

        public Dictionary<string, string> Process(string path)
        {
            var result = new Dictionary<string, string>();

            foreach (var file in GetFilesToCommit(path))
            {
                var newName = Rename(file.FullName);

                if (string.Equals(file.FullName, newName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (File.Exists(newName))
                {
                    File.Delete(newName);
                }

                File.Move(file.FullName, newName);
                result.Add(file.FullName, newName);
            }

            return result;
        } 

        private IEnumerable<FileInfo> GetFilesToCommit(string path)
        {
            if (Directory.Exists(path))
                return new DirectoryInfo(path).EnumerateFiles(_pattern, SearchOption.AllDirectories);

            Log.Warn(string.Format("Cannot commit config files. Path {0} does not exist.", path), this);
            return Enumerable.Empty<FileInfo>();
        }

        private string Rename(string file)
        {
            return _renamer.IsMatch(file) ? _renamer.Replace(file, ".config") : file;
        }
    }
}