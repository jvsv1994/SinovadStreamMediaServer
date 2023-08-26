using SinovadMediaServer.Application.DTOs;

namespace SinovadMediaServer.Strategies
{
    public class ManageDirectoryStrategy
    {
           
        public ManageDirectoryStrategy()
        {
        }

        public async Task<List<DirectoryDto>> GetListMainDirectories()
        {
            List<DirectoryDto> listMainDirectory = new List<DirectoryDto>();
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string userName = Environment.UserName;
            var userProfileDirectory = new DirectoryDto();
            userProfileDirectory.Path = folderPath;
            userProfileDirectory.Name = userName;
            userProfileDirectory.IsMainDirectory = true;
            userProfileDirectory.ListSubdirectories = GetSubDirectories(folderPath).Result;
            listMainDirectory.Add(userProfileDirectory);
            System.IO.DriveInfo[] sdu = System.IO.DriveInfo.GetDrives();
            for (int i = 0; i < sdu.Length; i++)
            {
                var driverInfo = sdu[i];
                var driveDirectory = new DirectoryDto();
                driveDirectory.IsMainDirectory = true;
                driveDirectory.Path = driverInfo.Name;
                driveDirectory.Name = driverInfo.Name.Replace("\\", "");
                driveDirectory.ListSubdirectories = GetSubDirectories(driverInfo.Name).Result;
                listMainDirectory.Add(driveDirectory);
            }
            return listMainDirectory;
        }

        public async Task<List<DirectoryDto>> GetSubDirectories(string fullpath)
        {
            List<DirectoryDto> listSubdirectory = new List<DirectoryDto>();
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
                            var userProfileDirectory = new DirectoryDto();
                            userProfileDirectory.Path = path;
                            userProfileDirectory.Name = subdirectoryName;
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
