using AutoMapper;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.UseCases.TranscoderSetting;
using SinovadMediaServer.MiddlewareInjector;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Interface;

namespace SinovadMediaServer.Application.UseCases.MediaFilePlaybacks
{
    public class MediaFilePlaybackService:IMediaFilePlaybackService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly AutoMapper.IMapper _mapper;

        private readonly MiddlewareInjectorOptions _middlewareInjectorOptions;

        private readonly IAppLogger<TranscoderSettingsService> _logger;

        public MediaFilePlaybackService(IUnitOfWork unitOfWork, SharedData sharedData, IMapper mapper, MiddlewareInjectorOptions middlewareInjectorOptions, IAppLogger<TranscoderSettingsService> logger)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _mapper = mapper;
            _middlewareInjectorOptions = middlewareInjectorOptions;
            _logger = logger;
        }

        public void CreateMediaFilePlayback(MediaFilePlaybackRealTimeDto mediaFilePlaybackRealTime)
        {
            _sharedData.ListMediaFilePlaybackRealTime.Add(mediaFilePlaybackRealTime);
        }

        public void DeleteMediaFilePlaybackByGuid(string guid)
        {
            _sharedData.ListMediaFilePlaybackRealTime.RemoveAll(x=>x.Guid.ToString()== guid);
        }

    }
}
