using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.UseCases.MediaFilePlaybacks;
using SinovadMediaServer.Transversal.Interface;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SinovadMediaServer.Application.Shared
{
    public class SharedService
    {
        private readonly IAppLogger<SharedService> _logger;

        public SharedService(IAppLogger<SharedService> logger)
        {
            _logger = logger;
        }

        public string GetFormattedText(string input)
        {
            return Simplify(input).Replace(" ", "").Replace(".", "").Replace(":", "").Replace(",", "").Replace("-", "").Replace("!", "").Replace("¡", "").Replace("?", "").Replace("¿", "").Trim().ToUpper();
        }

        private static string Simplify(string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public bool KillProcessAndRemoveDirectory(MediaFilePlaybackTranscodingProcess mediaFilePlaybackTranscodingProcess)
        {
            var killAndRemoveCompleted = false;
            try
            {
                if (mediaFilePlaybackTranscodingProcess.TranscodeAudioVideoProcessId != null)
                {
                    KillProcess((int)mediaFilePlaybackTranscodingProcess.TranscodeAudioVideoProcessId);
                }
                if (mediaFilePlaybackTranscodingProcess.TranscodeSubtitlesProcessId != null)
                {
                    KillProcess((int)mediaFilePlaybackTranscodingProcess.TranscodeSubtitlesProcessId);
                }
                if (System.IO.Directory.Exists(mediaFilePlaybackTranscodingProcess.TranscodeFolderPath))
                {
                    System.IO.Directory.Delete(mediaFilePlaybackTranscodingProcess.TranscodeFolderPath, true);
                }
                killAndRemoveCompleted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
            return killAndRemoveCompleted;
        }


        private void KillProcess(int processId)
        {
            try
            {
                var proc = GetProcess(processId);
                if (proc != null)
                {
                    try 
                    {
                        proc.Kill();
                    }catch (Exception e){}
                    try{
                        proc.Close();
                    }catch (Exception e){}
                }
            }catch (Exception ex){
                _logger.LogError(ex.StackTrace);
            }
        }

        private Process GetProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                return process;
            }catch (Exception ex){
                return null;
            }
        }

    }
}
