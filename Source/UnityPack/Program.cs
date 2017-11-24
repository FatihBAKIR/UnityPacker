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
            var rootDir = args.Length > 2 ? args[2] : "Assets/";
            var exts = args.Length > 3 ? args[3].Split(',') : new string[0];
            var dirs = args.Length > 4 ? args[4].Split(',') : new string[0];

            var extensions = new List<string>(exts)
            {
                "meta" // always skip meta files
            };

            // Create a package object from the given directory
            var pack = Package.FromDirectory(inpath, fileName, true, extensions.ToArray(), dirs);
            pack.GeneratePackage(rootDir);
        }
    }
}