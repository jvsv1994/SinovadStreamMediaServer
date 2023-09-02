using AutoMapper;
using Microsoft.AspNetCore.SignalR.Client;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;
using System.Diagnostics;

namespace SinovadMediaServer.Application.UseCases.MediaFilePlaybacks
{
    public class MediaFilePlaybackService : IMediaFilePlaybackService
    {
        private IUnitOfWork _unitOfWork;

        private readonly SharedData _sharedData;

        private readonly IAppLogger<MediaFilePlaybackService> _logger;

        private readonly FfmpegStrategy _ffmpegStrategy;

        public MediaFilePlaybackService(IUnitOfWork unitOfWork, SharedData sharedData, IMapper mapper, IAppLogger<MediaFilePlaybackService> logger, FfmpegStrategy ffmpegStrategy)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
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

        public Response<TranscodedMediaFileResponseDto> CreateTranscodedMediaFile(MediaFilePlaybackDto MediaFilePlayback, string clientIpAddress)
        {
            var response = new Response<TranscodedMediaFileResponseDto>();
            try
            {
                if (MediaFilePlayback.ClientData != null)
                {
                    MediaFilePlayback.ClientData.LocalIpAddress = clientIpAddress;
                }
                var physicalPath = MediaFilePlayback.ItemData.PhysicalPath;
                var timeSpan = MediaFilePlayback.ClientData.CurrentTime.ToString();
                MediaFilePlayback.StreamsData = _ffmpegStrategy.GenerateMediaFilePlaybackStreamsData(physicalPath);
                var mediaFilePlaybackTranscodingProcess = _ffmpegStrategy.ExecuteTranscodeAudioVideoAndSubtitleProcess(MediaFilePlayback.StreamsData, timeSpan);
                MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess = mediaFilePlaybackTranscodingProcess;
                _sharedData.ListMediaFilePlayback.Add(MediaFilePlayback);
                _sharedData.HubConnection.InvokeAsync("AddMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, MediaFilePlayback.Guid);
                MediaFilePlayback.ItemData.Duration = MediaFilePlayback.StreamsData.Duration;
                MediaFilePlayback.ClientData.Duration = MediaFilePlayback.StreamsData.Duration;
                var videoUrl = _sharedData.WebUrl + "/transcoded/" + mediaFilePlaybackTranscodingProcess.TranscodeFolderName + "/" + MediaFilePlayback.StreamsData.OutputTranscodedFileName;
                var transcodedFile = new TranscodedMediaFileResponseDto();
                transcodedFile.Guid = MediaFilePlayback.Guid;
                transcodedFile.Url = videoUrl;
                transcodedFile.InitialTime = MediaFilePlayback.ClientData.CurrentTime;
                transcodedFile.Duration = MediaFilePlayback.StreamsData.Duration;
                transcodedFile.ListAudioStreams = MediaFilePlayback.StreamsData.ListAudioStreams;
                transcodedFile.ListSubtitleStreams = MediaFilePlayback.StreamsData.ListSubtitleStreams;
                transcodedFile.VideoTransmissionTypeId = MediaFilePlayback.StreamsData.VideoTransmissionTypeId;
                response.Data = transcodedFile;
                response.IsSuccess = true;
                response.Message = "Successful";
            } catch (Exception ex) {
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
                    var mediaFileProfile = _unitOfWork.MediaFileProfiles.GetByExpression(x => x.MediaFileId == mediaFilePlayback.ItemData.MediaFileId && x.ProfileId == mediaFilePlayback.ProfileData.ProfileId);
                    if (mediaFileProfile != null)
                    {
                        if (mediaFilePlayback.ClientData.IsPlaying && mediaFilePlayback.ClientData.CurrentTime != mediaFileProfile.CurrentTime)
                        {
                            mediaFileProfile.CurrentTime = mediaFilePlayback.ClientData.CurrentTime;
                            _unitOfWork.MediaFileProfiles.Update(mediaFileProfile);
                            _unitOfWork.Save();
                        }
                    }else
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
            }catch (Exception ex){
                _logger.LogError(ex.StackTrace);
            }
        }

        public void DeleteOldTranscodedMediaFiles()
        {
            try
            {
                List<MediaFilePlaybackDto> listMediaFilePlaybackForDelete = new List<MediaFilePlaybackDto>();
                foreach (var MediaFilePlayback in _sharedData.ListMediaFilePlayback)
                {
                    bool forceDelete = CheckIfDeleteTranscodedFile(MediaFilePlayback.StreamsData.MediaFilePlaybackTranscodingProcess);                 
                    if (forceDelete)
                    {
                        _sharedData.HubConnection.InvokeAsync("RemoveMediaFilePlayback", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, MediaFilePlayback.Guid);
                    }
                }
            }catch (Exception ex){
                _logger.LogError(ex.StackTrace);
            }
        }

        private bool CheckIfDeleteTranscodedFile(MediaFilePlaybackTranscodingProcess mediaFilePlaybackTranscodingProcess)
        {
            var forceDelete = false;
            var currentMilisecond = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var tvpMilisecond = mediaFilePlaybackTranscodingProcess.Created.Ticks / TimeSpan.TicksPerMillisecond;
            if (currentMilisecond - tvpMilisecond > 86400000)
            {
                forceDelete = true;
            }      
            return forceDelete;
        }

        public bool KillProcessAndRemoveDirectory(MediaFilePlaybackTranscodingProcess mediaFilePlaybackTranscodingProcess)
        {
            var killAndRemoveCompleted = false;
            try
            {
                if (mediaFilePlaybackTranscodingProcess.TranscodeAudioVideoProcessId != null)
                {
                    KillProcess((int)mediaFilePlaybackTranscodingProcess.TranscodeAudioVideoProcessId);
                    System.Threading.Thread.Sleep(1000);
                }
                if (mediaFilePlaybackTranscodingProcess.TranscodeSubtitlesProcessId != null)
                {
                    KillProcess((int)mediaFilePlaybackTranscodingProcess.TranscodeSubtitlesProcessId);
                    System.Threading.Thread.Sleep(1000);
                }
                if (System.IO.Directory.Exists(mediaFilePlaybackTranscodingProcess.TranscodeFolderPath))
                {
                    System.IO.Directory.Delete(mediaFilePlaybackTranscodingProcess.TranscodeFolderPath, true);
                }
                killAndRemoveCompleted = true;
            }catch (Exception ex){
               _logger.LogError(ex.StackTrace);
            }
            return killAndRemoveCompleted;
        }


        private void KillProcess(int processId)
        {
            try
            {
                var proc = Process.GetProcessById(processId);
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
                }catch (Exception ex){
                    _logger.LogError(ex.StackTrace);

                }
            }catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

    }
}
