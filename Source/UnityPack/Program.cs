using System;
using System.Collections.Generic;

namespace UnityPacker
{
    class PackProgram
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine(
                    "UnityPacker [Source Path] [Package Name] [Respect Meta] [Omitted Extensions] [Omitted Directories]");
                return;
            }

            var inpath = args[0];
            var fileName = args.Length > 1 ? args[1] : "Package";
            var respectMeta = args.Length <= 2 || (args[2].ToLower() == "y" || args[2].ToLower() == "yes");
            var exts = args.Length > 4 ? args[4].Split(',') : new string[0];
            var dirs = args.Length > 5 ? args[5].Split(',') : new string[0];

            var extensions = new List<string>(exts)
            {
                "meta" // always skip meta files
            };

            // Create a package object from the given directory
            var pack = Package.FromDirectory(inpath, fileName, respectMeta, extensions.ToArray(), dirs);
            pack.GeneratePackage();
        }
    }
}