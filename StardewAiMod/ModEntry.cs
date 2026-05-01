using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewAiMod
{
    public class ModEntry : Mod
    {
        private Texture2D? buttonTexture;
        private Rectangle buttonBounds;

        public override void Entry(IModHelper helper)
        {
            this.buttonTexture = helper.ModContent.Load<Texture2D>("assets/chatButton.png");

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            Monitor.Log("Mod loaded!", LogLevel.Info);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            UpdateButtonBounds();
        }

        private void UpdateButtonBounds()
        {
            int size = 110;     // Bigger button
            int padding = 20;

            int x = padding; // Left side
            int y = Game1.uiViewport.Height - size - padding; // Bottom

            this.buttonBounds = new Rectangle(x, y, size, size);
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.eventUp || Game1.activeClickableMenu != null)
                return;

            if (this.buttonTexture == null)
                return;

            UpdateButtonBounds();

            e.SpriteBatch.Draw(this.buttonTexture, this.buttonBounds, Color.White);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // 🚨 FIX: Prevent key input from interfering while ChatMenu is open
            if (Game1.activeClickableMenu is ChatMenu)
                return;

            // Press K → open chat menu
            if (e.Button == SButton.K)
            {
                Game1.activeClickableMenu = new ChatMenu();
                return;
            }

            // Click button → open chat menu
            if (e.Button == SButton.MouseLeft)
            {
                Point mouse = Game1.getMousePosition();

                if (this.buttonBounds.Contains(mouse) && Game1.activeClickableMenu == null)
                {
                    Game1.activeClickableMenu = new ChatMenu();
                }
            }
        }
    }
}