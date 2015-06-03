using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPack
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("UnityPack {Source Path} {Package Name = \"Package\"} {Root Path = \"\"} {Skipped Extensions (CSV) = \"\"} {Skipped Directories (CSV) = \"\"}");
                return;
            }
            
            string inpath = args[0];
            string fileName = args.Length > 1 ? args[1] : "Package";
            string root = args.Length > 2 ? args[2] : "";
            string[] exts = args.Length > 3 ? args[3].Split(',') : new string[0];
            string[] dirs = args.Length > 4 ? args[4].Split(',') : new string[0];
            bool meaningfulHashes = true;
        
            List<string> extensions = new List<string>(exts);
            
            string[] files = Directory.GetFiles(inpath, "*.*", SearchOption.AllDirectories);

            string tmpPath = Path.Combine(Path.GetTempPath(), "packUnity" + RandomStuff(8));
            Directory.CreateDirectory(tmpPath);

            foreach (string file in files)
            {
                bool skip = false;
                foreach (string dir in dirs)
                    if (file.Replace(".\\", "").StartsWith(dir))
                    {
                        skip = true;
                        break;
                    }

                string extension = Path.GetExtension(file).Replace(".", "");
                if (skip || extensions.Contains(extension))
                    continue;
                
                string hash1 = RandomHash(), hash2 = RandomHash();

                if (meaningfulHashes)
                {
                    string metaFile = file + ".meta";
                    string hash = "";
                    using (StreamReader read = new StreamReader(metaFile))
                    {
                        while (!read.EndOfStream)
                        {
                            string line = read.ReadLine();
                            if (line.StartsWith("guid"))
                            {
                                hash = line.Split(' ')[1];
                                break;
                            }
                        }
                    }
                    hash1 = hash;
                    Console.WriteLine(metaFile);
                }

                string path = Path.Combine(tmpPath, hash1);
                Directory.CreateDirectory(path);

                File.Copy(file, Path.Combine(path, "asset"));
                using (StreamWriter writer = new StreamWriter(Path.Combine(path, "pathname")))
                    writer.Write(root + file.Replace(".\\", "").Replace("\\", "/") + "\n" + hash2);
            }

            CreateTarGZ(fileName  + ".unitypackage", tmpPath);

            Directory.Delete(tmpPath, true);
        }

        static string RandomHash()
        {
            return CreateMd5(RandomStuff()).ToLower();
        }

        static string CreateMd5(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }


        static Random r = new Random();
        static string RandomStuff(int len = 32)
        {
            string c = "";
            for (int i = 0; i < len; i++)
            {
                c += r.Next(0, 128);
            }
            return c;
        }

        private static void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {

            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

            tarArchive.Close();
        }
        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {

            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            //
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            //
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
    }
}
