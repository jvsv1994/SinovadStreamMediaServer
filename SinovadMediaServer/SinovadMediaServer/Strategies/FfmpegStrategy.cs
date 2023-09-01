using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Shared;
using System.Diagnostics;

namespace SinovadMediaServer.Strategies
{
    public class FfmpegStrategy
    {
        private readonly SharedData _sharedData;

        public FfmpegStrategy(SharedData sharedData)
        {
            _sharedData = sharedData;
        }

        public void GenerateThumbnailByPhysicalPath(string folderName,string physicalPath)
        {
            var mediaAnalysis = FFMpegCore.FFProbe.Analyse(physicalPath);
            var position = mediaAnalysis.Duration.Divide(2).ToString();
            string exe = "ffmpeg.exe";
            var physicalPathInQuotes = "\"" + physicalPath + "\"";
            string arguments = "-ss "+ position + " -i "+ physicalPathInQuotes + " -frames:v 1 thumbnail.png";
            var rootPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sinovad Media Server");
            var mediaPath = Path.Combine(rootPath, "Media");
            var folderPath=Path.Combine(mediaPath, folderName);
            System.IO.Directory.CreateDirectory(folderPath);
            ProcessStartInfo procStartIfo = new ProcessStartInfo();
            procStartIfo.WorkingDirectory = folderPath;
            procStartIfo.FileName = exe;
            procStartIfo.Arguments = arguments;
            procStartIfo.UseShellExecute = false;
            procStartIfo.CreateNoWindow = true;
            try
            {
                var process = new Process();
                process.StartInfo = procStartIfo;
                process.EnableRaisingEvents = true;
                process.Start();
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private int ExecuteFfmpegProcess(string workingDirectory,string arguments)
        {
            string exe = "ffmpeg.exe";
            ProcessStartInfo procStartIfo = new ProcessStartInfo();
            procStartIfo.WorkingDirectory = workingDirectory;
            procStartIfo.FileName = exe;
            procStartIfo.Arguments = arguments;
            procStartIfo.UseShellExecute = false;
            procStartIfo.CreateNoWindow = true;
            var process = new Process();
            try
            {
                process.StartInfo = procStartIfo;
                process.EnableRaisingEvents = true;
                process.Start();
            }catch (Exception ex){
                throw new Exception(ex.Message);
            }
            return process.Id;
        }

        public MediaFilePlaybackCurrentStreamsDto GenerateMediaFilePlaybackStreamsData(string physicalPath)
        {
            var transcoderSettings = _sharedData.TranscoderSettingsData;
            var mediaAnalysis = FFMpegCore.FFProbe.Analyse(physicalPath);
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

                var varInputPhysicalPathInQuotes = "\"" + physicalPath + "\"";

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

                var mediaFilePlaybackCurrentStreams = new MediaFilePlaybackCurrentStreamsDto();
                mediaFilePlaybackCurrentStreams.Duration = mediaAnalysis.Duration.TotalSeconds;
                mediaFilePlaybackCurrentStreams.VideoTransmissionTypeId = transcoderSettings.VideoTransmissionTypeCatalogDetailId;
                mediaFilePlaybackCurrentStreams.ListAudioStreams = listAudioStreams;
                mediaFilePlaybackCurrentStreams.ListSubtitleStreams = listSubtitlesStreams;
                mediaFilePlaybackCurrentStreams.OutputTranscodedFileName = "video.m3u8";
                mediaFilePlaybackCurrentStreams.CommandGenerateAudioVideoStreams = finalCommandGenerateStreams;
                mediaFilePlaybackCurrentStreams.CommandGenerateSubtitleStreams = finalCommandGenerateSubtitles;
                return mediaFilePlaybackCurrentStreams;
            }
            return null;
        }

        public MediaFilePlaybackTranscodingProcess ExecuteTranscodeAudioVideoAndSubtitleProcess(MediaFilePlaybackCurrentStreamsDto mediaFilePlaybackCurrentStreams,string timeSpan)
        {           
            var mediaFilePlaybackTranscodingProcess= new MediaFilePlaybackTranscodingProcess();
            string time = "0";
            if(timeSpan != null)
            {
                time = timeSpan;
            }
            var seekTimeSpanSection = "";
            if (time != "0")
            {
                seekTimeSpanSection = "-seek_timestamp 1 -ss " + time;
            }

            var transcodeFolderName=Guid.NewGuid().ToString();
            mediaFilePlaybackTranscodingProcess.TranscodeFolderName = transcodeFolderName;
            var workingDirectory = _sharedData.TranscoderSettingsData.TemporaryFolder + "\\" + transcodeFolderName;
            System.IO.Directory.CreateDirectory(workingDirectory);
            mediaFilePlaybackTranscodingProcess.TranscodeFolderPath = workingDirectory;

            // Generate Audio Video Transcode Process
            var finalArgumentsAudioVideoStreams = seekTimeSpanSection + " " + mediaFilePlaybackCurrentStreams.CommandGenerateAudioVideoStreams;
            var processAudioVideoId = ExecuteFfmpegProcess(workingDirectory, finalArgumentsAudioVideoStreams);
            mediaFilePlaybackTranscodingProcess.TranscodeAudioVideoProcessId = processAudioVideoId;
            //Generate Subtitles Transcode Process
            if (mediaFilePlaybackCurrentStreams.CommandGenerateSubtitleStreams != null)
            {
                var finalArgumentsSubtitlesStreams = seekTimeSpanSection + " " + mediaFilePlaybackCurrentStreams.CommandGenerateSubtitleStreams;
                var processSubtitlesId = ExecuteFfmpegProcess(workingDirectory, finalArgumentsSubtitlesStreams);
                mediaFilePlaybackTranscodingProcess.TranscodeSubtitlesProcessId = processSubtitlesId;
            }
            var outputFilePath = workingDirectory + "\\" + mediaFilePlaybackCurrentStreams.OutputTranscodedFileName;
            var exist = checkIfExistFile(outputFilePath).Result;
            if (mediaFilePlaybackCurrentStreams.ListSubtitleStreams != null && mediaFilePlaybackCurrentStreams.ListSubtitleStreams.Count > 0)
            {
                try
                {
                    string text = System.IO.File.ReadAllText(outputFilePath);
                    string[] lines = text.Split("\n");
                    string line = lines.Where(item => item.IndexOf("RESOLUTION") != -1).First();
                    string subtitleLines = "";
                    for (var i = 0; i < mediaFilePlaybackCurrentStreams.ListSubtitleStreams.Count; i++)
                    {
                        var subtitle = mediaFilePlaybackCurrentStreams.ListSubtitleStreams[i];
                        subtitleLines = subtitleLines + "\n" + subtitle.lineTextPlayList;
                    }
                    string lineToReplace = subtitleLines + "\n" + line + ",SUBTITLES=\"subtitles\"";
                    string finalText = text.Replace(line, lineToReplace);
                    System.IO.File.WriteAllText(outputFilePath, finalText);
                }catch (Exception e)
                {

                }
            }
            mediaFilePlaybackTranscodingProcess.Created = DateTime.Now;
            return mediaFilePlaybackTranscodingProcess;
        }

        private async Task<bool> checkIfExistFile(string outputFilePhysicalPathFinal)
        {
            var exist = System.IO.File.Exists(outputFilePhysicalPathFinal);
            if (exist)
            {
                return true;
            }else
            {
                System.Threading.Thread.Sleep(1000);
                return await checkIfExistFile(outputFilePhysicalPathFinal);
            }
        }

    }
}
