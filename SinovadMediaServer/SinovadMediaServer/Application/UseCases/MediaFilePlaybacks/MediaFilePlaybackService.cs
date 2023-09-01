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
using SinovadMediaServer.Transversal.Mapping;
using System.Diagnostics;

namespace SinovadMediaServer.Application.UseCases.MediaFilePlaybacks
{
    public class MediaFilePlaybackService:IMediaFilePlaybackService
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
                if(mediaFilePlaybackRealTime.ClientData!=null)
                {
                    mediaFilePlaybackRealTime.ClientData.LocalIpAddress = clientIpAddress;
                }
                var physicalPath = mediaFilePlaybackRealTime.ItemData.PhysicalPath;
                var timeSpan = mediaFilePlaybackRealTime.ClientData.CurrentTime.ToString();
                mediaFilePlaybackRealTime.StreamsData = _ffmpegStrategy.GenerateMediaFilePlaybackStreamsData(physicalPath);
                var mediaFilePlaybackTranscodingProcess=_ffmpegStrategy.ExecuteTranscodeAudioVideoAndSubtitleProcess(mediaFilePlaybackRealTime.StreamsData, timeSpan);
                mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess = new List<MediaFilePlaybackTranscodingProcess>() { mediaFilePlaybackTranscodingProcess };
               _sharedData.ListMediaFilePlaybackRealTime.Add(mediaFilePlaybackRealTime);
                _sharedData.HubConnection.InvokeAsync("AddMediaFilePlayBackRealTime", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid,mediaFilePlaybackRealTime.Guid);
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
            }catch (Exception ex){
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
                    mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess.Add(mediaFilePlaybackTranscodingProcess);
                    var videoUrl = _sharedData.WebUrl + "/transcoded/" + mediaFilePlaybackTranscodingProcess.TranscodeFolderName + "/" + mediaFilePlaybackRealTime.StreamsData.OutputTranscodedFileName;
                    response.Data = videoUrl;
                }else{
                    throw new Exception("Transcoded Video Not Found");
                }
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex) {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }        
            return response;
        }

        public Response<bool> UpdateMediaFilePlayback(UpdateMediaFilePlaybackRequestDto updateMediaFilePlaybackData)
        {
            var response = new Response<bool>();
            try
            {
                MediaFilePlayback mediaFilePlayback = _unitOfWork.MediaFilePlaybacks.GetByExpression(a => a.MediaFileId == updateMediaFilePlaybackData.MediaFileId && a.ProfileId == updateMediaFilePlaybackData.ProfileId);
                if (mediaFilePlayback != null)
                {
                    mediaFilePlayback.CurrentTime = updateMediaFilePlaybackData.CurrentTime;
                    mediaFilePlayback.DurationTime = updateMediaFilePlaybackData.DurationTime;
                    _unitOfWork.MediaFilePlaybacks.Update(mediaFilePlayback);
                }else
                {
                    mediaFilePlayback = updateMediaFilePlaybackData.MapTo<MediaFilePlayback>();  
                    _unitOfWork.MediaFilePlaybacks.Add(mediaFilePlayback);
                }
                _unitOfWork.Save();
                var mediaFilePlaybackRealTime = _sharedData.ListMediaFilePlaybackRealTime.Where(x => x.Guid == updateMediaFilePlaybackData.Guid).FirstOrDefault();
                if(mediaFilePlaybackRealTime!=null)
                {
                    mediaFilePlaybackRealTime.ClientData.Duration= updateMediaFilePlaybackData.DurationTime;
                    mediaFilePlaybackRealTime.ClientData.CurrentTime = updateMediaFilePlaybackData.CurrentTime;
                    mediaFilePlaybackRealTime.ClientData.IsPlaying = updateMediaFilePlaybackData.IsPlaying;
                }
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

        public Response<bool> DeleteTranscodedMediaFileByGuid(string guid)
        {
            var response = new Response<bool>();
            try
            {
                var mediaFilePlaybackRealTime = _sharedData.ListMediaFilePlaybackRealTime.Where(x => x.Guid == guid).FirstOrDefault();
                if (mediaFilePlaybackRealTime != null)
                {
                    foreach (var mediaFilePlaybackTranscodingProcess in mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess)
                    {
                        KillProcessAndRemoveDirectory(mediaFilePlaybackTranscodingProcess);
                    }
                }
                _sharedData.ListMediaFilePlaybackRealTime.RemoveAll(x => x.Guid == guid);
                _sharedData.HubConnection.SendAsync("RemoveMediaFilePlayBackRealTime", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, guid);
                response.Data = true;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<bool> DeleteLastTranscodedMediaFileProcessByGuid(string guid)
        {
            var response = new Response<bool>();
            try
            {
                var mediaFilePlaybackRealTime = _sharedData.ListMediaFilePlaybackRealTime.Where(x => x.Guid == guid).FirstOrDefault();
                if (mediaFilePlaybackRealTime != null)
                {
                    var lastMediaFileTranscodingProcess=mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess.OrderBy(x => x.Created).FirstOrDefault();
                    if(lastMediaFileTranscodingProcess != null) {
                       var killAndRemoveCompleted=KillProcessAndRemoveDirectory(lastMediaFileTranscodingProcess);
                        if(killAndRemoveCompleted)
                        {
                            mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess.Remove(lastMediaFileTranscodingProcess);
                        }
                    }
                }
                response.Data = true;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<bool> DeleteAllTranscodedMediaFiles()
        {
            var response = new Response<bool>();
            try
            {
                foreach (var mediaFilePlaybackRealTime in _sharedData.ListMediaFilePlaybackRealTime)
                {
                    foreach (var mediaFilePlaybackTranscodingProcess in mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess)
                    {
                        KillProcessAndRemoveDirectory(mediaFilePlaybackTranscodingProcess);
                    }
                    _sharedData.HubConnection.SendAsync("RemoveMediaFilePlayBackRealTime", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlaybackRealTime.Guid);
                }
                _sharedData.ListMediaFilePlaybackRealTime.Clear();
                response.Data = true;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public Response<bool> DeleteOldTranscodedMediaFiles()
        {
            var response = new Response<bool>();
            try
            {
                List<string> listMediaFileGuidsForDelete= new List<string>();
                foreach (var mediaFilePlaybackRealTime in _sharedData.ListMediaFilePlaybackRealTime)
                {
                    foreach (var mediaFilePlaybackTranscodingProcess in mediaFilePlaybackRealTime.StreamsData.ListMediaFilePlaybackTranscodingProcess)
                    {
                        var forceDelete = CheckAndDeleteTranscodedFile(mediaFilePlaybackTranscodingProcess);
                        if (forceDelete)
                        {
                            listMediaFileGuidsForDelete.Add(mediaFilePlaybackRealTime.Guid);
                        }
                    }
                    _sharedData.HubConnection.SendAsync("RemoveMediaFilePlayBackRealTime", _sharedData.UserData.Guid, _sharedData.MediaServerData.Guid, mediaFilePlaybackRealTime.Guid);
                }
                _sharedData.ListMediaFilePlaybackRealTime.RemoveAll(x=> listMediaFileGuidsForDelete.Contains(x.Guid));
                response.Data = true;
                response.IsSuccess = true;
                response.Message = "Successful";
            }catch (Exception ex)
            {
                response.Message = ex.Message;
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        private bool CheckAndDeleteTranscodedFile(MediaFilePlaybackTranscodingProcess mediaFilePlaybackTranscodingProcess)
        {
            var forceDelete = false;
            var currentMilisecond = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var tvpMilisecond = mediaFilePlaybackTranscodingProcess.Created.Ticks / TimeSpan.TicksPerMillisecond;
            if (currentMilisecond - tvpMilisecond > 86400000)
            {
                forceDelete = true;
            }      
            if (forceDelete)
            {
                KillProcessAndRemoveDirectory(mediaFilePlaybackTranscodingProcess);
            }
            return forceDelete;
        }

        private bool KillProcessAndRemoveDirectory(MediaFilePlaybackTranscodingProcess mediaFilePlaybackTranscodingProcess)
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
