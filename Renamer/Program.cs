using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renamer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new InvalidOperationException("Too few input arguments");

            string filesDir = args[0];
            IEnumerable<string> extensions = args[1].Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<Tuple<string, string>> replacementPairs = ParseReplacements(args[2]).ToList();

            var files = Directory.GetFiles(filesDir, "*", SearchOption.AllDirectories)
                                 .Select(f => new FileInfo(f))
                                 .Where(f => HasAcceptedExtension(f, extensions))
                                 .Where(f => HasAnythingToReplace(f, replacementPairs));

            Parallel.ForEach(files, file =>
                {
                    string newName = ReplaceCharacters(file.Name, replacementPairs);
                    Console.WriteLine("Renaming {0} to {1}", file.Name, newName);

                    string newFile = Path.Combine(file.DirectoryName, newName);
                    if (File.Exists(newFile))
                        return;

                    File.Move(file.FullName, newFile);
                });
        }

        private static bool HasAnythingToReplace(FileInfo f, IEnumerable<Tuple<string, string>> replacementPairs)
        {
            return replacementPairs.Any(p => f.Name.Contains(p.Item1));
        }

        private static IEnumerable<Tuple<string, string>> ParseReplacements(string replacements)
        {
            var strings = replacements.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length % 2 != 0)
                throw new ArgumentException("String replacements count must be even");

            return strings.Select((s, index) => new { s, index })
                          .GroupBy(p => p.index / 2)
                          .Select(p => Tuple.Create(p.First().s, p.Skip(1).First().s));
        }

        private static string ReplaceCharacters(string fileName, IEnumerable<Tuple<string, string>> replacements)
        {
            var oldStrings = replacements.Select(r => r.Item1);
            var newStrings = replacements.Select(r => r.Item2);

            foreach (var replacementPair in replacements)
            {
                fileName = fileName.Replace(replacementPair.Item1, replacementPair.Item2);
            }

            return fileName;
        }

        private static bool HasAcceptedExtension(FileInfo file, IEnumerable<string> extensions)
        {
            return extensions.Any(ext => file.Extension.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
