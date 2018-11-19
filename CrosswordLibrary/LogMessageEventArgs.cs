using System;

namespace Kranken.ScrabbleLibrary
{
	public class LogMessageEventArgs : EventArgs
	{
		public LogMessageEventArgs(string message, bool incompleteMessage)
		{
			Message = message;
			Timestamp = DateTime.Now;
			IncompleteMessage = incompleteMessage;
		}

		public string Message { get; }
		public DateTime Timestamp { get; }
		public bool IncompleteMessage { get; }
	}
}
