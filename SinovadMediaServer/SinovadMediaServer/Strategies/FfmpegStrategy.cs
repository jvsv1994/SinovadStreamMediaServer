using System.Diagnostics;

namespace SinovadMediaServer.Strategies
{
    public class FfmpegStrategy
    {
        public FfmpegStrategy()
        {

        }

        public void GenerateThumbnailByPhysicalPath(string folderName,string physicalPath)
        {
            var mediaAnalysis = FFMpegCore.FFProbe.Analyse(physicalPath);
            var position = mediaAnalysis.Duration.Divide(2).ToString();
            string exe = "ffmpeg.exe";
            var physicalPathInQuotes = "\"" + physicalPath + "\"";
            string arguments = "-ss "+ position + " -i "+ physicalPathInQuotes + " -frames:v 1 thumbnail.png";
            var rootPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sinovad Media Server");
            var mediaPath = Path.Combine(rootPath, "Media");
            var folderPath=Path.Combine(mediaPath, folderName);
            System.IO.Directory.CreateDirectory(folderPath);
            ProcessStartInfo procStartIfo = new ProcessStartInfo();
            procStartIfo.WorkingDirectory = folderPath;
            procStartIfo.FileName = exe;
            procStartIfo.Arguments = arguments;
            procStartIfo.UseShellExecute = false;
            procStartIfo.CreateNoWindow = true;
            try
            {
                var process = new Process();
                process.StartInfo = procStartIfo;
                process.EnableRaisingEvents = true;
                process.Start();
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }
}
