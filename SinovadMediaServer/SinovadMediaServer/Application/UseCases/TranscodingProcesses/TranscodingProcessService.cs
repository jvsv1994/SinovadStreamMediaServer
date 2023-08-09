
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;
using System;

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

        public async Task<Response<List<TranscodingProcessDto>>> GetAllByMediaServerAsync(int mediaServerId)
        {
            var response = new Response<List<TranscodingProcessDto>>();
            try
            {
                var result = await _unitOfWork.TranscodingProcesses.GetAllByExpressionAsync(x => x.MediaServerId == mediaServerId);
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

        public Response<object> DeleteByListGuids(string guids)
        {
            var response = new Response<object>();
            try
            {
                List<Guid> listGuids = new List<Guid>();
                if (!string.IsNullOrEmpty(guids))
                {
                    listGuids = guids.Split(",").Select(x => Guid.Parse(x.ToString())).ToList();
                }
                _unitOfWork.TranscodingProcesses.DeleteByExpression(x => listGuids.Contains(x.RequestGuid));
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
    }
}
