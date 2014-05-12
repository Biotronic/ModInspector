using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModInspector
{
    class Tools
    {
        private static void extractZip(FileInfo filename, DirectoryInfo path)
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(filename.FullName, path.FullName);
        }

        private static void extract7z(FileInfo filename, DirectoryInfo path)
        {
            SevenZip.SevenZipCompressor.SetLibraryPath(Path.GetFullPath("7z.dll"));
            var archive = new SevenZip.SevenZipExtractor(filename.FullName);
            archive.ExtractArchive(path.FullName);
        }

        private static void extractRar(FileInfo filename, DirectoryInfo path)
        {
            NUnrar.Archive.RarArchive.WriteToDirectory(filename.FullName, path.FullName);
        }

        static public string GetRandomTempFolder()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        static public void ExtractArchive(FileInfo archiveName, DirectoryInfo path)
        {
            switch (Path.GetExtension(archiveName.FullName))
            {
                case ".zip":
                    extractZip(archiveName, path);
                    break;
                case ".7z":
                    extract7z(archiveName, path);
                    break;
                case ".rar":
                    extractRar(archiveName, path);
                    break;
            }
        }
    }
}
