using System;

namespace GifTool.ViewModel
{
    internal class MainWindowViewModel : NotifyBase
    {
        private readonly Func<SelectReplayViewModel> _selectReplayViewModelFactory;
        private readonly Func<string, SelectTurnViewModel> _selectTurnViewModelFactory;
        private readonly Func<string, CreateGifViewModel> _createGifViewModelFactory;
        private PageViewModelBase _currentPage;

        public PageViewModelBase CurrentPage
        {
            get => _currentPage;
            set => SetProperty(out _currentPage, value);
        }

        public MainWindowViewModel(
            Func<SelectReplayViewModel> selectReplayViewModelFactory,
            Func<string, SelectTurnViewModel> selectTurnViewModelFactory,
            Func<string, CreateGifViewModel> createGifViewModelFactory)
        {
            _selectReplayViewModelFactory = selectReplayViewModelFactory;
            _selectTurnViewModelFactory = selectTurnViewModelFactory;
            _createGifViewModelFactory = createGifViewModelFactory;
            NavigateToSelectReplay();
        }

        private void NavigateToSelectReplay()
        {
            var page = _selectReplayViewModelFactory();
            page.ReplaySelected += NavigateToSelectTurn;
            CurrentPage = page;
        }

        private void NavigateToSelectTurn(string replay)
        {
            var page = _selectTurnViewModelFactory(replay);
            page.BackPressed += NavigateToSelectReplay;
            page.TurnSelected += t => NavigateToCreateGif(replay, t.StartTime, t.EndTime);
            CurrentPage = page;
        }

        private void NavigateToCreateGif(string replay, TimeSpan replayStart, TimeSpan replayEnd)
        {
            var page = _createGifViewModelFactory(replay);
            page.BackPressed += () => NavigateToSelectTurn(replay);
            page.StartTime = replayStart.ToString();
            page.EndTime = replayEnd.ToString();
            CurrentPage = page;
        }
    }
}
