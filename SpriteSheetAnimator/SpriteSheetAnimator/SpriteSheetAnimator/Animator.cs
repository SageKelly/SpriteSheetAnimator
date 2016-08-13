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
    public class Animator : Microsoft.Xna.Framework.DrawableGameComponent
    {
        /* NOTE TO SELF: make an animation mascot for program (film-reel guy),
         * and have his head spin at the same rate the fps, while having the animations show on the film 
         * dynamically stretched to fit the sprite's size (X and Y).*/

        /*TimeSpan gameTime;
        Rectangle SpriteBox;
        List<Texture2D> SpriteSheets;
        const int DIVIDE_TIME = 1000;
        int x_offset;
        int y_offset;
        int width;
        int height;
        int frame;
        int fps;
        float frame_increm;
        bool isRunning;*/

        public Animator(Game game)
            : base(game)
        {
        }

        protected override void LoadContent()
        {
           base.LoadContent();
        }

        public override void Initialize()
        {

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            
            base.Draw(gameTime);
        }
    }
}
