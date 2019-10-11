using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GifTool.Worms;

namespace GifTool.ViewModel
{
    internal class SelectTurnViewModel : PageViewModelBase
    {
        public class TurnViewModel
        {
            public Turn Model { get; }
            public string Description { get; }

            public TurnViewModel(Turn turn)
            {
                Model = turn;
                Description = $"{turn.Team}: {turn.WeaponActions.LastOrDefault()?.Description ?? "No weapon used"}";
            }
        }

        private readonly IWormsRunner _wormsRunner;

        public ObservableOperation<IReadOnlyList<TurnViewModel>> TurnsOperation { get; }
        public int SelectedTurn { get; set; } = -1;

        public ICommand SelectTurnCommand => new ActionCommand(SelectTurn);
        public ICommand BackCommand => new ActionCommand(() => BackPressed?.Invoke());

        public SelectTurnViewModel(IWormsRunner wormsRunner, string replay)
        {
            _wormsRunner = wormsRunner;
            TurnsOperation = new ObservableOperation<IReadOnlyList<TurnViewModel>>(GetTurnsForReplay(replay), new TurnViewModel[0]);
        }

        private async Task<IReadOnlyList<TurnViewModel>> GetTurnsForReplay(string replay)
        {
            if (!_wormsRunner.TryGetLogForReplay(replay, out var replayLog))
            {
                replayLog = await _wormsRunner.CreateReplayLog(replay);
            }

            if (replayLog == null)
            {
                throw new OperationFailedException($"Log for replay not created: '{replay}'");
            }

            var turns = _wormsRunner.ReadReplayLog(replayLog);
            return turns.Select(x => new TurnViewModel(x)).ToArray();
        }

        private void SelectTurn()
        {
            if (SelectedTurn >= 0 && SelectedTurn < TurnsOperation.Result.Count)
            {
                TurnSelected?.Invoke(TurnsOperation.Result[SelectedTurn].Model);
            }
        }

        public event Action<Turn> TurnSelected;
        public event Action BackPressed;
    }
}

