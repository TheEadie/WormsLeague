using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GifTool.Worms
{
    internal class WormsRunner : IWormsRunner
    {
        private const string ReplayExtension = ".WAgame";
        private const string ReplayLogExtension = ".log";
        private const string TimeFormatString = @"hh\:mm\:ss\.ff";
        private static readonly Encoding ReplayLogEncoding = Encoding.GetEncoding(1252);

        private readonly IWormsLocator _wormsLocator;
        private readonly ISteamService _steamService;
        private readonly ITurnParser _turnParser;

        public WormsRunner(IWormsLocator wormsLocator, ISteamService steamService, ITurnParser turnParser)
        {
            _wormsLocator = wormsLocator;
            _steamService = steamService;
            _turnParser = turnParser;
        }

        public string[] GetAllReplays()
        {
            return Directory.GetFiles(_wormsLocator.GamesLocation, $"*{ReplayExtension}")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        public Turn[] ReadReplayLog(string replayLog)
        {
            var contents = File.ReadAllText(replayLog, ReplayLogEncoding);
            return _turnParser.ParseTurns(contents);
        }

        public bool TryGetLogForReplay(string replay, out string replayLog)
        {
            var path = Path.Combine(_wormsLocator.GamesLocation, replay + ReplayLogExtension);
            if (File.Exists(path))
            {
                replayLog = path;
                return true;
            }

            replayLog = null;
            return false;
        }

        public async Task<string> CreateReplayLog(string replay)
        {
            var path = Path.Combine(_wormsLocator.GamesLocation, replay + ReplayExtension);
            if (await RunWorms("/getlog", $"\"{path}\"", "/quiet"))
            {
                return Path.Combine(_wormsLocator.GamesLocation, replay + ReplayLogExtension);
            }
            return null;
        }

        public async Task<string[]> CreateReplayVideo(string replay, int frameRateDivider, TimeSpan start, TimeSpan end, int xResolution, int yResolution)
        {
            var path = Path.Combine(_wormsLocator.GamesLocation, replay + ReplayExtension);
            var startTime = start.ToString(TimeFormatString);
            var endTime = end.ToString(TimeFormatString);

            var videoFolder = Path.Combine(_wormsLocator.VideoLocation, replay);
            if (Directory.Exists(videoFolder))
            {
                Directory.Delete(videoFolder, true);
            }

            await RunWorms("/getvideo",
                $"\"{path}\"",
                frameRateDivider.ToString(),
                startTime,
                endTime,
                xResolution.ToString(),
                yResolution.ToString());

            return Directory.GetFiles(videoFolder, "*.png");
        }

        private Task<bool> RunWorms(params string[] wormsArgs)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                var args = string.Join(" ", wormsArgs);
                using (var process = Process.Start(_wormsLocator.ExeLocation, args))
                {
                    if(process == null) { return false; }

                    process.WaitForExit();
                    if (process.ExitCode == 0) { return true; }
                }

                _steamService.WaitForSteamPrompt();

                var wormsProcess = Process.GetProcessesByName(_wormsLocator.ProcessName).FirstOrDefault();
                wormsProcess?.WaitForExit();

                return wormsProcess != null;
            });
        }
    }
}
