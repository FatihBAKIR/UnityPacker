using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("UnityPacker {Source Path} {Package Name = \"Package\"} {Root Path = \"\"} {Skipped Extensions (CSV) = \"\"} {Skipped Directories (CSV) = \"\"}");
                return;
            }
            
            string inpath = args[0];
            string fileName = args.Length > 1 ? args[1] : "Package";
            bool meaningfulHashes = args.Length > 2 ? args[2].ToLower() == "y" || args[2].ToLower() == "yes" : false;
            string root = args.Length > 3 ? args[3] : "";
            string[] exts = args.Length > 4 ? args[4].Split(',') : new string[0];
            string[] dirs = args.Length > 5 ? args[5].Split(',') : new string[0];

            List<string> extensions = new List<string>(exts)
            {
                //"meta"
            };
            
            string[] files = Directory.GetFiles(inpath, "*.*", SearchOption.AllDirectories);

            string tmpPath = Path.Combine(Path.GetTempPath(), "packUnity" + RandomStuff(8));
            Directory.CreateDirectory(tmpPath);

			for	(int i = 0; i < files.Length; ++i)
			{
				string file = files[i];
				string altName = file;
				if (file.StartsWith("."))
                	altName = file.Replace("." + Path.DirectorySeparatorChar, "");
				
				bool skip = false;
                foreach (string dir in dirs)
                    if (altName.StartsWith(dir))
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
                }

                string path = Path.Combine(tmpPath, hash1);
                Directory.CreateDirectory(path);

                File.Copy(file, Path.Combine(path, "asset"));
                using (StreamWriter writer = new StreamWriter(Path.Combine(path, "pathname")))
                    writer.Write(root + altName.Replace(Path.DirectorySeparatorChar + "", "/") + "\n" + hash2);
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
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

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

			Console.WriteLine(sourceDirectory);
			tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);
			Console.WriteLine(tarArchive.RootPath);

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

            tarArchive.Close();
        }

        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            TarEntry tarEntry;
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarEntry.Name = filename.Remove(0, tarArchive.RootPath.Length + 1);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, true);
            }
        }
    }
}
