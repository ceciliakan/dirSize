// http://github.com/ceciliakan/dirSize

using System;
using System.IO;
using System.Data;
using System.Collections;

public class StackBasedIteration
{
    static void Main()
    {
        string outloc = null; 
        while (outloc == null)
        {
            Console.WriteLine("Where shoud I save the text file output? Please enter full path to directory.");
            outloc = Console.ReadLine();
            
            if (!Directory.Exists(outloc))
            {
               Console.WriteLine("{0} is not a valid directory.", outloc);
               outloc = null;
            }
        }

        string txtname = null;
        while (txtname == null)
        {
            Console.WriteLine("Please enter file name without extenstion.");
            txtname = Console.ReadLine();

            if ( outloc.EndsWith(Path.DirectorySeparatorChar.ToString()) )
            {
                txtname = outloc + txtname + ".txt";
            }
            else
            {
                txtname = outloc + "/" + txtname + ".txt";
            }

            if (File.Exists(txtname))
            {
               Console.WriteLine("{0} already exists.", txtname);
               txtname = null;
            }
        }

        DataTable dirInfo = new DataTable();
        dirInfo.Columns.Add("Name", typeof(string));
        dirInfo.Columns.Add("FullPath", typeof(string));
        dirInfo.Columns.Add("Size", typeof(decimal));
        dirInfo.Columns.Add("SubFolders", typeof(string));
        dirInfo.Columns.Add("Files", typeof(string));

        string[] drives = Environment.GetLogicalDrives();

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
            Console.WriteLine("Begin calculating folder sizes");
            TraverseTree(rootDir, dirInfo);
            Console.WriteLine("Finished calculating folder sizes");
        }
        
        Console.WriteLine("Begin calculating folder sizes");

        // Sort datatable by size
        DataView sortView = new DataView(dirInfo);
        sortView.Sort = "Size DESC";

        // write to json txt file
        using (System.IO.StreamWriter file = 
            new System.IO.StreamWriter(@txtname, true))
        {
            Console.WriteLine("Starting to write file.");

            file.WriteLine("{");
            foreach (DataRowView row in sortView)
            {
                file.WriteLine("\"{0}\":{\n",row["Name"]);
                file.WriteLine("\"Full Path\":{0},", row["FullPath"]);
                file.WriteLine("\"Size\":{0},", row["Size"]);
                file.WriteLine("\"Sub Folders\":[{0}],", row["SubFolders"]);
                file.WriteLine("\"Files\":[{0}]", row["Files"]);
                file.WriteLine("},\n");
            }
            file.WriteLine("}");

            Console.WriteLine("Finished writing file.");
        }
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
            string subDirNames = null;
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

            // Push the subdirectories onto the stack for traversal.
            foreach (string str in subDirs)
            {
                dirs.Push(str);
                subDirNames = subDirNames + ", " + str;
            }
                
            // Get file size
            string[] files = null;
            string fileNames = null;
            decimal fileSize = 0;
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
                    FileInfo fi = new FileInfo(file);
                    fileSize += fi.Length;
                    fileNames = fileNames + ", " + fi.Name;

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
            if (subDirNames != null)
            {
                subDirNames = subDirNames.Remove(0,1);                    
            }
            if (fileNames != null)
            {
                fileNames = fileNames.Remove(0,1);                    
            }

            dirInfo.Rows.Add(Path.GetFileName(currentDir), currentDir, fileSize, subDirNames ,fileNames);
            sumDirSize(fileSize, currentDir, dirInfo);
        }
    }
    public static void sumDirSize(Decimal fileSize, string currentDir, DataTable dirInfo)
    {
        if (fileSize > 0)
        {
            string traversePath = currentDir + ".";
            string rootDrive = Path.GetPathRoot(currentDir);
            do
            {
                try
                {
                    traversePath = traversePath.Remove(traversePath.Length-1);                    
                    traversePath = Path.GetDirectoryName(traversePath);
                    string criteria = "FullPath = '" + traversePath + "'";
                    DataRow[] matchRows = dirInfo.Select(criteria);

                    for(int i = 0; i < matchRows.Length; i ++)
                    {
                        matchRows[i]["Size"] = Convert.ToDecimal(matchRows[i]["Size"]) + fileSize;
                    }
                }
                catch (System.IndexOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            } while (string.Compare(rootDrive, traversePath) != 0);

        }
    }
}