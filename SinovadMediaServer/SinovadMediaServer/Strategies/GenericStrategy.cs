using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using System.Diagnostics;

namespace SinovadMediaServer.Strategies
{
    public class GenericStrategy
    {
            
        private IOptions<MyConfig> _config;

        private SharedData _sharedData;

        public GenericStrategy(IOptions<MyConfig> config,SharedData sharedData)
        {
            _config = config;
            _sharedData = sharedData;
        }

        public void deleteOldTranscodeVideoProcess()
        {
            var listTranscodeVideoProcess = getListTranscodeVideoProcess();
            if (listTranscodeVideoProcess != null && listTranscodeVideoProcess.Count > 0)
            {
                performDeleteListTranscodeVideoProcess(listTranscodeVideoProcess, false);
            }
        }

        private List<TranscodeVideoProcessDto> getListTranscodeVideoProcess()
        {
            var list = new List<TranscodeVideoProcessDto>();
            if (_sharedData.hostData!=null)
            {
                var restService = new RestService<List<TranscodeVideoProcessDto>>(_config, _sharedData);
                list = restService.ExecuteHttpMethodAsync(HttpMethodType.GET, "/transcodeVideoProcesses/GetAllByAccountServerAsync/" + _sharedData.hostData.accountServerId).Result;
            }
            return list;
        }

        public List<String> performDeleteListTranscodeVideoProcess(List<TranscodeVideoProcessDto> listTranscodeVideoProcess, Boolean forceDelete)
        {
            List<String> listProcessDeletedGUIDs = new List<String>();
            for (var i = 0; i < listTranscodeVideoProcess.Count; i++)
            {
                var transcodeVideoProcess = listTranscodeVideoProcess[i];
                if (!forceDelete)
                {
                    var currentMilisecond = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    var tvpMilisecond = transcodeVideoProcess.Created.Ticks / TimeSpan.TicksPerMillisecond;
                    if (currentMilisecond - tvpMilisecond > 86400000)
                    {
                        forceDelete = true;
                    }
                }
                if (forceDelete)
                {
                    try
                    {
                        var proc = Process.GetProcessById(transcodeVideoProcess.TranscodeAudioVideoProcessId);
                        try
                        {
                            if (proc != null)
                            {
                                if (!proc.HasExited)
                                {
                                    proc.Kill();
                                    proc.Close();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    if (transcodeVideoProcess.TranscodeSubtitlesProcessId != null && transcodeVideoProcess.TranscodeSubtitlesProcessId != 0)
                    {
                        try
                        {
                            var proc = Process.GetProcessById((int)transcodeVideoProcess.TranscodeSubtitlesProcessId);
                            try
                            {
                                if (proc != null)
                                {
                                    if (!proc.HasExited)
                                    {
                                        proc.Kill();
                                        proc.Close();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        if (System.IO.Directory.Exists(transcodeVideoProcess.WorkingDirectoryPath))
                        {
                            System.IO.Directory.Delete(transcodeVideoProcess.WorkingDirectoryPath, true);
                        }
                        listProcessDeletedGUIDs.Add(transcodeVideoProcess.Guid);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            if(listProcessDeletedGUIDs.Count>0)
            {
                var guids = string.Join(",", listProcessDeletedGUIDs);
                var restService = new RestService<Object>(_config, _sharedData);
                var res= restService.ExecuteHttpMethodAsync(HttpMethodType.DELETE, "/transcodeVideoProcesses/DeleteByListGuids/" + guids).Result;
            }
            return listProcessDeletedGUIDs;
        }

    }
}
