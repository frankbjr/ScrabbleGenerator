namespace Kranken.ScrabbleLibrary
{
	public class CrossWord
	{
		public CrossWord()
		{
			Word = "";
			XPosition = 0;
			YPosition = 0;
			Direction = DirectionEnum.Horizontal;
		}

		public CrossWord(string word, int x, int y, DirectionEnum direction)
		{
			Word = word;
			XPosition = x;
			YPosition = y;
			Direction = direction;
		}

		public string Word { get; set; }
		public int XPosition { get; set; }
		public int YPosition { get; set; }

		public DirectionEnum Direction { get; set; }
		public int Length => Word.Length;
	}
}
