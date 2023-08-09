using Microsoft.Extensions.Options;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.CustomModels;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Infrastructure;
using SinovadMediaServer.Shared;
using System.Diagnostics;

namespace SinovadMediaServer.Strategies
{
    public class TranscodeVideoStrategy
    {

        private readonly SinovadApiService _sinovadApiService;

        private readonly SharedData _sharedData;

        public TranscodeVideoStrategy(SinovadApiService restService, SharedData sharedData)
        {
            _sinovadApiService = restService;
            _sharedData = sharedData;
        }

        public async Task<TranscodePrepareVideoDto> Prepare(TranscodePrepareVideoDto transcodePrepareVideo)
        {
            var transcoderSettings = _sharedData.TranscoderSettingsData;
            var mediaAnalysis = FFMpegCore.FFProbe.Analyse(transcodePrepareVideo.PhysicalPath);
            if (mediaAnalysis != null)
            {
                var argumentsStreamsAudio = "";
                var argumentsVarStreamMapAudio = "";
                var listSubtitlesStreams = new List<CustomStream>();
                var listAudioStreams = new List<CustomStream>();

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
                        var newStream = new CustomStream();
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
                                var newStream = new CustomStream();
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

                var transcodeDirectoryRoutePath = _sharedData.WebUrl+ "/transcoded";

                transcodePrepareVideo.TotalSeconds=mediaAnalysis.Duration.TotalSeconds;
                transcodePrepareVideo.TranscodeDirectoryPhysicalPath= transcoderSettings.TemporaryFolder;
                transcodePrepareVideo.TranscodeDirectoryRoutePath= transcodeDirectoryRoutePath;
                transcodePrepareVideo.VideoOutputName=videoOutputName;
                transcodePrepareVideo.FinalCommandGenerateStreams=finalCommandGenerateStreams;
                transcodePrepareVideo.FinalCommandGenerateSubtitles=finalCommandGenerateSubtitles;
                transcodePrepareVideo.ListAudioStreams=listAudioStreams;
                transcodePrepareVideo.ListSubtitlesStreams=listSubtitlesStreams;
                transcodePrepareVideo.TransmissionMethodId=transcoderSettings.VideoTransmissionTypeCatalogDetailId;
                transcodePrepareVideo.Preset=presetObject.TextValue;
            }
            
            return transcodePrepareVideo;
        }

        public TranscodeRunVideoDto Run(TranscodePrepareVideoDto transcodePrepareVideo)
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
                transcodeRunVideo.TranscodeProcessStreamId= process.Id;
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
                registerTranscodeVideoProcess(transcodeVideoProcess);
            }

            transcodeRunVideo.VideoPath=videoOutputRoutePath;
            transcodeRunVideo.VideoOutputDirectoryPhysicalPath=videoOutputDirectoryPhysicalPath;
            transcodeRunVideo.VideoOutputDirectoryRoutePath=videoOutputDirectoryRoutePath;
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

        public void registerTranscodeVideoProcess(TranscodingProcessDto transcodeVideoProcess)
        {
            var res = _sinovadApiService.ExecuteHttpMethodAsync<object>(HttpMethodType.POST, "/transcodingProcesses/Create", transcodeVideoProcess).Result;
        }

        public async Task<bool> checkIfExistFile(string outputFilePhysicalPathFinal)
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
