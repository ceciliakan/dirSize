# dirSize
C# Code for a simple console application that exports to a text file all folders and files on the machine it is running on and displays the total combined size of the files contained within each folder.
The text file output is ordered by folder size with the largest at the top

Code is based on How to: Iterate Through a Directory Tree (C# Programming Guide) non-recursive example
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree

Directory data is stored in datatable structure during size calculation.
Total file size for each folder is summed, then added to all corrsponding data entry for upper directories as the directory path is traversed backwards.

Directory information is written in JSON to a txt file at user specified location.
