using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace SpriteSheetAnimator
{
    public sealed partial class FPS : Microsoft.Xna.Framework.DrawableGameComponent
    {

        float fps;
        float timeSinceLastUpdate = 0.0f;
        float updateInterval = 1.0f;
        float framecount = 0;


        public FPS(Game game)
            : this(game, false, false, game.TargetElapsedTime)
        { }

        public FPS(Game game, bool synchWithVerticalRetrace, bool isFixedTimeStep, TimeSpan targetElapseTime)
            : base(game)
        {
            GraphicsDeviceManager graphics =
                (GraphicsDeviceManager)Game.Services.GetService(
                typeof(IGraphicsDeviceManager));

            graphics.SynchronizeWithVerticalRetrace = synchWithVerticalRetrace;
            game.IsFixedTimeStep = isFixedTimeStep;
            game.TargetElapsedTime = targetElapseTime;
        }

        public override void Draw(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            framecount++;

            timeSinceLastUpdate += elapsed;

            if (timeSinceLastUpdate > updateInterval)
            {
                fps = framecount / timeSinceLastUpdate;

                Game.Window.Title = "FPS: " + fps.ToString();

                framecount = 0;
                timeSinceLastUpdate -= updateInterval;

            }

            base.Draw(gameTime);
        }

    }
}
