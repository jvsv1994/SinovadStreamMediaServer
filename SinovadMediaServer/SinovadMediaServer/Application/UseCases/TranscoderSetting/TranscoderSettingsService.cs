using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.MiddlewareInjector;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.UseCases.TranscoderSetting
{
    public class TranscoderSettingsService : ITranscoderSettingsService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedService _sharedService;

        private readonly SharedData _sharedData;

        private readonly AutoMapper.IMapper _mapper;

        private readonly MiddlewareInjectorOptions _middlewareInjectorOptions;

        public TranscoderSettingsService(IUnitOfWork unitOfWork, SharedService sharedService, SharedData sharedData, AutoMapper.IMapper mapper,MiddlewareInjectorOptions middlewareInjectorOptions)
        {
            _unitOfWork = unitOfWork;
            _sharedService = sharedService;
            _sharedData = sharedData;
            _mapper = mapper;
            _middlewareInjectorOptions = middlewareInjectorOptions;
        }
        public async Task<Response<TranscoderSettingsDto>> GetAsync()
        {
            var response = new Response<TranscoderSettingsDto>();
            try
            {
                var list = await _unitOfWork.TranscoderSettings.GetAllAsync();
                if(list!=null && list.Count()>0)
                {
                    response.Data = _mapper.Map<TranscoderSettingsDto>(list.FirstOrDefault());
                }
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task<Response<TranscoderSettingsDto>> Save(TranscoderSettingsDto transcoderSettingsDto)
        {
            var response = new Response<TranscoderSettingsDto>();
            try
            {
                var transcoderSettings = _mapper.Map<TranscoderSettings>(transcoderSettingsDto);
                if(transcoderSettings.Id!=null && transcoderSettings.Id>0)
                {
                    _unitOfWork.TranscoderSettings.Update(transcoderSettings);
                }else{
                    transcoderSettings = await _unitOfWork.TranscoderSettings.AddAsync(transcoderSettings);
                }
                await _unitOfWork.SaveAsync();
                var tsDto= _mapper.Map<TranscoderSettingsDto>(transcoderSettings);
                response.Data = tsDto;
                response.IsSuccess = true;
                response.Message = "Successful";
                if(_sharedData.TranscoderSettingsData!=null)
                {
                    _sharedData.TranscoderSettingsData = tsDto;
                }
                _middlewareInjectorOptions.InjectMiddleware(app =>
                {
                    if (_sharedData.TranscoderSettingsData != null)
                    {
                        var fileOptions = new FileServerOptions
                        {
                            FileProvider = new PhysicalFileProvider(_sharedData.TranscoderSettingsData.TemporaryFolder),
                            RequestPath = new PathString("/transcoded"),
                            EnableDirectoryBrowsing = true,
                            EnableDefaultFiles = false
                        };
                        fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;
                        app.UseFileServer(fileOptions);
                    }
                });
            }
            catch (Exception ex)
            {
                response.Message = ex.StackTrace;
                _sharedService._tracer.LogError(ex.StackTrace);
            }
            return response;
        }

    }
}
