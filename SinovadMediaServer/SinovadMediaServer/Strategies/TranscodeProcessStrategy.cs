using SinovadMediaServer.DTOs;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Proxy;
using System.Diagnostics;

namespace SinovadMediaServer.Strategies
{
    public class TranscodeProcessStrategy
    {

        private readonly RestService _restService;

        public TranscodeProcessStrategy(RestService restService)
        {
            _restService = restService;
        }

        public async Task<List<Guid>> deleteList(string guids)
        {

            var listTranscodeVideoProcess = await getTranscodeVideoProcesses(guids);
            var listProcessDeletedGUIDs = performDeleteListTranscodeVideoProcess(listTranscodeVideoProcess, true);      
            return listProcessDeletedGUIDs;
        }

        public async Task<List<TranscodingProcessDto>> getTranscodeVideoProcesses(string guids)
        {
            var list= new List<TranscodingProcessDto>();
            var res = await _restService.ExecuteHttpMethodAsync<List<TranscodingProcessDto>>(HttpMethodType.GET, "/transcodingProcesses/GetAllByListGuidsAsync/" + guids);
            if(res.IsSuccess)
            {
                list = res.Data;
            }
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
                var guids = string.Join(",", listProcessDeletedGUIDs);
                var res=_restService.ExecuteHttpMethodAsync<object>(HttpMethodType.DELETE, "/transcodingProcesses/DeleteByListGuids/" + guids).Result;
            }
            return listProcessDeletedGUIDs;
        }

    }
}
