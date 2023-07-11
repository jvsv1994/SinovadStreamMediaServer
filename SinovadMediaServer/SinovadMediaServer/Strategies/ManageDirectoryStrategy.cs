using SinovadMediaServer.CustomModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SinovadMediaServer.Strategies
{
    public class ManageDirectoryStrategy
    {
           
        public ManageDirectoryStrategy()
        {
        }

        public async Task<List<CustomDirectory>> getListMainDirectories()
        {
            List<CustomDirectory> listMainDirectory = new List<CustomDirectory>();
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string userName = Environment.UserName;
            var userProfileDirectory = new CustomDirectory();
            userProfileDirectory.path = folderPath;
            userProfileDirectory.name = userName;
            userProfileDirectory.isMainDirectory = true;
            userProfileDirectory.listSubdirectory = getSubDirectories(folderPath).Result;
            listMainDirectory.Add(userProfileDirectory);
            System.IO.DriveInfo[] sdu = System.IO.DriveInfo.GetDrives();
            for (int i = 0; i < sdu.Length; i++)
            {
                var driverInfo = sdu[i];
                var driveDirectory = new CustomDirectory();
                driveDirectory.isMainDirectory = true;
                driveDirectory.path = driverInfo.Name;
                driveDirectory.name = driverInfo.Name.Replace("\\", "");
                driveDirectory.listSubdirectory = getSubDirectories(driverInfo.Name).Result;
                listMainDirectory.Add(driveDirectory);
            }
            return listMainDirectory;
        }

        public async Task<List<CustomDirectory>> getSubDirectories(string fullpath)
        {
            List<CustomDirectory> listSubdirectory = new List<CustomDirectory>();
            try
            {
                var directories = System.IO.Directory.GetDirectories(fullpath);
                if (directories.Length > 0)
                {
                    for (int i = 0; i < directories.Length; i++)
                    {
                        var path = directories[i];
                        var subdirectoryName = path.Replace(fullpath, "").Replace("\\", "");
                        if (!subdirectoryName.StartsWith("."))
                        {
                            var userProfileDirectory = new CustomDirectory();
                            userProfileDirectory.path = path;
                            userProfileDirectory.name = subdirectoryName;
                            listSubdirectory.Add(userProfileDirectory);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return listSubdirectory;
        }

    }
}
