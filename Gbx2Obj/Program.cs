Console.WriteLine("Please input the absolute path of the Trackmania\\Items folder:");
string blocksPath = Console.ReadLine();

if (!Directory.Exists(blocksPath))
{
    Console.WriteLine($"The folder {blocksPath} does not exist");
    return;
}

getAllFoldersWithGbxFiles(blocksPath);

List<string> getAllFoldersWithGbxFiles(string blocksPath)
{
    List<string> folders = Directory.EnumerateDirectories(blocksPath).ToList();

    foreach (string folder in folders)
    {
        Console.WriteLine(folder);
    }

    return folders;
}