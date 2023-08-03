using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using System.Diagnostics;

namespace SinovadMediaServer.Strategies
{
    public class TranscodeProcessStrategy
    {

        private readonly SharedData _sharedData;

        private readonly RestService _restService;

        public TranscodeProcessStrategy(SharedData sharedData, RestService restService)
        {
            _sharedData = sharedData;
            _restService = restService;
        }

        public async Task<bool> DeleteOldTranscodeVideoProcess()
        {
            var listTranscodeVideoProcess = await GetTranscodingProcessesByMediaServer();
            if (listTranscodeVideoProcess != null && listTranscodeVideoProcess.Count > 0)
            {
                await PerformDeleteListTranscodeVideoProcess(listTranscodeVideoProcess, false);
            }
            return true;
        }

        private async Task<List<TranscodingProcessDto>> GetTranscodingProcessesByMediaServer()
        {
            var list = new List<TranscodingProcessDto>();
            if (_sharedData.MediaServerData != null)
            {
                var response = await _restService.ExecuteHttpMethodAsync<List<TranscodingProcessDto>>(HttpMethodType.GET, "/transcodingProcesses/GetAllByMediaServerAsync/" + _sharedData.MediaServerData.Id);
                if (response.IsSuccess)
                {
                    list = response.Data;
                }
            }
            return list;
        }

        public async Task<List<Guid>> DeleteListByGuids(string guids)
        {

            var listTranscodeVideoProcess = await GetTranscodingProcessesByListGuids(guids);
            var listProcessDeletedGUIDs = await PerformDeleteListTranscodeVideoProcess(listTranscodeVideoProcess, true);      
            return listProcessDeletedGUIDs;
        }

        public async Task<List<TranscodingProcessDto>> GetTranscodingProcessesByListGuids(string guids)
        {
            var list= new List<TranscodingProcessDto>();
            var res = await _restService.ExecuteHttpMethodAsync<List<TranscodingProcessDto>>(HttpMethodType.GET, "/transcodingProcesses/GetAllByListGuidsAsync/" + guids);
            if(res.IsSuccess)
            {
                list = res.Data;
            }
            return list;
        }

        public async Task<List<Guid>> PerformDeleteListTranscodeVideoProcess(List<TranscodingProcessDto> listTranscodeVideoProcess, Boolean forceDelete)
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
                    System.Threading.Thread.Sleep(1000);
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
                var guids = string.Join(",", listProcessDeletedGUIDs);
                await _restService.ExecuteHttpMethodAsync<object>(HttpMethodType.DELETE, "/transcodingProcesses/DeleteByListGuids/" + guids);
            }
            return listProcessDeletedGUIDs;
        }

    }
}
