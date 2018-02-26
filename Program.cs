// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree

using System;
using System.IO;
using System.Data;
using System.Collections;

public class StackBasedIteration
{
    static void Main()
    {
        String[] drives = Environment.GetLogicalDrives();
        DataTable dirInfo = new DataTable();
        dirInfo.Columns.Add("Name", typeof(string));
        dirInfo.Columns.Add("Location", typeof(string));
        dirInfo.Columns.Add("Size", typeof(decimal));
        dirInfo.Columns.Add("Type", typeof(string));

        foreach (string dr in drives)
        {
            DriveInfo di = new DriveInfo(dr);

            // Here we skip the drive if it is not ready to be read. This
            // is not necessarily the appropriate action in all scenarios.
            if (!di.IsReady)
            {
                Console.WriteLine("The drive {0} could not be read", di.Name);
                continue;
            }
            string rootDir = di.Name;
            TraverseTree(rootDir, dirInfo);
        }

        //Console.WriteLine("Press any key");
        //Console.ReadKey();
    }

    public static void TraverseTree(string root, DataTable dirInfo)
    {
        // Data structure to hold names of subfolders to be
        // examined for files.
        Stack dirs = new Stack(500);

        if (!Directory.Exists(root))
        {
            throw new ArgumentException();
        }
        dirs.Push(root);

        while (dirs.Count > 0)
        {
            string currentDir = dirs.Pop().ToString();
            string[] subDirs;
            try
            {
                subDirs = Directory.GetDirectories(currentDir);
            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder or file. It may or may not be acceptable 
            // to ignore the exception and continue enumerating the remaining files and 
            // folders. It is also possible (but unlikely) that a DirectoryNotFound exception 
            // will be raised. This will happen if currentDir has been deleted by
            // another application or thread after our call to Directory.Exists. The 
            // choice of which exceptions to catch depends entirely on the specific task 
            // you are intending to perform and also on how much you know with certainty 
            // about the systems on which this code will run.
            catch (UnauthorizedAccessException e)
            {                    
                Console.WriteLine(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }

            if (subDirs == null | subDirs.Length == 0)
            {
                dirInfo.Rows.Add(currentDir, Directory.GetParent(currentDir).FullName, 0.0, "end Directory");
            }
            else
            {
                // Push the subdirectories onto the stack for traversal.
                foreach (string str in subDirs)
                    dirs.Push(str);

                dirInfo.Rows.Add(currentDir, Directory.GetParent(currentDir).FullName, 0.0, "Folder");
            }

            // Get file size
            string[] files = null;
            try
            {
                files = Directory.GetFiles(currentDir);
            }

            catch (UnauthorizedAccessException e)
            {
                
                Console.WriteLine(e.Message);
                continue;
            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }
            
            foreach (string file in files)
            {
                try
                {
                    // Store file path and size in Datatable.
                    FileInfo fi = new FileInfo(file);
                    dirInfo.Rows.Add(fi.Name, currentDir, fi.Length, "file");
                    Console.WriteLine("{0}: {1}", fi.Name, fi.Length);
                }
                catch (FileNotFoundException e)
                {
                    // If file was deleted by a separate application
                    //  or thread since the call to TraverseTree()
                    // then just continue.
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
    }
    public static void orderDirSize(DataTable dirInfo)
    {
        dirInfo.DefaultView.Sort = "columnName DESC";
    }
}

