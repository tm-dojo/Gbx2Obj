using GBX.NET;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;


Console.WriteLine("Please input the absolute path of the Trackmania\\Items folder:");
string blocksPath = Console.ReadLine();

if (!Directory.Exists(blocksPath))
{
    Console.WriteLine($"The folder {blocksPath} does not exist");
    return;
}

createLocalFolders(blocksPath);
convertGbxToObj(blocksPath);

string removeBlockExtension(string blockPath)
{
    return blockPath.Replace(".Item.Gbx", "");
}

string getLocalPathFromItemFolder(string itemFolderPath)
{
    // Convert the absolution path of the item folder (from the game's directory) to local path (Gbx2Obj/bin/Debug/net7.0/)
    string localFolder = itemFolderPath.Replace(blocksPath, "");

    if (localFolder.StartsWith("/") || localFolder.StartsWith("\\"))
    {
        localFolder = localFolder.Substring(1);
    }
    localFolder = "./" + localFolder;
    return localFolder;
}

void createLocalFolders(string blocksPath)
{
    // Browse through the game Items folders recursively, create same arborescence locally (Gbx2Obj/bin/Debug/net7.0/)

    List<string> folders = Directory.EnumerateDirectories(blocksPath).ToList();

    foreach (string folder in folders)
    {
        string localFolder = getLocalPathFromItemFolder(folder);
        if (!Directory.Exists(localFolder))
        {
            Console.WriteLine($"Creating local folder {localFolder}");
            Directory.CreateDirectory(localFolder);
        }
        createLocalFolders(folder);
    }
}

void ExportCrystalObj(CPlugCrystal crystal, string filePath)
{
    FileStream objStream = new FileStream(filePath + ".obj", FileMode.OpenOrCreate);
    FileStream mtlStream = new FileStream(filePath + ".mtl", FileMode.OpenOrCreate);

    crystal.ExportToObj(objStream, mtlStream);
}
void getBlockMesh(string blockPath)
{
    string outputObjPath = getLocalPathFromItemFolder(blockPath);
    outputObjPath = removeBlockExtension(outputObjPath);

    Console.WriteLine($"outputObjPath {outputObjPath}");

    try
    {
        GameBox<CGameItemModel> gbx = GameBox.Parse<CGameItemModel>(blockPath);

        if (gbx.Node is CGameItemModel)
        {
            CGameItemModel item = gbx.Node;

            if (item.BlockModel != null)
            {
                foreach (KeyValuePair<int, CPlugCrystal> variant in item.BlockModel.CustomizedVariants)
                {
                    CPlugCrystal crystal = variant.Value;

                    if (crystal != null)
                    {
                        ExportCrystalObj(crystal, outputObjPath);
                    }
                    else
                    {
                        Console.WriteLine("File with BlockModel (variant " + variant.Key + ") has no Crystal: \t" + outputObjPath);
                    }
                }
            }
            else if (item.ItemModel != null)
            {
                CPlugCrystal crystal = item.ItemModel.MeshCrystal;

                if (crystal != null)
                {
                    ExportCrystalObj(crystal, outputObjPath);
                }
                else
                {
                    Console.WriteLine("File with ItemModel contained no Crystal: " + blockPath);
                }
            }
            else
            {
                Console.WriteLine("File contained no BlockModel or ItemModel: " + blockPath);
            }
        }
    }
    catch(Exception e)
    {
        Console.WriteLine($"Error converting {blockPath}: {e}");
    }
}

void convertGbxToObj(string blocksPath)
{
    // Browse through the game Items folders recursively, convert each .Item.Gbx file to .obj file (output is at Gbx2Obj/bin/Debug/net7.0/)
    List<string> folderFiles = Directory.EnumerateFiles(blocksPath).ToList();

    if (folderFiles.Count > 0 &&
        (folderFiles.Any(file => file.EndsWith(".Item.Gbx")) || folderFiles.Any(file => file.EndsWith(".Solid.Gbx")))
        )
    {
        List<string> gbxFiles = folderFiles.FindAll(file => file.EndsWith(".Item.Gbx") || file.EndsWith(".Solid.Gbx"));
        Console.WriteLine($"Found {gbxFiles.Count} files to convert in {blocksPath}");

        foreach (string file in gbxFiles)
        {
            //if (!file.Contains("RoadTechDiagRightStartCurve1In"))
            //    continue;
            string localFilePath = getLocalPathFromItemFolder(file);
            if (!File.Exists(localFilePath))
            {
                string[] fileSplit = file.Split("\\");
                string fileName = fileSplit[fileSplit.Length - 1];
                Console.WriteLine($"Convert {removeBlockExtension(fileName)}");
                getBlockMesh(file);
            }
        }
    }

    List<string> folderSubFolders = Directory.EnumerateDirectories(blocksPath).ToList();
    if (folderSubFolders.Count > 0)
    {
        foreach (string subFolder in folderSubFolders)
        {
            convertGbxToObj(subFolder);
        }
    }
}