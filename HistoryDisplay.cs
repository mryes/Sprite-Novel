using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace SpriteNovel
{
    class HistoryDisplay : Drawable
    {
        public readonly uint NumberOfLines  = 12;
        public readonly uint BorderWidth    = Textbox.BorderWidth;
        public readonly uint BorderHeight   = 8;

        public bool Active { get; private set; }

        public Script SessionScript { get; private set; }

        public HistoryDisplay(Script flatScript, Window window)
        {
            SessionScript = flatScript;

            Active = false;

            window.MouseButtonPressed += (sender, e) => Active = false;
            window.MouseWheelMoved    += UpdateScrollPosition;
        }

        public void Update(Script flatScript)
        {
            SessionScript = flatScript;

            drawableTextLines = new List<Text>();

            uint spaceBetweenLines = Textbox.FontSize + Textbox.LineSpacing;

            float yPosition = Stage.ScreenHeight - BorderHeight
                              + (currentBottom-1) * spaceBetweenLines;

            foreach (var block in textBlocks.Reverse<TextBlock>()) {

                yPosition -= spaceBetweenLines * block.Text.Strings.Count;
                int lineNumWithinBlock = 0;

                foreach (var str in block.Text.Strings) {

                    var text = new Text(str.TrimStart(' '), Textbox.TextFont, Textbox.FontSize);
                    text.Position = new Vector2f(
                        BorderWidth,
                        yPosition + lineNumWithinBlock * spaceBetweenLines);

                    text.Color = block.TextColor;

                    if ((text.Position.Y < Stage.ScreenHeight - 10)
                        && (text.Position.Y > 30))
                        drawableTextLines.Add(text);

                    ++lineNumWithinBlock;
                }
            } 
        }

        public void UpdateScrollPosition(object sender, EventArgs e)
        {
            var args = (MouseWheelEventArgs)e;
            if (args.Delta != 0) {

                if (args.Delta > 0) {
                    if (!Active) {
                        Active = true;
                        ConstructTextBlocks();
                    }
                }

                currentBottom += args.Delta;

                if (currentBottom <= 0) {
                    currentBottom = 0;
                    Active = false;
                }

                int lines = 0;
                foreach (var block in textBlocks)
                    lines += block.Text.Strings.Count;

                if (args.Delta > 0){
                    if ((lines - currentBottom) + 1 <= NumberOfLines)
                        currentBottom -= args.Delta;
                    if (currentBottom < 1)
                        currentBottom = 1;
                }

            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (!Active)
                return;

            target.Draw(historyScreen);
            foreach (var text in drawableTextLines)
                target.Draw(text);
        }

        void ConstructTextBlocks()
        {
            currentBottom = 0;

            textBlocks = new List<TextBlock>();

            var wrappedText = new WrappedText(
                "",
                Textbox.TextFont,
                Textbox.FontSize,
                Stage.ScreenWidth - Textbox.BorderWidth * 2);

            for (int i=0; i<SessionScript.Count; i++)
            {
                List<string> directiveNames = SessionScript[i].Directives.Select(x => x.Name).ToList();
                if (directiveNames.Contains("c")) {

                    wrappedText = new WrappedText(
                        "",
                        wrappedText.FontReference,
                        wrappedText.FontReferenceSize,
                        wrappedText.WrapWidth);

                    // I need a text block to contain both the wrapped text
                    // and the color of the text
                    var color = Color.Black;
                    if (directiveNames.Contains("character")) {
                        color = Stage.CharacterSettings
                             [SessionScript[i].Directives.First(x => x.Name == "character").Value]
                            .TextColor;
                    }

                    wrappedText.AppendText(SessionScript[i].Dialogue, true);

                    var textBlock = new TextBlock(wrappedText, color);
                    textBlocks.Add(textBlock);

                } else {
                    wrappedText.AppendText(SessionScript[i].Dialogue, true);
                }
                
            }
        }

        int currentBottom = 0;

        class TextBlock
        {
            public readonly WrappedText Text = new WrappedText();
            public readonly Color TextColor  = Color.Black;

            public TextBlock(WrappedText text, Color color)
            {
                Text = text;
                TextColor = color;
            }

        }

        List<TextBlock> textBlocks = new List<TextBlock>();
        List<Text> drawableTextLines = new List<Text>();

        static readonly Texture historyTexture = new Texture("resources/historyscreen.png");
        readonly Sprite historyScreen = new Sprite(historyTexture);

    }
}

