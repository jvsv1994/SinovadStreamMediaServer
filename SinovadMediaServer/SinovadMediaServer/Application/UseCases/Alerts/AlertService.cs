using AutoMapper;
using Microsoft.Extensions.Logging;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.UseCases.Alerts
{
    public class AlertService : IAlertService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        private readonly ILogger<AlertService> _logger;

        public AlertService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AlertService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<bool>> Create(string description,AlertType alertType)
        {
            var response = new Response<bool>();
            try
            {
                var alertDto = new AlertDto();
                alertDto.Description = description;
                alertDto.AlertType = alertType;
                await _unitOfWork.Alerts.AddAsync(_mapper.Map<Alert>(alertDto));
                await _unitOfWork.SaveAsync();
                response.Data = true;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<ResponsePagination<List<AlertDto>>> GetAllWithPagination(int page, int take)
        {
            var response = new ResponsePagination<List<AlertDto>>();
            try
            {
                var result = await _unitOfWork.Alerts.GetAllWithPaginationAsync(page, take, "Created", "desc");
                response.Data = _mapper.Map<List<AlertDto>>(result.Items);
                response.PageNumber = page;
                response.TotalPages = result.Pages;
                response.TotalCount = result.Total;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }
    }
}
