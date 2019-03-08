using System.Threading.Tasks;

namespace GifTool.ViewModel
{
    internal class ObservableOperation<T> : NotifyBase
    {
        public T Result { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public bool IsRunning { get; private set; }
        public bool IsFinished => !IsRunning;
        public bool HasResult { get; private set; }
        public bool HasError { get; private set; }

        public ObservableOperation(T defaultResult)
        {
            IsRunning = false;
            HasResult = false;
            HasError = false;
            Result = defaultResult;
            ErrorMessage = string.Empty;
        }

        public ObservableOperation(Task<T> task, T defaultResult)
        {
            SetFromTask(task, defaultResult);

            if (IsRunning)
            {
                task.ContinueWith(
                    t =>
                    {
                        SetFromTask(t, defaultResult);
                        OnPropertyChanged(null);
                    });
            }
        }

        private void SetFromTask(Task<T> task, T defaultResult)
        {
            IsRunning = !task.IsCompleted;

            HasError = task.Status == TaskStatus.Faulted;
            if (HasError && task.Exception != null)
            {
                var unwrappedException = task.Exception.InnerExceptions.Count == 1 ? task.Exception.InnerExceptions[0] : task.Exception;
                ErrorMessage = unwrappedException.Message;
            }

            HasResult = task.Status == TaskStatus.RanToCompletion;
            Result = HasResult ? task.Result : defaultResult;
        }
    }
}
