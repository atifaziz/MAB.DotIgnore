﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MAB.DotIgnore
{
    public class IgnoreList
    {
        private List<IgnoreRule> _rules = new List<IgnoreRule>();

        public ReadOnlyCollection<IgnoreRule> Rules { get { return _rules.AsReadOnly(); } }

        /// <summary>
        /// Create a list of ignore rules to check paths against
        /// </summary>
        /// <param name="rules">A list of glob ignore patterns as strings</param>
        public IgnoreList(IEnumerable<string> rules)
        {
            AddRules(rules);
        }

        /// <summary>
        /// Create a list of ignore rules to check paths against
        /// </summary>
        /// <param name="ignoreFilePath">Path to a text file containing a list of glob ignore patterns as strings</param>
        public IgnoreList(string ignoreFilePath)
        {
            AddRules(File.ReadAllLines(ignoreFilePath));
        }

        /// <summary>
        /// Add a rule to the ignore list
        /// </summary>
        /// <param name="rule">Glob ignore pattern as string</param>
        public void AddRule(string rule)
        {
            AddRules(new string[] { rule });
        }

        /// <summary>
        /// Add multiple rules to the ignore list
        /// </summary>
        /// <param name="ignoreFilePath">Path to a text file containing a list of glob ignore patterns as strings</param>
        public void AddRules(string ignoreFilePath)
        {
            _rules.AddRange(CleanRules(File.ReadAllLines(ignoreFilePath)).Select(line => new IgnoreRule(line)));
        }

        /// <summary>
        /// Add multiple rules to the ignore list
        /// </summary>
        /// <param name="rules">A list of glob ignore patterns as strings</param>
        public void AddRules(IEnumerable<string> rules)
        {
            _rules.AddRange(CleanRules(rules).Select(line => new IgnoreRule(line)));
        }

        /// <summary>
        /// Remove a rule from the ignore list
        /// </summary>
        /// <param name="rule">Glob ignore pattern as string</param>
        public void RemoveRule(string rule)
        {
            _rules.RemoveAll(r => r.OriginalPattern == rule.Trim());
        }
        
        /// <summary>
        /// Check if a file path matches any of the rules in the ignore list
        /// </summary>
        /// <param name="file">FileInfo representing the file to chec</param>
        public bool IsMatch(FileInfo file)
        {
            return IsMatch(file.FullName, false);
        }

        /// <summary>
        /// Check if a directory path matches any of the rules in the ignore list
        /// </summary>
        /// <param name="file">FileInfo representing the file to check</param>
        /// <param name="log">List of strings to append log messages to</param>
        public bool IsMatch(FileInfo file, List<string> log)
        {
            return IsMatch(file.FullName, false, log);
        }

        /// <summary>
        /// Check if a directory path matches any of the rules in the ignore list
        /// </summary>
        /// <param name="directory">DirectoryInfo representing the file to check</param>
        public bool IsMatch(DirectoryInfo directory)
        {
            return IsMatch(directory.FullName, true);
        }

        /// <summary>
        /// Check if a directory path matches any of the rules in the ignore list
        /// </summary>
        /// <param name="directory">DirectoryInfo representing the file to check</param>
        /// <param name="log">List of strings to append log messages to</param>
        public bool IsMatch(DirectoryInfo directory, List<string> log)
        {
            return IsMatch(directory.FullName, true, log);
        }

        /// <summary>
        /// Check if a string path matches any of the rules in the ignore list
        /// </summary>
        /// <param name="path">String representing the path to check</param>
        /// <param name="pathIsDirectory">Should be set True if the path represents a directory, False if it represents a file</param>
        public bool IsMatch(string path, bool pathIsDirectory)
        {
            return IsMatch(path, pathIsDirectory, null);
        }

        // <summary>
        /// Check if a string path matches any of the rules in the ignore list
        /// </summary>
        /// <param name="path">String representing the path to check</param>
        /// <param name="pathIsDirectory">Should be set True if the path represents a directory, False if it represents a file</param>
        /// <param name="log">List of strings to append log messages to</param>
        public bool IsMatch(string path, bool pathIsDirectory, List<string> log)
        {
            // This pattern modified from https://github.com/henon/GitSharp/blob/master/GitSharp/IgnoreRules.cs
            var ignore = false;

            foreach (var rule in _rules)
            {
                if (rule.Exclude != ignore)
                {
                    ignore = rule.IsMatch(path, pathIsDirectory);

                    if (log != null)
                    {
                        log.Add(string.Format("{0} by {1}", (rule.Exclude ? "Ignored" : "Included"), rule.OriginalPattern));
                    }
                }
            }

            return ignore;
        }

        /// <summary>
        /// Create an exact copy of the ignore list
        /// </summary>
        public IgnoreList Clone()
        {
            return new IgnoreList(_rules.Select(x => x.OriginalPattern));
        }

        private IEnumerable<string> CleanRules(IEnumerable<string> rules)
        {
            // Exclude all comment or whitespace lines
            return rules.Select(line => line.Trim())
                        .Where(line => line.Length > 0 && !line.StartsWith("#", StringComparison.OrdinalIgnoreCase));
        }
    }
}