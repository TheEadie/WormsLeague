using System;

namespace GifTool.ViewModel
{
    public class OperationFailedException : Exception
    {
        public OperationFailedException(string message) : base(message)
        {
        }

        public OperationFailedException(string message, Exception innerException) : base(message, innerException)
        {          
        }
    }
}
