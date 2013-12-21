using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;
using SFML.Graphics;

namespace SpriteNovel
{
	struct WrappedText
	{
		public List<string> Strings { get; set; }
		public string FlatString 
		{ 
			get
			{
				string flatString = "";
				foreach (string str in Strings)
					flatString += str;
				return flatString;
			}
		}
		public Font FontReference { get; private set; }
		public uint FontReferenceSize { get; private set; }
		public uint WrapWidth { get; private set; }
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

		public WrappedText(string rawStr, Font font, uint fontSize, uint width) : this()
		{
			rawText = rawStr;
			FontReference = font;
			FontReferenceSize = fontSize;
			WrapWidth = width;
			Strings = new List<string>();
			CreateTextStrings();
		}

		public void ChangeText(string rawStr)
		{
			rawText = rawStr;
			Strings = new List<string>();
			CreateTextStrings();
		}

		public void AppendText(string str, bool space)
		{
			if (space)
				str = " " + str;
			CreateTextStrings(str);
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

		public void RemoveLine(int line)
		{
			Strings.RemoveAt(line);
		}

		public List<string> StringHead(int characters)
		{
			var newWrappedText = new List<string>();

			if (characters == 0)
				return newWrappedText;
			if (characters <= EndPosition+1)
			{
				int row = 1;
				int column = 1;
				foreach (string str in Strings) {
					if ((column + str.Length) <= characters) {
						++row;
						characters -= str.Length;
					}
					else {
						column = characters;
						break;
					}
				}

				var newStrings = new List<string>();
				for (int i=0; i<row; i++)
					newStrings.Add(string.Copy(Strings[i]));
					
				if (column > 0) newStrings[row-1] = Strings[row-1].Substring(0, column);

				return newStrings;
			}

			throw new ArgumentException();
		}

		string rawText;

		void CreateTextStrings(string stringToAdd = "")
		{
			var words = (stringToAdd == "")
            	? rawText.Split(' ').ToList()
            	: stringToAdd.Split(' ').ToList();

			string line;
			if (Strings.Count > 0)
			{
				line = Strings.Last();
				Strings.RemoveAt(Strings.Count-1);
			}
			else line = "";

			while (words.Count > 0) {

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
				line = "";
			}
		}

		int CalculateWidthOfString(string str)
		{
			return (int)(new Text(str, FontReference, FontReferenceSize).GetLocalBounds().Width);
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
		public double TextAppearanceSpeed { get; set; }
		public bool AnimationActive { get; private set; }

		int scrollAmount;
		public int ScrollAmount { 
			get { return scrollAmount; }
			private set
			{
				scrollAmount = value;
				/*for (int i=0; i<VisibleText.Strings.Count; i++)
					if (i < scrollAmount)
						VisibleText.RemoveLine(i);*/
			}
		}

		public AnimatedWrappedText(string rawStr, Font font, uint fontsize, uint width)
		{
			AnimationActive = false;
			FullText    = new WrappedText(rawStr, font, fontsize, width);
			VisibleText = new WrappedText("", font, fontsize, width);

			for (int i=0; i<VisibleText.Strings.Count; i++)
			{
				string invisibleString = "";
				foreach (char c in VisibleText.Strings[i])
					invisibleString += '\v';
				VisibleText.Strings[i] = invisibleString;
			}

			invisibleCharacters = FullText.EndPosition + 1;
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
					if (mostRecentCharacter == ',')
						timeUntilNextCharacter *= commaPause;
				}
			}
		}

		public void ClearAndRestart(string rawStr)
		{
			Font font = FullText.FontReference;
			uint size = FullText.FontReferenceSize;
			FullText = new WrappedText(rawStr, font, size, FullText.WrapWidth);
			VisibleText = new WrappedText("", font, size, FullText.WrapWidth);
			invisibleCharacters = FullText.EndPosition;
		}

		readonly int textboxRows = 4;
		int invisibleCharacters;

		double timeUntilNextCharacter;
		char mostRecentCharacter;
		readonly double commaPause = 15;

		void AddCharacter()
		{
			--invisibleCharacters;
			if (invisibleCharacters < 0) {
				AnimationActive = false;
				return;
			}

			if (VisibleText.Strings.Count > ScrollAmount + textboxRows)
				++ScrollAmount;

			mostRecentCharacter = FullText.GetCharacterAtPosition(
				FullText.EndPosition - invisibleCharacters
			);

			var newWrappedText = new WrappedText(
				"",
				FullText.FontReference,
				FullText.FontReferenceSize,
				FullText.WrapWidth); 

			newWrappedText.Strings = FullText.StringHead(FullText.EndPosition+1 - invisibleCharacters);
			VisibleText = newWrappedText;

			//for (int i=0; i<VisibleText.Strings.Count; i++)
			//	if (i < scrollAmount)
			//		VisibleText.RemoveLine(i);
			while (VisibleText.Strings.Count > textboxRows) {
				VisibleText.RemoveLine(0);
			}
		}
	}

	class Textbox : Drawable
	{
		public static readonly Texture BoxTexture = new Texture("resources/textbox.png");
		public static readonly Font TextFont = new Font("resources/rix.ttf");
		public static readonly uint FontSize = 8;
		public static readonly uint LineSpacing = 5;

		public static readonly Vector2f Position = new Vector2f(0, 135);
		public static readonly uint BorderWidth = 8;

		public AnimatedWrappedText Content { get; private set; }
		
		public Textbox(AnimatedWrappedText textSource)
		{
			boxSprite = new Sprite(BoxTexture);
			boxSprite.Position = Position;
			Content = textSource;
		}

		public void Draw(RenderTarget target, RenderStates states)
		{
			target.Draw(boxSprite, states);

			int i = 0;
			foreach (string str in Content.VisibleText.Strings)
			{
				var text = new Text(str, TextFont, FontSize);
				text.Position = boxSprite.Position + new Vector2f(
					BorderWidth, 
					BorderWidth + (FontSize+LineSpacing) * i);
				text.Color = Color.Black;
				target.Draw(text, states);
				i++;
			}
		}
		
		readonly Sprite boxSprite;
	}
}
