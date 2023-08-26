
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Persistence;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Interface;
using System.Diagnostics;

namespace SinovadMediaServer.Application.UseCases.TranscodingProcesses
{
    public class TranscodingProcessService : ITranscodingProcessService
    {
        private IUnitOfWork _unitOfWork;


        private readonly SharedData _sharedData;

        private readonly AutoMapper.IMapper _mapper;

        private readonly IAppLogger<TranscodingProcessService> _logger;

        public TranscodingProcessService(IUnitOfWork unitOfWork,SharedData sharedData, AutoMapper.IMapper mapper, IAppLogger<TranscodingProcessService> logger)
        {
            _unitOfWork = unitOfWork;
            _sharedData = sharedData;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<TranscodingProcessDto>> GetAsync(int id)
        {
            var response = new Response<TranscodingProcessDto>();
            try
            {
                var result = await _unitOfWork.TranscodingProcesses.GetAsync(id);
                response.Data = _mapper.Map<TranscodingProcessDto>(result);
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

        public async Task<Response<List<TranscodingProcessDto>>> GetAllAsync()
        {
            var response = new Response<List<TranscodingProcessDto>>();
            try
            {
                var result = await _unitOfWork.TranscodingProcesses.GetAllAsync();
                response.Data = _mapper.Map<List<TranscodingProcessDto>>(result);
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
                response.Data = _mapper.Map<List<TranscodingProcessDto>>(result);
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

        public Response<object> Create(TranscodingProcessDto transcodingProcessDto)
        {
            var response = new Response<object>();
            try
            {
                var transcodingProcess = _mapper.Map<TranscodingProcess>(transcodingProcessDto);
                _unitOfWork.TranscodingProcesses.Add(transcodingProcess);
                _unitOfWork.Save();
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

        public Response<object> CreateList(List<TranscodingProcessDto> listTranscodingProcessDto)
        {
            var response = new Response<object>();
            try
            {
                var transcodingProcess = _mapper.Map<List<TranscodingProcess>>(listTranscodingProcessDto);
                _unitOfWork.TranscodingProcesses.AddList(transcodingProcess);
                _unitOfWork.Save();
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

        public Response<object> Update(TranscodingProcessDto transcodingProcessDto)
        {
            var response = new Response<object>();
            try
            {
                var transcodingProcess = _mapper.Map<TranscodingProcess>(transcodingProcessDto);
                _unitOfWork.TranscodingProcesses.Update(transcodingProcess);
                _unitOfWork.Save();
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
                _logger.LogError(ex.StackTrace);
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
                _logger.LogError(ex.StackTrace);
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
                var listProcessDeletedGUIDs = PerformDeleteListTranscodeVideoProcess(_mapper.Map<List<TranscodingProcessDto>>(listTranscodingProcess), true).Result;
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
                _logger.LogError(ex.StackTrace);
            }
            return response;
        }

        public async Task DeleteOldTranscodeVideoProcess()
        {
            var listTranscodeVideoProcess =  await _unitOfWork.TranscodingProcesses.GetAllAsync();
            if (listTranscodeVideoProcess != null && listTranscodeVideoProcess.Count() > 0)
            {
               await PerformDeleteListTranscodeVideoProcess(_mapper.Map<List<TranscodingProcessDto>>(listTranscodeVideoProcess), false);
            }
        }

        public async Task DeleteAllTranscodeVideoProcess()
        {
            var listTranscodeVideoProcess = await _unitOfWork.TranscodingProcesses.GetAllAsync();
            if (listTranscodeVideoProcess != null && listTranscodeVideoProcess.Count() > 0)
            {
                await PerformDeleteListTranscodeVideoProcess(_mapper.Map<List<TranscodingProcessDto>>(listTranscodeVideoProcess), true);
            }
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

        public async Task<TranscodePrepareVideoDto> PrepareProcess(TranscodePrepareVideoDto transcodePrepareVideo)
        {
            var transcoderSettings = _sharedData.TranscoderSettingsData;
            var mediaAnalysis = FFMpegCore.FFProbe.Analyse(transcodePrepareVideo.PhysicalPath);
            if (mediaAnalysis != null)
            {
                var argumentsStreamsAudio = "";
                var argumentsVarStreamMapAudio = "";
                var listSubtitlesStreams = new List<StreamDto>();
                var listAudioStreams = new List<StreamDto>();

                if (mediaAnalysis.AudioStreams != null && mediaAnalysis.AudioStreams.Count > 0)
                {
                    for (var i = 0; i < mediaAnalysis.AudioStreams.Count; i++)
                    {
                        var audioStream = mediaAnalysis.AudioStreams[i];

                        var language = audioStream.Language;
                        var name = "audio_" + i;
                        var title = "";
                        var isDefault = false;
                        if (audioStream.Disposition != null && audioStream.Disposition.ContainsKey("default"))
                        {
                            isDefault = (bool)audioStream.Disposition["default"];
                        }

                        if (audioStream.Tags != null && audioStream.Tags.ContainsKey("title"))
                        {
                            title = audioStream.Tags["title"].ToString();
                        }
                        else
                        {
                            if (language != null && language != "")
                            {
                                title = language + "_" + i;
                            }
                            else
                            {
                                title = "audio_" + i;
                            }
                        }
                        if (language == null || language == "")
                        {
                            language = "und";
                        }
                        var defaultValue = isDefault == true ? "YES" : "NO";
                        argumentsStreamsAudio = argumentsStreamsAudio + " -map 0:a:" + i;
                        if (argumentsVarStreamMapAudio == "")
                        {
                            argumentsVarStreamMapAudio = argumentsVarStreamMapAudio + "a:" + i + ",name:" + name + ",default:" + defaultValue + ",language:" + language + ",agroup:audios";
                        }
                        else
                        {
                            argumentsVarStreamMapAudio = argumentsVarStreamMapAudio + " a:" + i + ",name:" + name + ",default:" + defaultValue + ",language:" + language + ",agroup:audios";
                        }
                        var newStream = new StreamDto();
                        newStream.index = i;
                        newStream.language = audioStream.Language;
                        newStream.isDefault = isDefault;
                        newStream.title = title;
                        listAudioStreams.Add(newStream);
                    }
                }

                var argumentsFinalStreams = "";
                if (argumentsStreamsAudio != "")
                {
                    argumentsFinalStreams = argumentsStreamsAudio + " -map 0:v:0";
                }
                var argumentsFinalVarStreamMap = "";
                if (argumentsVarStreamMapAudio != "")
                {
                    var varStreamsWithVideo = argumentsVarStreamMapAudio + " v:0,agroup:audios";
                    var varStreamsInQuotes = "\"" + varStreamsWithVideo + "\"";
                    var varStreamsInThreeQuotes = "\"\"\"" + varStreamsWithVideo + "\"\"\"";
                    argumentsFinalVarStreamMap = "-var_stream_map " + varStreamsInQuotes;
                }
                var crf = 18;
                if (transcoderSettings.ConstantRateFactor != null)
                {
                    crf = transcoderSettings.ConstantRateFactor;
                }
                var argumentsFormatVideo = "-muxpreload 0 -muxdelay 0 -c:v copy";
                if (mediaAnalysis.VideoStreams != null && mediaAnalysis.VideoStreams.Count > 0)
                {
                    var videoStream = mediaAnalysis.VideoStreams[0];
                    if (videoStream.CodecName == "mpeg4" || (videoStream.CodecName == "hevc" && videoStream.CodecLongName.IndexOf("H.265") != -1))
                    {
                        argumentsFormatVideo = "-muxpreload 0 -muxdelay 0 -c:v libx264 -crf " + crf + " -vf format=yuv420p";
                    }
                }
                var argumentsFormatAudio = "-c:a aac";

                var panSection = "stereo|c0=0.5*c2+0.707*c0+0.707*c4+0.5*c3|c1=0.5*c2+0.707*c1+0.707*c5+0.5*c3";
                var panSectionInQuates = "\"" + panSection + "\"";
                var argumentsStereoAudio = "-af pan=" + panSectionInQuates;
                var presetObject = _sharedData.ListPresets.Find(x => x.Id == transcoderSettings.PresetCatalogDetailId);
                var argumentsPreset = "-preset " + presetObject.TextValue;

                var videoOutputName = "video.m3u8";
                var formatOutputName = "out_%v.m3u8";

                var varInputPhysicalPathInQuotes = "\"" + transcodePrepareVideo.PhysicalPath + "\"";

                var mediaStreamsArguments = " -itsoffset 10 -i " + varInputPhysicalPathInQuotes + " " + argumentsFormatVideo + " " + argumentsFormatAudio + " " + argumentsFinalStreams + " " + argumentsStereoAudio + " " + argumentsPreset + " -f hls " + argumentsFinalVarStreamMap + " -master_pl_name " + videoOutputName + " -start_number 0 -hls_time 1 -hls_list_size 0 " + formatOutputName;

                var finalCommandGenerateStreams = mediaStreamsArguments;

                var argumentsSubtitles = "";
                if (mediaAnalysis.SubtitleStreams != null && mediaAnalysis.SubtitleStreams.Count > 0)
                {
                    var currentIndex = 0;
                    for (var i = 0; i < mediaAnalysis.SubtitleStreams.Count; i++)
                    {
                        var subtitleStream = mediaAnalysis.SubtitleStreams[i];
                        if (subtitleStream.CodecName == "subrip")
                        {
                            var outputName = "subtitle_" + i + "_%03d.vtt";
                            argumentsSubtitles = argumentsSubtitles + " -map 0:s:" + i + " -f segment -segment_list subtitle_" + i + ".m3u8 -segment_time 1 " + outputName;
                            try
                            {
                                var newStream = new StreamDto();
                                newStream.index = currentIndex;
                                newStream.language = subtitleStream.Language;
                                var title = "";
                                var defaultValue = "NO";
                                var forcedValue = "NO";
                                if (subtitleStream.Disposition != null && subtitleStream.Disposition.ContainsKey("default"))
                                {
                                    defaultValue = (bool)subtitleStream.Disposition["default"] == true ? "YES" : "NO";
                                    newStream.isDefault = (bool)subtitleStream.Disposition["default"];
                                }
                                if (subtitleStream.Disposition != null && subtitleStream.Disposition.ContainsKey("forced"))
                                {
                                    forcedValue = (bool)subtitleStream.Disposition["forced"] == true ? "YES" : "NO";
                                    newStream.isForced = (bool)subtitleStream.Disposition["forced"];
                                }
                                if (subtitleStream.Tags != null && subtitleStream.Tags.ContainsKey("title"))
                                {

                                    title = subtitleStream.Tags["title"].ToString();
                                }
                                else
                                {
                                    if (newStream.language != null && newStream.language != "")
                                    {
                                        title = newStream.language + "_" + i;
                                    }
                                    else
                                    {
                                        title = "subtitle_" + i;
                                    }
                                }
                                var lineTextPlayList = "#EXT-X-MEDIA:TYPE=SUBTITLES,GROUP-ID=\"subtitles\",NAME=\"" + title + "\",DEFAULT=" + defaultValue + ",FORCED=" + forcedValue + ",LANGUAGE=\"" + newStream.language + "\",URI=\"subtitle_" + i + ".m3u8\"";

                                newStream.lineTextPlayList = lineTextPlayList;
                                newStream.title = title;
                                listSubtitlesStreams.Add(newStream);
                                currentIndex = currentIndex + 1;
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                var finalCommandGenerateSubtitles = "";
                if (argumentsSubtitles != "")
                {
                    var argumentsFormatSubtitle = "-c:s webvtt";
                    finalCommandGenerateSubtitles = "-itsoffset 10 -i " + varInputPhysicalPathInQuotes + " " + argumentsFormatSubtitle + " " + argumentsSubtitles;
                }

                var transcodeDirectoryRoutePath = _sharedData.WebUrl + "/transcoded";

                transcodePrepareVideo.TotalSeconds = mediaAnalysis.Duration.TotalSeconds;
                transcodePrepareVideo.TranscodeDirectoryPhysicalPath = transcoderSettings.TemporaryFolder;
                transcodePrepareVideo.TranscodeDirectoryRoutePath = transcodeDirectoryRoutePath;
                transcodePrepareVideo.VideoOutputName = videoOutputName;
                transcodePrepareVideo.FinalCommandGenerateStreams = finalCommandGenerateStreams;
                transcodePrepareVideo.FinalCommandGenerateSubtitles = finalCommandGenerateSubtitles;
                transcodePrepareVideo.ListAudioStreams = listAudioStreams;
                transcodePrepareVideo.ListSubtitlesStreams = listSubtitlesStreams;
                transcodePrepareVideo.TransmissionMethodId = transcoderSettings.VideoTransmissionTypeCatalogDetailId;
                transcodePrepareVideo.Preset = presetObject.TextValue;
            }

            return transcodePrepareVideo;
        }

        public TranscodeRunVideoDto RunProcess(TranscodePrepareVideoDto transcodePrepareVideo)
        {
            var transcodeRunVideo = new TranscodeRunVideoDto();

            string time = "0";
            if (transcodePrepareVideo.TimeSpan != null)
            {
                time = transcodePrepareVideo.TimeSpan;
            }
            var seekTimeSpanSection = "";
            if (time != "0")
            {
                seekTimeSpanSection = "-seek_timestamp 1 -ss " + time;
            }

            var finalArgumentsStreams = seekTimeSpanSection + " " + transcodePrepareVideo.FinalCommandGenerateStreams;
            var finalArgumentsSubtitles = seekTimeSpanSection + " " + transcodePrepareVideo.FinalCommandGenerateSubtitles;

            //Create video output directory
            Guid foldervideoname = Guid.NewGuid();
            var videoOutputDirectoryPhysicalPath = transcodePrepareVideo.TranscodeDirectoryPhysicalPath + "\\" + foldervideoname;
            System.IO.Directory.CreateDirectory(videoOutputDirectoryPhysicalPath);
            var videoOutputDirectoryRoutePath = transcodePrepareVideo.TranscodeDirectoryRoutePath + "/" + foldervideoname;
            string exe = "ffmpeg.exe";

            var videoOutputRoutePath = videoOutputDirectoryRoutePath + "/" + transcodePrepareVideo.VideoOutputName;
            var videoOutputPhysicalPath = videoOutputDirectoryPhysicalPath + "\\" + transcodePrepareVideo.VideoOutputName;

            var transcodeVideoProcess = new TranscodingProcessDto();
            transcodeVideoProcess.RequestGuid = transcodePrepareVideo.ProcessGUID;
            transcodeVideoProcess.GeneratedTemporaryFolder = videoOutputDirectoryPhysicalPath;

            ProcessStartInfo procStartIfo = new ProcessStartInfo();
            procStartIfo.WorkingDirectory = videoOutputDirectoryPhysicalPath;
            procStartIfo.FileName = exe;
            procStartIfo.Arguments = finalArgumentsStreams;
            procStartIfo.UseShellExecute = false;
            procStartIfo.CreateNoWindow = true;
            try
            {
                var process = new Process();
                process.StartInfo = procStartIfo;
                process.EnableRaisingEvents = true;
                process.Start();
                transcodeVideoProcess.SystemProcessIdentifier = process.Id;
                transcodeRunVideo.TranscodeProcessStreamId = process.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (transcodePrepareVideo.FinalCommandGenerateSubtitles != "")
            {
                ProcessStartInfo procStartIfo2 = new ProcessStartInfo();
                procStartIfo2.WorkingDirectory = videoOutputDirectoryPhysicalPath;
                procStartIfo2.FileName = exe;
                procStartIfo2.Arguments = finalArgumentsSubtitles;
                //procStartIfo.ArgumentList.Add(finalArgumentsSubtitles);
                procStartIfo2.UseShellExecute = false;
                procStartIfo2.CreateNoWindow = true;
                try
                {
                    var process2 = new Process();
                    process2.StartInfo = procStartIfo2;
                    process2.EnableRaisingEvents = true;
                    process2.Start();
                    transcodeRunVideo.TranscodeProcessSubtitlesId = process2.Id;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            if (transcodeVideoProcess.RequestGuid != null)
            {
                transcodeVideoProcess.Created = DateTime.Now;
                if (transcodePrepareVideo.MediaServerId != null && transcodePrepareVideo.MediaServerId != 0)
                {
                    transcodeVideoProcess.MediaServerId = transcodePrepareVideo.MediaServerId;
                }
                _unitOfWork.TranscodingProcesses.Add(_mapper.Map<TranscodingProcess>(transcodeVideoProcess));
                _unitOfWork.Save();
            }

            transcodeRunVideo.VideoPath = videoOutputRoutePath;
            transcodeRunVideo.VideoOutputDirectoryPhysicalPath = videoOutputDirectoryPhysicalPath;
            transcodeRunVideo.VideoOutputDirectoryRoutePath = videoOutputDirectoryRoutePath;
            var exist = checkIfExistFile(videoOutputPhysicalPath).Result;
            if (transcodePrepareVideo.ListSubtitlesStreams != null && transcodePrepareVideo.ListSubtitlesStreams.Count > 0)
            {
                try
                {
                    string text = System.IO.File.ReadAllText(videoOutputPhysicalPath);
                    string[] lines = text.Split("\n");
                    string line = lines.Where(item => item.IndexOf("RESOLUTION") != -1).First();
                    string subtitleLines = "";
                    for (var i = 0; i < transcodePrepareVideo.ListSubtitlesStreams.Count; i++)
                    {
                        var subtitle = transcodePrepareVideo.ListSubtitlesStreams[i];
                        subtitleLines = subtitleLines + "\n" + subtitle.lineTextPlayList;
                    }
                    string lineToReplace = subtitleLines + "\n" + line + ",SUBTITLES=\"subtitles\"";
                    string finalText = text.Replace(line, lineToReplace);
                    System.IO.File.WriteAllText(videoOutputPhysicalPath, finalText);
                }
                catch (Exception e)
                {

                }
            }
            transcodeRunVideo.TranscodePrepareVideo = transcodePrepareVideo;
            return transcodeRunVideo;
        }

        private async Task<bool> checkIfExistFile(string outputFilePhysicalPathFinal)
        {
            var exist = System.IO.File.Exists(outputFilePhysicalPathFinal);
            if (exist)
            {
                return true;
            }
            else
            {
                System.Threading.Thread.Sleep(1000);
                return checkIfExistFile(outputFilePhysicalPathFinal).Result;
            }
        }


    }
}
