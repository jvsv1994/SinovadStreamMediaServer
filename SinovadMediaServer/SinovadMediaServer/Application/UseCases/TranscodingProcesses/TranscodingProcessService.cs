
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;
using System.Diagnostics;

namespace SinovadMediaServer.Application.UseCases.TranscodingProcesses
{
    public class TranscodingProcessService : ITranscodingProcessService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedService _sharedService;

        public TranscodingProcessService(IUnitOfWork unitOfWork, SharedService sharedService)
        {
            _unitOfWork = unitOfWork;
            _sharedService = sharedService;
        }

        public async Task<Response<TranscodingProcessDto>> GetAsync(int id)
        {
            var response = new Response<TranscodingProcessDto>();
            try
            {
                var result = await _unitOfWork.TranscodingProcesses.GetAsync(id);
                response.Data = result.MapTo<TranscodingProcessDto>();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<List<TranscodingProcessDto>>> GetAllAsync()
        {
            var response = new Response<List<TranscodingProcessDto>>();
            try
            {
                var result = await _unitOfWork.TranscodingProcesses.GetAllAsync();
                response.Data = result.MapTo<List<TranscodingProcessDto>>();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<List<TranscodingProcessDto>>> GetAllByListGuidsAsync(string guids)
        {
            var response = new Response<List<TranscodingProcessDto>>();
            try
            {
                List<Guid> listGuids = new List<Guid>();
                if (!string.IsNullOrEmpty(guids))
                {
                    listGuids = guids.Split(",").Select(x => Guid.Parse(x.ToString())).ToList();
                }
                var result = await _unitOfWork.TranscodingProcesses.GetAllByExpressionAsync(x => listGuids.Contains(x.RequestGuid));
                response.Data = result.MapTo<List<TranscodingProcessDto>>();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<object> Create(TranscodingProcessDto transcodingProcessDto)
        {
            var response = new Response<object>();
            try
            {
                var transcodingProcess = transcodingProcessDto.MapTo<TranscodingProcess>();
                _unitOfWork.TranscodingProcesses.Add(transcodingProcess);
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<object> CreateList(List<TranscodingProcessDto> listTranscodingProcessDto)
        {
            var response = new Response<object>();
            try
            {
                var transcodingProcess = listTranscodingProcessDto.MapTo<List<TranscodingProcess>>();
                _unitOfWork.TranscodingProcesses.AddList(transcodingProcess);
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<object> Update(TranscodingProcessDto transcodingProcessDto)
        {
            var response = new Response<object>();
            try
            {
                var transcodingProcess = transcodingProcessDto.MapTo<TranscodingProcess>();
                _unitOfWork.TranscodingProcesses.Update(transcodingProcess);
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<object> Delete(int id)
        {
            var response = new Response<object>();
            try
            {
                _unitOfWork.TranscodingProcesses.Delete(id);
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<object> DeleteList(string ids)
        {
            var response = new Response<object>();
            try
            {
                List<int> listIds = new List<int>();
                if (!string.IsNullOrEmpty(ids))
                {
                    listIds = ids.Split(",").Select(x => Convert.ToInt32(x)).ToList();
                }
                _unitOfWork.TranscodingProcesses.DeleteByExpression(x => listIds.Contains(x.Id));
                _unitOfWork.Save();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<List<Guid>> DeleteByListGuids(string guids)
        {
            var response = new Response<List<Guid>>();
            try
            {
                List<Guid> listGuids = new List<Guid>();
                if (!string.IsNullOrEmpty(guids))
                {
                    listGuids = guids.Split(",").Select(x => Guid.Parse(x.ToString())).ToList();
                }

                var listTranscodingProcess = _unitOfWork.TranscodingProcesses.GetAllByExpression(x=> listGuids.Contains(x.RequestGuid));
                var listProcessDeletedGUIDs = PerformDeleteListTranscodeVideoProcess(listTranscodingProcess.MapTo<List<TranscodingProcessDto>>(), true).Result;
                if(listProcessDeletedGUIDs!=null && listProcessDeletedGUIDs.Count()>0)
                {
                    _unitOfWork.TranscodingProcesses.DeleteByExpression(x => listProcessDeletedGUIDs.Contains(x.RequestGuid));
                    _unitOfWork.Save();
                }
                response.Data = listProcessDeletedGUIDs;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<bool> DeleteOldTranscodeVideoProcess()
        {
            var listTranscodeVideoProcess = await _unitOfWork.TranscodingProcesses.GetAllAsync();
            if (listTranscodeVideoProcess != null && listTranscodeVideoProcess.Count() > 0)
            {
                await PerformDeleteListTranscodeVideoProcess(listTranscodeVideoProcess.MapTo<List<TranscodingProcessDto>>(), false);
            }
            return true;
        }

        private async Task<List<Guid>> PerformDeleteListTranscodeVideoProcess(List<TranscodingProcessDto> listTranscodeVideoProcess, Boolean forceDelete)
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
                _unitOfWork.TranscodingProcesses.DeleteByExpression(x=> listProcessDeletedGUIDs.Contains(x.RequestGuid));
                _unitOfWork.Save();
            }
            return listProcessDeletedGUIDs;
        }


    }
}
