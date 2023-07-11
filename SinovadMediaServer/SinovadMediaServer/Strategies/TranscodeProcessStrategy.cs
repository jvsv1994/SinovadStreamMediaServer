using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using System;

namespace SinovadMediaServer.Strategies
{
    public class TranscodeProcessStrategy
    {

        private IOptions<MyConfig> _config;

        private SharedData _sharedData;

        public TranscodeProcessStrategy(IOptions<MyConfig> config,SharedData sharedData)
        {
            _config = config;
            _sharedData = sharedData;
        }

        public List<String> deleteList(string guids)
        {

            var listTranscodeVideoProcess = getTranscodeVideoProcesses(guids);
            var listProcessDeletedGUIDs = performDeleteListTranscodeVideoProcess(listTranscodeVideoProcess, true);      
            return listProcessDeletedGUIDs;
        }

        public List<TranscodeVideoProcessDto> getTranscodeVideoProcesses(string guids)
        {
            var restService = new RestService<List<TranscodeVideoProcessDto>>(_config, _sharedData);
            List<TranscodeVideoProcessDto> list = restService.ExecuteHttpMethodAsync(HttpMethodType.GET, "/transcodeVideoProcesses/GetAllByListGuidsAsync/" + guids).Result;
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
            if (listProcessDeletedGUIDs.Count > 0)
            {
                var restService = new RestService<Object>(_config, _sharedData);
                var guids = string.Join(",", listProcessDeletedGUIDs);
                var res=restService.ExecuteHttpMethodAsync(HttpMethodType.DELETE, "/transcodeVideoProcesses/DeleteByListGuids/" + guids).Result;
            }
            return listProcessDeletedGUIDs;
        }

    }
}
