using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Configuration;

namespace NugetCleanupTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] allfileslist;
            string strRootpath = @"E:\1ES.SCOMCPY\SystemCenter\Migration\SCOM\src";
            string strPath = @"E:\1ES.SCOMCPY\SystemCenter\Migration\SCOM\src\product\sdk\server\RuntimeService";
            //string directoryCmdPath = String.Empty;

            string[] Outlines;
            List<string> allRootfilesItemList = new List<string>();
            List<string> allCsprojOfNugetsList = new List<string>();
            if (args.Length != 0)
            {
                strRootpath = args[0];
            }
            //GetCSprojlistIntoTextFile(strRootpath, allRootfilesItemList);
            //allfileslist = GetCSprojlistIntoTextFile(strRootpath, allRootfilesItemList);
            //Findcorrespondingcsproj(strPath, allCsprojOfNugetsList);

            //Task 1
            // Below Code reads the entry from dirs.proj as for migrated projects we create csproj entry in dirs.proj 
            // CSV List of migrated .csproj,.vcxproj,.mpproj,.wixproj projects
            //InternalBinaryList(strRootpath);


            //Task 2
            // List of .inc files under a migrated project folders
            //IncFilelist(strRootpath);


            //Task 3
            // List of files in placefile
            // Take the Key, Value pairs from each line of placefile Which do not start ';' character
            // Read the Value seprated by ':' character into below format
            // Example: 
            // A.dll path1:path2:path3
            // <Xxx.dll> <path1>:<path2>:<path3>
            //FilesinPlaceFileList(strRootpath);

            //Task 2
            // Sources, Makefile, and Makefile.inc cleanup: These files should be deleted from all migrated projects. 
            // This task should also remove the files.inc related files.

            // Getting the list of all migrated csproj,mpproj,vcxproj etc.
            List<string> allRootfilesItemList1 = MigratedProjectsList(strRootpath);
            foreach (var directoryCmdPath in allRootfilesItemList1)
            {
                CleanUpTFSFile(strRootpath, directoryCmdPath);
            }

            Console.ReadLine();
        }
        // Sources, Makefile, and Makefile.inc & files.inc related files
        private static void CleanUpTFSFile(string strRootpath, string directoryCmdPath)
        {
            DeleteFiles(directoryCmdPath, "sources");
            DeleteFiles(directoryCmdPath, "Makefile");
            DeleteFiles(directoryCmdPath, "makefile.inc");
            //DeleteFiles(directoryCmdPath, "files.inc");
        }
        private static void DeleteFiles(string rootpath, string format, Func<string, bool> searchFilterFunc = null)
        {
            var csprojfiles = (searchFilterFunc == null) ? Directory.GetFiles(rootpath, format, SearchOption.AllDirectories) :
                Directory.GetFiles(rootpath, format, SearchOption.AllDirectories).Where(searchFilterFunc);
            foreach (string csprojfile in csprojfiles)
            {
                Console.WriteLine("Deleting {0}", csprojfile);
                ForceDeleteFile(csprojfile);
            }
        }
        private static void ForceDeleteFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            RemoveReadOnlyFlag(filename);
            File.Delete(filename);
        }
        private static void RemoveReadOnlyFlag(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            FileAttributes attrs = File.GetAttributes(filename);
            if (attrs.HasFlag(FileAttributes.ReadOnly))
            {
                File.SetAttributes(filename, attrs & ~FileAttributes.ReadOnly);
            }
        }

        private static void FilesinPlaceFileList(string strRootpath)
        {
            List<string> allRootfilesItemList = new List<string>();

            if (Directory.Exists(strRootpath))
            {
                int i = 0;
                string[] allfileslist = Directory.GetFiles(strRootpath, "*placefile*", SearchOption.AllDirectories);
                string str1 = string.Empty;
                bool flag = true;
                //foreach (var item in allfileslist)
                //{
                //    i = i + 1;
                //    str1 = i.ToString() + "," + item;
                //    System.Console.WriteLine(str1);
                //    allRootfilesItemList.Add(str1);
                //}
                foreach (string line1 in allfileslist)
                {
                    string[] lines = System.IO.File.ReadAllLines(line1);

                    // string str1, str2;
                    // Display the file contents by using a foreach loop.
                    foreach (var item in lines)
                    {
                        StringBuilder stringBuilder = new StringBuilder(item.Length);
                        int j = 0;
                        foreach (char c in item)
                        {
                            if (c != ' ' || j == 0 || item[j - 1] != ' ')
                                stringBuilder.Append(c);
                            j++;
                        }
                        str1 = stringBuilder.ToString();
                        if (!str1.Contains(";"))
                        {
                            str1 = str1.Replace(":", ",");
                            allRootfilesItemList.Add(str1);
                        }
                    }
                }
                allfileslist = null;
                File.WriteAllLines(@"C:\Users\v-gikala\source\repos\NugetCleanupTest\NugetCleanupTest\filesInPlaceFileList.txt", allRootfilesItemList.ToList());
            }
            else
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                string path = System.IO.Path.GetDirectoryName(ass.Location);
                //System.IO.File.Copy(@"\\sccxe-scratch\scratch\v-satvin\IES\InternalBinaryList.txt", @path + @"\InternalBinaryList.txt", true);
            }
            Console.ReadLine();
        }

        private static void IncFilelist(string strRootpath)
        {
            List<string> allRootfilesItemList = new List<string>();

            if (Directory.Exists(strRootpath))
            {
                int i = 0;
                string[] allfileslist = Directory.GetFiles(strRootpath, "*.inc", SearchOption.AllDirectories);
                string str1;
                foreach (var item in allfileslist)
                {
                    i = i + 1;
                    str1 = i.ToString() + "," + item;
                    System.Console.WriteLine(str1);
                    allRootfilesItemList.Add(str1);
                }
                File.WriteAllLines(@"C:\Users\v-gikala\source\repos\NugetCleanupTest\NugetCleanupTest\IncFileList.txt", allRootfilesItemList.ToList());
            }
            else
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                string path = System.IO.Path.GetDirectoryName(ass.Location);
                //System.IO.File.Copy(@"\\sccxe-scratch\scratch\v-satvin\IES\InternalBinaryList.txt", @path + @"\InternalBinaryList.txt", true);
            }
            Console.ReadLine();
        }

        private static List<string> MigratedProjectsList(string strInputFolderPath)
        {
            List<string> allRootfilesItemList = new List<string>();
            //strInputFolderPath = strInputFolderPath.Remove(strInputFolderPath.Length - 1);

            if (Directory.Exists(strInputFolderPath))
            {
                int i = 0;
                string[] allfileslist = Directory.GetFiles(strInputFolderPath, "*dirs.*", SearchOption.AllDirectories);
                foreach (string line1 in allfileslist)
                {
                    string[] lines = System.IO.File.ReadAllLines(line1);
                    // string str1, str2;
                    // Display the file contents by using a foreach loop.
                    foreach (string line in lines)
                    {
                        if (line.Contains(@"<ProjectReference") && !line.Contains(@"!--") && !line.Contains(@"-->") && !line.Contains(@"dirs.proj") && line.Contains(@".csproj"))
                        {
                            var line0 = line.Replace(@"<ProjectReference Include=", "").Replace("\t", "").Replace(" ", "").Replace("\"", "").Replace(@"$(SRCROOT)", strInputFolderPath).Replace(@"/>", "");
                            if (File.Exists(line0))
                            {
                                string appPath = System.IO.Path.GetDirectoryName(line0);
                                allRootfilesItemList.Add(appPath);
                            }
                            else
                            {
                                System.Console.WriteLine(line0);
                            }
                        }
                    }
                    allfileslist = null;
                    File.WriteAllLines(@"C:\Users\v-gikala\source\repos\NugetCleanupTest\NugetCleanupTest\MigratedProjectsList.txt", allRootfilesItemList.ToList());
                }
            }
            else
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                string path = System.IO.Path.GetDirectoryName(ass.Location);
                //System.IO.File.Copy(@"\\sccxe-scratch\scratch\v-satvin\IES\InternalBinaryList.txt", @path + @"\InternalBinaryList.txt", true);
            }
            //Console.ReadLine();
            return allRootfilesItemList;
        }

        private static void InternalBinaryList(string strInputFolderPath)
        {
            List<string> allRootfilesItemList = new List<string>();
            //strInputFolderPath = strInputFolderPath.Remove(strInputFolderPath.Length - 1);

            if (Directory.Exists(strInputFolderPath))
            {
                int i = 0;
                string[] allfileslist = Directory.GetFiles(strInputFolderPath, "*dirs.*", SearchOption.AllDirectories);
                foreach (string line1 in allfileslist)
                {
                    string[] lines = System.IO.File.ReadAllLines(line1);
                    // string str1, str2;
                    // Display the file contents by using a foreach loop.
                    foreach (string line in lines)
                    {
                        if (line.Contains(@"<ProjectReference") && !line.Contains(@"!--") && !line.Contains(@"-->") && !line.Contains(@"dirs.proj") && line.Contains(@".csproj"))
                        {
                            var line0 = line.Replace(@"<ProjectReference Include=", "").Replace("\t", "").Replace(" ", "").Replace("\"", "").Replace(@"$(SRCROOT)", strInputFolderPath).Replace(@"/>", "");
                            if (File.Exists(line0))
                            {
                                string[] lines1 = System.IO.File.ReadAllLines(line0);
                                string str1, str2 = "", str3;
                                i++;
                                string strAssembly = string.Empty, strGu = string.Empty;
                                string strAssemblyline = lines1.Where(x => x.Contains(@"<AssemblyName>")).FirstOrDefault();
                                if (!string.IsNullOrEmpty(strAssemblyline))
                                {
                                    strAssembly = strAssemblyline.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "").Trim();

                                    string strGuideline = lines1.Where(x => x.Contains(@"<ProjectGuid>")).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(strGuideline))
                                    {
                                        strGu = (strGuideline.Replace(@"<ProjectGuid>", "").Replace(@"</ProjectGuid>", "").Replace("{", "").Replace("}", "")).ToString().Trim();
                                    }
                                    str2 = i.ToString() + "," + strAssembly.Trim() + "," + line0.Trim() + "," + strGu + "," + ".csproj";
                                    System.Console.WriteLine(str2);
                                    allRootfilesItemList.Add(str2);
                                }
                            }
                            else
                            {
                                System.Console.WriteLine(line0);
                            }
                        }
                        if (line.Contains(@"<ProjectReference") && !line.Contains(@"!--") && !line.Contains(@"-->") && !line.Contains(@"dirs.proj") && line.Contains(@".vcxproj"))
                        {
                            var line0 = line.Replace(@"<ProjectReference Include=", "").Replace("\t", "").Replace(" ", "").Replace("\"", "").Replace(@"$(SRCROOT)", strInputFolderPath).Replace(@"/>", "");
                            if (File.Exists(line0))
                            {
                                string[] lines1 = System.IO.File.ReadAllLines(line0);
                                string str1, str2 = "", str3;
                                i++;
                                string strAssembly = string.Empty, strGu = string.Empty;
                                string strAssemblyline = lines1.Where(x => x.Contains(@"<AssemblyName>")).FirstOrDefault();
                                if (!string.IsNullOrEmpty(strAssemblyline))
                                {
                                    strAssembly = strAssemblyline.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "").Trim();

                                    string strGuideline = lines1.Where(x => x.Contains(@"<ProjectGuid>")).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(strGuideline))
                                    {
                                        strGu = (strGuideline.Replace(@"<ProjectGuid>", "").Replace(@"</ProjectGuid>", "").Replace("{", "").Replace("}", "")).ToString().Trim();
                                    }
                                    str2 = i.ToString() + "," + strAssembly.Trim() + "," + line0.Trim() + "," + strGu + "," + ".vcxproj";
                                    System.Console.WriteLine(str2);
                                    allRootfilesItemList.Add(str2);
                                }
                            }
                            else
                            {
                                System.Console.WriteLine(line0);
                            }
                        }
                        if (line.Contains(@"<ProjectReference") && !line.Contains(@"!--") && !line.Contains(@"-->") && !line.Contains(@"dirs.proj") && line.Contains(@".mpproj"))
                        {
                            var line0 = line.Replace(@"<ProjectReference Include=", "").Replace("\t", "").Replace(" ", "").Replace("\"", "").Replace(@"$(SRCROOT)", strInputFolderPath).Replace(@"/>", "");
                            if (File.Exists(line0))
                            {
                                string[] lines1 = System.IO.File.ReadAllLines(line0);
                                string str1, str2 = "", str3;
                                i++;
                                string strAssembly = string.Empty, strGu = string.Empty;
                                string strAssemblyline = lines1.Where(x => x.Contains(@"<AssemblyName>")).FirstOrDefault();
                                if (!string.IsNullOrEmpty(strAssemblyline))
                                {
                                    strAssembly = strAssemblyline.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "").Trim();

                                    string strGuideline = lines1.Where(x => x.Contains(@"<ProjectGuid>")).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(strGuideline))
                                    {
                                        strGu = (strGuideline.Replace(@"<ProjectGuid>", "").Replace(@"</ProjectGuid>", "").Replace("{", "").Replace("}", "")).ToString().Trim();
                                    }
                                    str2 = i.ToString() + "," + strAssembly.Trim() + "," + line0.Trim() + "," + strGu + "," + ".mpproj";
                                    System.Console.WriteLine(str2);
                                    allRootfilesItemList.Add(str2);
                                }
                            }
                            else
                            {
                                System.Console.WriteLine(line0);
                            }
                        }
                        if (line.Contains(@"<ProjectFile") && !line.Contains(@"!--") && !line.Contains(@"-->") && !line.Contains(@"dirs.proj") && line.Contains(@".wixproj"))
                        {
                            var line0 = line.Replace(@"<ProjectFile Include=", "").Replace("\t", "").Replace(" ", "").Replace("\"", "").Replace(@"$(SRCROOT)", strInputFolderPath).Replace(@"/>", "");
                            line0 = line1.Replace("dirs.proj", "") + line0;
                            if (File.Exists(line0))
                            {
                                string[] lines1 = System.IO.File.ReadAllLines(line0);
                                string str1, str2 = "", str3;
                                i++;
                                string strAssembly = string.Empty, strGu = string.Empty;
                                string strAssemblyline = lines1.Where(x => x.Contains(@"<Name>")).FirstOrDefault();
                                if (!string.IsNullOrEmpty(strAssemblyline))
                                {
                                    strAssembly = strAssemblyline.Replace(@"<Name>", "").Replace(@"</Name>", "").Trim();

                                    string strGuideline = lines1.Where(x => x.Contains(@"<ProjectGuid>")).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(strGuideline))
                                    {
                                        strGu = (strGuideline.Replace(@"<ProjectGuid>", "").Replace(@"</ProjectGuid>", "").Replace("{", "").Replace("}", "")).ToString().Trim();
                                    }
                                    str2 = i.ToString() + "," + strAssembly.Trim() + "," + line0.Trim() + "," + strGu + "," + ".wixproj";
                                    System.Console.WriteLine(str2);
                                    allRootfilesItemList.Add(str2);
                                }
                            }
                            else
                            {
                                System.Console.WriteLine(line0);
                            }
                        }

                    }

                }
                allfileslist = null;
                //Assembly ass = Assembly.GetExecutingAssembly();
                //string path = System.IO.Path.GetDirectoryName(ass.Location);
                //File.WriteAllLines(@path + @"\InternalBinaryList.txt", allRootfilesItemList.ToList());
                File.WriteAllLines(@"C:\Users\v-gikala\source\repos\NugetCleanupTest\NugetCleanupTest\InternalBinaryList.txt", allRootfilesItemList.ToList());
            }
            else
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                string path = System.IO.Path.GetDirectoryName(ass.Location);
                //System.IO.File.Copy(@"\\sccxe-scratch\scratch\v-satvin\IES\InternalBinaryList.txt", @path + @"\InternalBinaryList.txt", true);
            }
            Console.ReadLine();
        }

        private static void GetCSprojlistIntoTextFile(string strRootpath, List<string> allRootfilesItemList)
        {
            string[] allfileslist;
            if (Directory.Exists(strRootpath))
            {
                int i = 0;
                allfileslist = Directory.GetFiles(strRootpath, "*.csproj", SearchOption.AllDirectories);
                foreach (string line1 in allfileslist)
                {

                    string[] lines = System.IO.File.ReadAllLines(line1);
                    string str1, str2;
                    // Display the file contents by using a foreach loop.


                    /*foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        if (line.Contains(@"<AssemblyName>") && line.Contains(@"</AssemblyName>"))
                        {
                            str1 = line.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "");
                            i++;
                            str2 = i.ToString() + "," + str1.Trim() + "," + line1.Trim();
                            System.Console.WriteLine(str2);
                            allRootfilesItemList.Add(str2);

                        }

                    }*/

                    string strAssembly = string.Empty, strGu = string.Empty;
                    string strAssemblyline = lines.Where(x => x.Contains(@"<AssemblyName>")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(strAssemblyline))
                    {
                        strAssembly = strAssemblyline.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "").Trim();

                        string strGuideline = lines.Where(x => x.Contains(@"<ProjectGuid>")).FirstOrDefault();
                        if (!string.IsNullOrEmpty(strGuideline))
                        {
                            strGu = (strGuideline.Replace(@"<ProjectGuid>", "").Replace(@"</ProjectGuid>", "").Replace("{", "").Replace("}", "")).ToString().Trim();
                        }
                        i++;
                        str2 = i.ToString() + "," + strAssembly.Trim() + "," + line1.Trim() + "," + strGu;

                        System.Console.WriteLine(str2);
                        allRootfilesItemList.Add(str2);

                    }
                }

                //File.WriteAllLines(@"\\sccxe-scratch\scratch\v-satvin\IES\InternalBinaryList.txt", allRootfilesItemList.ToList());
                File.WriteAllLines(@"C:\Users\v-gikala\source\repos\NugetCleanupTest\NugetCleanupTest\InternalBinaryList.txt", allRootfilesItemList.ToList());


            }
        }
        //private static void Findcorrespondingcsproj(string strPath, List<string> allCsprojOfNugetsList)
        //{
        //    string[] allfileslist;
        //    if (Directory.Exists(strPath))
        //    {
        //        int i = 0;
        //        allfileslist = Directory.GetFiles(strPath, "*.csproj", SearchOption.AllDirectories);
        //        foreach (string line1 in allfileslist)
        //        {

        //            string[] lines = System.IO.File.ReadAllLines(line1);
        //            string str1, str2;
        //            Display the file contents by using a foreach loop.


        //            /*foreach (string line in lines)
        //            {
        //                 Use a tab to indent each line of the file.
        //                if (line.Contains(@"<AssemblyName>") && line.Contains(@"</AssemblyName>"))
        //                {
        //                    str1 = line.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "");
        //                    i++;
        //                    str2 = i.ToString() + "," + str1.Trim() + "," + line1.Trim();
        //                    System.Console.WriteLine(str2);
        //                    allRootfilesItemList.Add(str2);

        //                }

        //            }*/

        //           string strAssembly = string.Empty, strGu = string.Empty;
        //            string strAssemblyline = lines.Where(x => x.Contains(@"<AssemblyName>")).FirstOrDefault();
        //            if (!string.IsNullOrEmpty(strAssemblyline))
        //            {
        //                strAssembly = strAssemblyline.Replace(@"<AssemblyName>", "").Replace(@"</AssemblyName>", "").Trim();

        //                string strGuideline = lines.Where(x => x.Contains(@"<ProjectGuid>")).FirstOrDefault();
        //                if (!string.IsNullOrEmpty(strGuideline))
        //                {
        //                    strGu = (strGuideline.Replace(@"<ProjectGuid>", "").Replace(@"</ProjectGuid>", "").Replace("{", "").Replace("}", "")).ToString().Trim();
        //                }
        //                i++;
        //                str2 = i.ToString() + "," + strAssembly.Trim() + "," + line1.Trim() + "," + strGu;

        //                System.Console.WriteLine(str2);
        //                allRootfilesItemList.Add(str2);

        //            }
        //        }
        //    }
        //}
    }
}
