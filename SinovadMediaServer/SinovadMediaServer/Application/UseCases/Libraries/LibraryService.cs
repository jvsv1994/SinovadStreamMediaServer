using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.DTOs.Library;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;
using System.Linq.Expressions;

namespace SinovadMediaServer.Application.UseCases.Libraries
{
    public class LibraryService : ILibraryService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly AutoMapper.IMapper _mapper;

        private readonly IAppLogger<LibraryService> _logger;

        private readonly IAlertService _alertService;

        private readonly FfmpegStrategy _ffmpegStrategy;

        private readonly SharedService _sharedService;

        public LibraryService(IUnitOfWork unitOfWork, SharedData sharedData,IMapper mapper, IAppLogger<LibraryService> logger, IAlertService alertService, FfmpegStrategy ffmpegStrategy, SharedService sharedService)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _mapper = mapper;
            _logger = logger;
            _alertService = alertService;
            _ffmpegStrategy = ffmpegStrategy;
            _sharedService = sharedService;
        }

        public async Task<Response<LibraryDto>> GetAsync(int id)
        {
            var response = new Response<LibraryDto>();
            try
            {
                var result = await _unitOfWork.Libraries.GetAsync(id);
                response.Data = _mapper.Map<LibraryDto>(result);
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

        public async Task<Response<List<LibraryDto>>> GetAllAsync()
        {
            var response = new Response<List<LibraryDto>>();
            try
            {
                var result = await _unitOfWork.Libraries.GetAllAsync();
                response.Data = _mapper.Map<List<LibraryDto>>(result);
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

        public async Task<Response<LibraryDto>> CreateAsync(LibraryCreationDto libraryCreationDto)
        {
            var response = new Response<LibraryDto>();
            try
            {
               var library = _mapper.Map<Library>(libraryCreationDto);
               library.MediaTypeCatalogId = (int)Catalog.MediaType;
               library=await _unitOfWork.Libraries.AddAsync(library);
               await _unitOfWork.SaveAsync();
               response.Data = _mapper.Map<LibraryDto>(library);
               response.IsSuccess = true;
               response.Message = "Successful";
               await _sharedData.HubConnection.InvokeAsync("UpdateLibrariesByMediaServer", _sharedData.MediaServerData.Guid);
            }catch (Exception ex){
               response.Message = ex.Message;
               _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<object>> UpdateAsync(int id,LibraryCreationDto libraryCreationDto)
        {
            var response = new Response<object>();
            try
            {
               var library=await _unitOfWork.Libraries.GetAsync(id);
               library = _mapper.Map(libraryCreationDto, library);
               _unitOfWork.Libraries.Update(library);
               await _unitOfWork.SaveAsync();
               response.IsSuccess = true;
               response.Message = "Successful";
               await _sharedData.HubConnection.InvokeAsync("UpdateLibrariesByMediaServer", _sharedData.MediaServerData.Guid);
            }catch (Exception ex){
               response.Message = ex.Message;
               _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<object>> DeleteAsync(int id)
        {
            var response = new Response<object>();
            try
            {
                Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete = x => x.LibraryId==id;
                _sharedData.ListMediaFiles.RemoveAll(x => x.LibraryId == id);
                DeleteMediaFilesByExpresion(expresionMediaFilesToDelete);
                _unitOfWork.Libraries.Delete(id);
                await _unitOfWork.SaveAsync();
                response.IsSuccess = true;
                response.Message = "Successful";
                await _sharedData.HubConnection.InvokeAsync("UpdateLibrariesByMediaServer", _sharedData.MediaServerData.Guid);
            }catch (Exception ex){
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        private void DeleteMediaFilesByExpresion(Expression<Func<MediaFile, bool>> expresionMediaFilesToDelete)
        {
            var listMediaFilesToDelete = _unitOfWork.MediaFiles.GetAllByExpression(expresionMediaFilesToDelete).ToList();
            if (listMediaFilesToDelete.Count > 0)
            {
                _alertService.Create("Eliminando archivos en las rutas " + string.Join(",", listMediaFilesToDelete.Select(x => x.PhysicalPath)), AlertType.Bin);
                List<string> listIdsVideosDelete = listMediaFilesToDelete.Select(o => o.Id.ToString()).ToList();
                List<MediaFilePlaybackDto> listMediaFilePlaybackForDelete = _sharedData.ListMediaFilePlayback.Where(x => listIdsVideosDelete.Contains(x.ItemData.MediaFileId.ToString())).ToList();
                foreach (var mediaFilePlayback in listMediaFilePlaybackForDelete)
                {
                    _sharedService.KillProcessAndRemoveDirectory(mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);
                    _sharedData.ListMediaFilePlayback.Remove(mediaFilePlayback);
                    _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                }
                Expression<Func<MediaFileProfile, bool>> expressionVideoProfilesToDelete = x => listIdsVideosDelete.Contains(x.MediaFileId.ToString());
                _unitOfWork.MediaFileProfiles.DeleteByExpression(expressionVideoProfilesToDelete);
                _unitOfWork.MediaFiles.DeleteList(listMediaFilesToDelete);
                _unitOfWork.Save();
            }
        }


    }
}
