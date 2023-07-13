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

        public List<Guid> deleteList(string guids)
        {

            var listTranscodeVideoProcess = getTranscodeVideoProcesses(guids);
            var listProcessDeletedGUIDs = performDeleteListTranscodeVideoProcess(listTranscodeVideoProcess, true);      
            return listProcessDeletedGUIDs;
        }

        public List<TranscodingProcessDto> getTranscodeVideoProcesses(string guids)
        {
            var restService = new RestService<List<TranscodingProcessDto>>(_config, _sharedData);
            List<TranscodingProcessDto> list = restService.ExecuteHttpMethodAsync(HttpMethodType.GET, "/transcodingProcesses/GetAllByListGuidsAsync/" + guids).Result;
            return list;
        }

        public List<Guid> performDeleteListTranscodeVideoProcess(List<TranscodingProcessDto> listTranscodeVideoProcess, Boolean forceDelete)
        {
            List<Guid> listProcessDeletedGUIDs = new List<Guid>();
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
                        var proc = Process.GetProcessById(transcodeVideoProcess.SystemProcessIdentifier);
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
                    if (transcodeVideoProcess.AdditionalSystemProcessIdentifier != null && transcodeVideoProcess.AdditionalSystemProcessIdentifier != 0)
                    {
                        try
                        {
                            var proc = Process.GetProcessById((int)transcodeVideoProcess.AdditionalSystemProcessIdentifier);
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
                        if (System.IO.Directory.Exists(transcodeVideoProcess.GeneratedTemporaryFolder))
                        {
                            System.IO.Directory.Delete(transcodeVideoProcess.GeneratedTemporaryFolder, true);
                        }
                        listProcessDeletedGUIDs.Add(transcodeVideoProcess.RequestGuid);
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
                var res=restService.ExecuteHttpMethodAsync(HttpMethodType.DELETE, "/transcodingProcesses/DeleteByListGuids/" + guids).Result;
            }
            return listProcessDeletedGUIDs;
        }

    }
}
