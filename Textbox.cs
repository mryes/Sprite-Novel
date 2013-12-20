using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;

namespace SpriteNovel
{
	struct WrappedText
	{
		public List<string> Strings { get; private set; }
		public Font FontReference { get; private set; }
		public int  FontReferenceSize { get; private set; }
		public int  WrapWidth { get; private set; }
		public int  EndPosition 
		{
			get 
			{
				int runningTotal = 0;
				foreach (string str in Strings)
					runningTotal += str.Length;
				return runningTotal-1;
			}
		}

		public WrappedText(string rawStr, Font font, int fontSize, int width) : this()
		{
			rawText = rawStr;
			FontReference = font;
			FontReferenceSize = fontSize;
			WrapWidth = width;
			CreateTextStrings();
		}

		public void AppendText(string str)
		{
			rawText += str;
			CreateTextStrings();
		}

		public char GetCharacterAtPosition(int position)
		{
			if (position <= EndPosition)
			{
				int row = 0;
	            int column = 0;
				foreach (string str in Strings) {
					if ((column + str.Length) <= position) {
						column += str.Length;
						++row;
					}
					else {
						column = position - column;
						break;
					}
				}
				return Strings[row][column];
			}
			return '\0';
		}

		string rawText;

		void CreateTextStrings()
		{
			Strings = new List<string>();
			List<string> words = rawText.Split(' ').ToList();

			while (words.Count > 0) {

				string line = "";
				int wordsAdded = 0;
				foreach (string word in words) {
					string testLine = line + word + " ";
					int calculatedWidth = CalculateWidthOfString(testLine);
					if (calculatedWidth > WrapWidth)
					{
						// Maybe it will fit without the extra space added
						if ((calculatedWidth - CalculateWidthOfString(" ")) > WrapWidth)
							break;
						testLine = testLine.TrimEnd(' ');
					}
					line = testLine;
					++wordsAdded;
				}

				words.RemoveRange(0, wordsAdded);
				line = line.TrimEnd(' ');
				Strings.Add(line);
			}
		}

		int CalculateWidthOfString(string str)
		{
			int width = 0;
			for (int i=0; i<str.Length; ++i) {
				width += FontReference.GetGlyph(str[i], (uint)FontReferenceSize, false).Bounds.Width;
				if (i+1 < str.Length)
					width += FontReference.GetKerning(str[i], str[i+1], (uint)FontReferenceSize);
			}
			return width;
		}
	}

	class AnimatedWrappedText
	{
		WrappedText fullText;
		public WrappedText FullText 
		{ 
			get { return fullText; } 

			set 
			{
				if (!AnimationActive)
					fullText = value;
			}
		}

		public WrappedText VisibleText { get; private set; }

		public int ScrollAmount { get; private set; }
		public double TextAppearanceSpeed { get; set; }
		public bool AnimationActive { get; private set; }

		AnimatedWrappedText(string rawStr, Font font, int fontsize, int width)
		{
			AnimationActive = false;
			FullText = new WrappedText(rawStr, font, fontsize, width);
			VisibleText = new WrappedText("", font, fontsize, width);
		}

		public void StartAnimation()
		{
			AnimationActive = true;
			timeUntilNextCharacter = 0;
		}

		public void UpdateAnimation(double elapsedTime)
		{
			if (AnimationActive) {
				timeUntilNextCharacter -= elapsedTime;
				if (timeUntilNextCharacter <= 0) {
					if (VisibleText.EndPosition < FullText.EndPosition)
						AddCharacter();
					else AnimationActive = false;
					timeUntilNextCharacter = 1 / TextAppearanceSpeed;
				}
			}
		}

		public void ClearAndRestart(string rawStr)
		{
			Font font = FullText.FontReference;
			int  size = FullText.FontReferenceSize;
			FullText = new WrappedText(rawStr, font, size, FullText.WrapWidth);
			VisibleText = new WrappedText("", font, size, FullText.WrapWidth);
		}

		readonly int textboxRows = 4;
		int invisibleCharacters;

		double timeUntilNextCharacter;
		readonly double commaPause = 1.5;

		void AddCharacter()
		{
			--invisibleCharacters;

			if (VisibleText.Strings.Count-1 > ScrollAmount + textboxRows)
				++ScrollAmount;

			char newCharacter = FullText.GetCharacterAtPosition(
				FullText.EndPosition - invisibleCharacters
			);

			VisibleText.AppendText(new string(newCharacter, 1));

			if (newCharacter == ',')
				timeUntilNextCharacter *= commaPause;
		}
	}
}

