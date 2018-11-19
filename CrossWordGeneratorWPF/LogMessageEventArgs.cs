// ---------------------------------------------------------------------------------------------
// FileName: LogMessageEventArgs.cs
// FileType: Visual C# Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Simple EventArgs class for delivering messages to subscribers
// ---------------------------------------------------------------------------------------------

using System;

namespace Kranken.ScrabbleGenerator
{
	public class LogMessageEventArgs : EventArgs
	{
		// Fields
		public string Message { get; }

		public DateTime Timestamp { get; }

		// Constructor
		public LogMessageEventArgs(string message)
		{
			Message = message;
			Timestamp = DateTime.Now;
		}
	}
}