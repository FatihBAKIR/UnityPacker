using System;
using System.Collections.Generic;

namespace UnityPacker
{
    internal class UnpackProgram
    {
        public static void Main(string[] args)
        {   
            var p = Package.FromPackage(args[0]);
            p.GenerateFolder(args.Length > 0 ? args[1] : "");
        }
    }
}