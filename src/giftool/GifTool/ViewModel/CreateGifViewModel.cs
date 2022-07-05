using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GifTool.Gif;
using GifTool.Worms;
using Microsoft.Win32;

namespace GifTool.ViewModel
{
    internal class CreateGifViewModel : PageViewModelBase
    {
        private const int FrameRateDivider = 3; //Fraction of worms frame rate per frame in gif
        private const int AnimationDelay = 6; //Fraction of 1/100th of a second delay

        private readonly IGifEncoder _gifEncoder;
        private readonly IWormsRunner _wormsRunner;
        private readonly string _replay;
        private string[] _frames;

        public ICommand ExportFramesCommand => new ActionCommand(ExportFrames);
        public ICommand CreateGifCommand => new ActionCommand(CreateGif);
        public ICommand BackCommand => new ActionCommand(() => BackPressed?.Invoke());
        
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ResolutionX { get; set; } = 640;
        public int ResolutionY { get; set; } = 480;
        public int Width { get; set; } = 640;
        public int Height { get; set; } = 480;

        public bool CanCreateGif => _frames?.Length > 0;

        public string OperationStatus { get; private set; }
        public ObservableOperation<string> CurrentOperation { get; private set; }

        public CreateGifViewModel(IGifEncoder gifEncoder, IWormsRunner wormsRunner, string replay)
        {
            _gifEncoder = gifEncoder;
            _wormsRunner = wormsRunner;
            _replay = replay;
            _frames = new string[0];
            CurrentOperation = new ObservableOperation<string>(Task.FromResult(string.Empty), string.Empty);
        }

        public void CreateGif()
        {
            var fileName = SaveFileAs(_replay, ".gif", "Animated GIF |*.gif");
            if (fileName != null)
            {
                CurrentOperation = new ObservableOperation<string>(CreateGifTask(fileName), string.Empty);
                OnPropertyChanged(nameof(CurrentOperation));
            }
        }

        public void ExportFrames()
        {
            CurrentOperation = new ObservableOperation<string>(ExportFramesTask(), string.Empty);
            OnPropertyChanged(nameof(CurrentOperation));
        }

        private async Task<string> ExportFramesTask()
        {
            var startTime = TimeSpan.Parse(StartTime);
            var endTime = TimeSpan.Parse(EndTime);

            _frames = null;
            OperationStatus = "Exporting replay frames...";
            OnPropertyChanged(nameof(OperationStatus));
            OnPropertyChanged(nameof(CanCreateGif));

            _frames = await _wormsRunner.CreateReplayVideo(_replay, FrameRateDivider, startTime, endTime, ResolutionX, ResolutionY);
            OnPropertyChanged(nameof(CanCreateGif));
            return "Exported replay frames";
        }

        private async Task<string> CreateGifTask(string fileName)
        {
            OperationStatus = "Creating GIF...";
            OnPropertyChanged(nameof(OperationStatus));
            await _gifEncoder.CreateGifFromFiles(_frames, fileName, AnimationDelay, Width, Height);
            return "Created: " + fileName;
        }

        private static string SaveFileAs(string defaultFileName, string defaultExt, string filter)
        {
            var dialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                DefaultExt = defaultExt,
                Filter = filter
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public event Action BackPressed;
    }
}
