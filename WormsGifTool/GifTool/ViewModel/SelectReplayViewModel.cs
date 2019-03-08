using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GifTool.Worms;

namespace GifTool.ViewModel
{
    internal class SelectReplayViewModel : PageViewModelBase
    {
        private readonly IWormsRunner _wormsRunner;

        public ObservableCollection<string> ReplayList { get; }

        public int SelectedReplay { get; set; } = -1;

        public ICommand Refresh => new ActionCommand(RefreshReplayList);

        public ICommand Select => new ActionCommand(SelectReplay);

        public SelectReplayViewModel(IWormsRunner wormsRunner)
        {
            _wormsRunner = wormsRunner;
            ReplayList = new ObservableCollection<string>();
            RefreshReplayList();
        }

        private void RefreshReplayList()
        {
            var replays = _wormsRunner.GetAllReplays();
            Array.Sort(replays, (left, right) => string.Compare(right, left, StringComparison.Ordinal));
            ReplayList.Clear();
            foreach (var replay in replays)
            {
                ReplayList.Add(replay);
            }
        }

        private void SelectReplay()
        {
            if (SelectedReplay >= 0 && SelectedReplay < ReplayList.Count)
            {
                ReplaySelected?.Invoke(ReplayList[SelectedReplay]);
            }
        }

        public event Action<string> ReplaySelected;
    }
}
