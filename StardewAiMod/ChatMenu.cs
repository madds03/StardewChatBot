using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StardewAiMod
{
    public class ChatMenu : IClickableMenu
    {
        public ChatMenu() : base(
            Game1.uiViewport.Width / 2 - 300,
            Game1.uiViewport.Height / 2 - 200,
            600,
            400,
            true)
        {
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
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

            string title = "Stardew AI Chat";
            string body = "Button works! Chat UI goes here.";

            b.DrawString(
                Game1.dialogueFont,
                title,
                new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 32),
                Color.Black
            );

            b.DrawString(
                Game1.smallFont,
                body,
                new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 100),
                Color.Black
            );

            this.drawMouse(b);
        }
    }
}