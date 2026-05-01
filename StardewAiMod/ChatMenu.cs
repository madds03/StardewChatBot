using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StardewAiMod
{
    public class ChatMenu : IClickableMenu
    {
        private readonly HttpClient httpClient = new HttpClient();

        private TextBox questionBox;
        private ClickableTextureComponent askButton;

        private string responseText = "Type a Stardew question, then click Ask or press Enter.";

        private int scrollOffset = 0;
        private int maxScroll = 0;

        public ChatMenu() : base(
            Game1.uiViewport.Width / 2 - 400,
            Game1.uiViewport.Height / 2 - 275,
            800,
            550,
            false)
        {
            this.questionBox = new TextBox(
                Game1.content.Load<Texture2D>("LooseSprites\\textBox"),
                null,
                Game1.smallFont,
                Game1.textColor
            );

            this.questionBox.X = this.xPositionOnScreen + 50;
            this.questionBox.Y = this.yPositionOnScreen + 120;
            this.questionBox.Width = 520;
            this.questionBox.Height = 48;
            this.questionBox.Text = "";
            this.questionBox.Selected = true;

            // Allow much longer questions
            this.questionBox.limitWidth = false;

            Game1.keyboardDispatcher.Subscriber = this.questionBox;

            this.askButton = new ClickableTextureComponent(
                new Rectangle(this.xPositionOnScreen + 600, this.yPositionOnScreen + 115, 120, 60),
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64),
                0.9f
            );
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            bool clickedQuestionBox =
                x >= this.questionBox.X &&
                x <= this.questionBox.X + this.questionBox.Width &&
                y >= this.questionBox.Y &&
                y <= this.questionBox.Y + this.questionBox.Height;

            this.questionBox.Selected = clickedQuestionBox;

            if (clickedQuestionBox)
                Game1.keyboardDispatcher.Subscriber = this.questionBox;

            if (this.askButton.containsPoint(x, y))
            {
                _ = AskServer();
                Game1.playSound("smallSelect");
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.E)
                return;

            if (key == Keys.Enter)
            {
                _ = AskServer();
                return;
            }

            base.receiveKeyPress(key);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            if (direction < 0)
                this.scrollOffset += 2;
            else if (direction > 0)
                this.scrollOffset -= 2;

            if (this.scrollOffset < 0)
                this.scrollOffset = 0;

            if (this.scrollOffset > this.maxScroll)
                this.scrollOffset = this.maxScroll;
        }

        private async System.Threading.Tasks.Task AskServer()
        {
            string question = this.questionBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(question))
            {
                this.responseText = "Please type a question first.";
                this.scrollOffset = 0;
                return;
            }

            this.responseText = "Thinking...";
            this.scrollOffset = 0;

            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    question = question
                });

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync("http://127.0.0.1:8000/ask", content);
                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("answer", out JsonElement answerElement))
                    this.responseText = answerElement.GetString() ?? "No answer returned.";
                else if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement))
                    this.responseText = "Server error: " + errorElement.GetString();
                else
                    this.responseText = "Unexpected response from server.";

                this.scrollOffset = 0;
            }
            catch
            {
                this.responseText = "Could not reach local chatbot server.";
                this.scrollOffset = 0;
            }

            Game1.keyboardDispatcher.Subscriber = this.questionBox;
            this.questionBox.Selected = true;
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(
                this.xPositionOnScreen,
                this.yPositionOnScreen,
                this.width,
                this.height,
                false,
                true
            );

            b.DrawString(
                Game1.dialogueFont,
                "Stardew AI Chat",
                new Vector2(this.xPositionOnScreen + 50, this.yPositionOnScreen + 45),
                Color.Black
            );

            b.DrawString(
                Game1.smallFont,
                "Question:",
                new Vector2(this.xPositionOnScreen + 50, this.yPositionOnScreen + 95),
                Color.Black
            );

            this.questionBox.Draw(b);
            this.askButton.draw(b);

            b.DrawString(
                Game1.smallFont,
                "Ask",
                new Vector2(this.xPositionOnScreen + 640, this.yPositionOnScreen + 132),
                Color.Black
            );

            b.DrawString(
                Game1.smallFont,
                "Answer:",
                new Vector2(this.xPositionOnScreen + 50, this.yPositionOnScreen + 205),
                Color.Black
            );

            DrawScrollableAnswer(b);

            this.drawMouse(b);
        }

        private void DrawScrollableAnswer(SpriteBatch b)
        {
            int answerX = this.xPositionOnScreen + 50;
            int answerY = this.yPositionOnScreen + 240;
            int answerWidth = 680;
            int answerHeight = 230;

            int lineHeight = Game1.smallFont.LineSpacing + 6;
            int visibleLines = answerHeight / lineHeight;

            List<string> lines = WrapTextLines(Game1.smallFont, this.responseText, answerWidth);

            this.maxScroll = lines.Count - visibleLines;

            if (this.maxScroll < 0)
                this.maxScroll = 0;

            if (this.scrollOffset > this.maxScroll)
                this.scrollOffset = this.maxScroll;

            for (int i = 0; i < visibleLines; i++)
            {
                int lineIndex = i + this.scrollOffset;

                if (lineIndex >= lines.Count)
                    break;

                b.DrawString(
                    Game1.smallFont,
                    lines[lineIndex],
                    new Vector2(answerX, answerY + i * lineHeight),
                    Color.Black
                );
            }

            if (this.maxScroll > 0)
            {
                b.DrawString(
                    Game1.smallFont,
                    "Use mouse wheel to scroll",
                    new Vector2(this.xPositionOnScreen + 500, this.yPositionOnScreen + 505),
                    Color.DarkSlateGray
                );
            }
        }

        private static List<string> WrapTextLines(SpriteFont font, string text, float maxLineWidth)
        {
            List<string> lines = new List<string>();

            string[] paragraphs = text.Replace("\r", "").Split('\n');

            foreach (string paragraph in paragraphs)
            {
                string[] words = paragraph.Split(' ');
                string line = "";

                foreach (string word in words)
                {
                    if (string.IsNullOrWhiteSpace(word))
                        continue;

                    string testLine = line.Length == 0 ? word : line + " " + word;

                    if (font.MeasureString(testLine).X > maxLineWidth)
                    {
                        if (line.Length > 0)
                            lines.Add(line);

                        line = word;
                    }
                    else
                    {
                        line = testLine;
                    }
                }

                if (line.Length > 0)
                    lines.Add(line);
            }

            return lines;
        }

        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();

            if (Game1.keyboardDispatcher.Subscriber == this.questionBox)
                Game1.keyboardDispatcher.Subscriber = null;
        }
    }
}