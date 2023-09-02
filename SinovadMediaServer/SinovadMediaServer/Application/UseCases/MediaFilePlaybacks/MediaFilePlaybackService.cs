﻿using AutoMapper;
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

        public Response<List<MediaFilePlaybackRealTimeDto>> GetListMediaFilePlaybackRealTime()
        {
            var response = new Response<List<MediaFilePlaybackRealTimeDto>>();
            try
            {
                response.Data = _sharedData.ListMediaFilePlaybackRealTime;
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

        public Response<TranscodedMediaFileResponseDto> CreateTranscodedMediaFile(MediaFilePlaybackRealTimeDto mediaFilePlaybackRealTime, string clientIpAddress)
        {
            var response = new Response<TranscodedMediaFileResponseDto>();
            try
            {
                if (mediaFilePlaybackRealTime.ClientData != null)
                {
                    mediaFilePlaybackRealTime.ClientData.LocalIpAddress = clientIpAddress;
                }
                var physicalPath = mediaFilePlaybackRealTime.ItemData.PhysicalPath;
                var timeSpan = mediaFilePlaybackRealTime.ClientData.CurrentTime.ToString();
                mediaFilePlaybackRealTime.StreamsData = _ffmpegStrategy.GenerateMediaFilePlaybackStreamsData(physicalPath);
                var mediaFilePlaybackTranscodingProcess = _ffmpegStrategy.ExecuteTranscodeAudioVideoAndSubtitleProcess(mediaFilePlaybackRealTime.StreamsData, timeSpan);
                mediaFilePlaybackRealTime.StreamsData.MediaFilePlaybackTranscodingProcess = mediaFilePlaybackTranscodingProcess;
                _sharedData.ListMediaFilePlaybackRealTime.Add(mediaFilePlaybackRealTime);
                _sharedData.HubConnection.InvokeAsync("AddMediaFilePlayBackRealTime", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlaybackRealTime.Guid);
                mediaFilePlaybackRealTime.ItemData.Duration = mediaFilePlaybackRealTime.StreamsData.Duration;
                mediaFilePlaybackRealTime.ClientData.Duration = mediaFilePlaybackRealTime.StreamsData.Duration;
                var videoUrl = _sharedData.WebUrl + "/transcoded/" + mediaFilePlaybackTranscodingProcess.TranscodeFolderName + "/" + mediaFilePlaybackRealTime.StreamsData.OutputTranscodedFileName;
                var transcodedFile = new TranscodedMediaFileResponseDto();
                transcodedFile.Guid = mediaFilePlaybackRealTime.Guid;
                transcodedFile.Url = videoUrl;
                transcodedFile.InitialTime = mediaFilePlaybackRealTime.ClientData.CurrentTime;
                transcodedFile.Duration = mediaFilePlaybackRealTime.StreamsData.Duration;
                transcodedFile.ListAudioStreams = mediaFilePlaybackRealTime.StreamsData.ListAudioStreams;
                transcodedFile.ListSubtitleStreams = mediaFilePlaybackRealTime.StreamsData.ListSubtitleStreams;
                transcodedFile.VideoTransmissionTypeId = mediaFilePlaybackRealTime.StreamsData.VideoTransmissionTypeId;
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
                var mediaFilePlaybackRealTime = _sharedData.ListMediaFilePlaybackRealTime.Where(x => x.Guid == retranscodeVideoRequest.Guid).FirstOrDefault();
                if (mediaFilePlaybackRealTime != null)
                {
                    var timeSpan = retranscodeVideoRequest.NewTime.ToString();
                    var mediaFilePlaybackTranscodingProcess = _ffmpegStrategy.ExecuteTranscodeAudioVideoAndSubtitleProcess(mediaFilePlaybackRealTime.StreamsData, timeSpan);
                    mediaFilePlaybackRealTime.StreamsData.MediaFilePlaybackTranscodingProcess=mediaFilePlaybackTranscodingProcess;
                    var videoUrl = _sharedData.WebUrl + "/transcoded/" + mediaFilePlaybackTranscodingProcess.TranscodeFolderName + "/" + mediaFilePlaybackRealTime.StreamsData.OutputTranscodedFileName;
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
                var mediaFilePlaybacks = _sharedData.ListMediaFilePlaybackRealTime;
                foreach (var mediaFilePlayback in mediaFilePlaybacks)
                {
                    var mediaFileProfile = _unitOfWork.MediaFilePlaybacks.GetByExpression(x => x.MediaFileId == mediaFilePlayback.ItemData.MediaFileId && x.ProfileId == mediaFilePlayback.ProfileData.ProfileId);
                    if (mediaFileProfile != null)
                    {
                        if (mediaFilePlayback.ClientData.IsPlaying && mediaFilePlayback.ClientData.CurrentTime != mediaFileProfile.CurrentTime)
                        {
                            mediaFileProfile.CurrentTime = mediaFilePlayback.ClientData.CurrentTime;
                            _unitOfWork.MediaFilePlaybacks.Update(mediaFileProfile);
                            _unitOfWork.Save();
                        }
                    }else
                    {
                        mediaFileProfile = new MediaFilePlayback();
                        mediaFileProfile.DurationTime = mediaFilePlayback.ClientData.Duration;
                        mediaFileProfile.CurrentTime = mediaFilePlayback.ClientData.CurrentTime;
                        mediaFileProfile.Title = mediaFilePlayback.ItemData.Title;
                        mediaFileProfile.Subtitle = mediaFilePlayback.ItemData.Subtitle;
                        mediaFileProfile.MediaFileId = mediaFilePlayback.ItemData.MediaFileId;
                        mediaFileProfile.ProfileId = mediaFilePlayback.ProfileData.ProfileId;
                        _unitOfWork.MediaFilePlaybacks.Add(mediaFileProfile);
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
                List<MediaFilePlaybackRealTimeDto> listMediaFilePlaybackForDelete = new List<MediaFilePlaybackRealTimeDto>();
                foreach (var mediaFilePlaybackRealTime in _sharedData.ListMediaFilePlaybackRealTime)
                {
                    bool forceDelete = CheckIfDeleteTranscodedFile(mediaFilePlaybackRealTime.StreamsData.MediaFilePlaybackTranscodingProcess);                 
                    if (forceDelete)
                    {
                        _sharedData.HubConnection.InvokeAsync("RemoveMediaFilePlayBackRealTime", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlaybackRealTime.Guid);
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
