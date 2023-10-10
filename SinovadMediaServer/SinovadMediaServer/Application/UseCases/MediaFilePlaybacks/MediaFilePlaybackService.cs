using AutoMapper;
using Microsoft.AspNetCore.SignalR.Client;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Application.Shared;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;

namespace SinovadMediaServer.Application.UseCases.MediaFilePlaybacks
{
    public class MediaFilePlaybackService : IMediaFilePlaybackService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        private readonly IAppLogger<MediaFilePlaybackService> _logger;

        private readonly FfmpegStrategy _ffmpegStrategy;

        public MediaFilePlaybackService(IUnitOfWork unitOfWork, SharedData sharedData, SharedService sharedService, IAppLogger<MediaFilePlaybackService> logger, FfmpegStrategy ffmpegStrategy)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _sharedService= sharedService;
            _logger = logger;
            _ffmpegStrategy = ffmpegStrategy;
        }

        public Response<List<MediaFilePlaybackDto>> GetListMediaFilePlayback()
        {
            var response = new Response<List<MediaFilePlaybackDto>>();
            try
            {
                response.Data = _sharedData.ListMediaFilePlayback;
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

        public async Task<Response<TranscodedMediaFileResponseDto>> CreateTranscodedMediaFile(MediaFilePlaybackDto mediaFilePlayback, string clientIpAddress)
        {
            var response = new Response<TranscodedMediaFileResponseDto>();
            try
            {
                if (mediaFilePlayback.ClientData != null)
                {
                    mediaFilePlayback.ClientData.LocalIpAddress = clientIpAddress;
                }
                var listMediaFilePlayback=_sharedData.ListMediaFilePlayback.Where(mfp => mfp.ClientData.LocalIpAddress == clientIpAddress).ToList();
                if(listMediaFilePlayback!=null && listMediaFilePlayback.Count>0)
                {
                    foreach (var mfpb in listMediaFilePlayback)
                    {
                        _sharedService.KillProcessAndRemoveDirectory(mfpb.StreamsData.MediaFilePlaybackTranscodingProcess);
                        _sharedData.ListMediaFilePlayback.RemoveAll(x => x.Guid == mfpb.Guid);
                        await _sharedData.HubConnection.SendAsync("RemovedMediaFilePlayBack", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mfpb.Guid);
                    }
                }          
                var physicalPath = mediaFilePlayback.ItemData.PhysicalPath;
                var timeSpan = mediaFilePlayback.ClientData.CurrentTime.ToString();
                mediaFilePlayback.StreamsData = _ffmpegStrategy.GenerateMediaFilePlaybackStreamsData(physicalPath);
                var mediaFilePlaybackTranscodingProcess = _ffmpegStrategy.ExecuteTranscodeAudioVideoAndSubtitleProcess(mediaFilePlayback.StreamsData, timeSpan);
                mediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess = mediaFilePlaybackTranscodingProcess;
                _sharedData.ListMediaFilePlayback.Add(mediaFilePlayback);
                await _sharedData.HubConnection.InvokeAsync("AddedMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlayback.Guid);
                mediaFilePlayback.ItemData.Duration = mediaFilePlayback.StreamsData.Duration;
                mediaFilePlayback.ClientData.Duration = mediaFilePlayback.StreamsData.Duration;
                mediaFilePlayback.Created=DateTime.Now;
                var videoUrl = _sharedData.WebUrl + "/transcoded/" + mediaFilePlaybackTranscodingProcess.TranscodeFolderName + "/" + mediaFilePlayback.StreamsData.OutputTranscodedFileName;
                var transcodedFile = new TranscodedMediaFileResponseDto();
                transcodedFile.Guid = mediaFilePlayback.Guid;
                transcodedFile.Url = videoUrl;
                transcodedFile.InitialTime = mediaFilePlayback.ClientData.CurrentTime;
                transcodedFile.Duration = mediaFilePlayback.StreamsData.Duration;
                transcodedFile.ListAudioStreams = mediaFilePlayback.StreamsData.ListAudioStreams;
                transcodedFile.ListSubtitleStreams = mediaFilePlayback.StreamsData.ListSubtitleStreams;
                transcodedFile.VideoTransmissionTypeId = mediaFilePlayback.StreamsData.VideoTransmissionTypeId;
                response.Data = transcodedFile;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex) {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<string> RetranscodeMediaFile(RetranscodeMediaFileRequestDto retranscodeVideoRequest)
        {
            var response = new Response<string>();
            try
            {
                var MediaFilePlayback = _sharedData.ListMediaFilePlayback.Where(x => x.Guid == retranscodeVideoRequest.Guid).FirstOrDefault();
                if (MediaFilePlayback != null)
                {
                    var timeSpan = retranscodeVideoRequest.NewTime.ToString();
                    var mediaFilePlaybackTranscodingProcess = _ffmpegStrategy.ExecuteTranscodeAudioVideoAndSubtitleProcess(MediaFilePlayback.StreamsData, timeSpan);
                    MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess=mediaFilePlaybackTranscodingProcess;
                    var videoUrl = _sharedData.WebUrl + "/transcoded/" + mediaFilePlaybackTranscodingProcess.TranscodeFolderName + "/" + MediaFilePlayback.StreamsData.OutputTranscodedFileName;
                    response.Data = videoUrl;
                } else {
                    throw new Exception("Transcoded Video Not Found");
                }
                response.IsSuccess = true;
                response.Message = "Successful";
            } catch (Exception ex) {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }
        public void UpdateAllMediaFileProfile()
        {
            try
            {
                var mediaFilePlaybacks = _sharedData.ListMediaFilePlayback;
                foreach (var mediaFilePlayback in mediaFilePlaybacks)
                {
                    if (mediaFilePlayback.ClientData.IsPlaying)
                    {
                        var mediaFileProfile = _unitOfWork.MediaFileProfiles.GetByExpression(x => x.MediaFileId == mediaFilePlayback.ItemData.MediaFileId && x.ProfileId == mediaFilePlayback.ProfileData.ProfileId);
                        if (mediaFileProfile != null)
                        {
                            if (mediaFilePlayback.ClientData.CurrentTime != mediaFileProfile.CurrentTime)
                            {
                                mediaFileProfile.CurrentTime = mediaFilePlayback.ClientData.CurrentTime;
                                _unitOfWork.MediaFileProfiles.Update(mediaFileProfile);
                                _unitOfWork.Save();
                            }
                        }
                        else
                        {
                            mediaFileProfile = new MediaFileProfile();
                            mediaFileProfile.DurationTime = mediaFilePlayback.ClientData.Duration;
                            mediaFileProfile.CurrentTime = mediaFilePlayback.ClientData.CurrentTime;
                            mediaFileProfile.Title = mediaFilePlayback.ItemData.Title;
                            mediaFileProfile.Subtitle = mediaFilePlayback.ItemData.Subtitle;
                            mediaFileProfile.MediaFileId = mediaFilePlayback.ItemData.MediaFileId;
                            mediaFileProfile.ProfileId = mediaFilePlayback.ProfileData.ProfileId;
                            _unitOfWork.MediaFileProfiles.Add(mediaFileProfile);
                            _unitOfWork.Save();
                        }
                    }
                }
            }catch (Exception ex){
                _logger.LogError(ex.StackTrace);
            }
        }


    }
}
