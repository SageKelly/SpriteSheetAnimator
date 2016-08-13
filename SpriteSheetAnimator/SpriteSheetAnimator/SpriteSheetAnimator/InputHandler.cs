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
    public interface IInputHandler
    {
        KeyboardState KBState { get; set; }
        KeyboardState PrevKBState { get; set; }
    };
    public class InputHandler : Microsoft.Xna.Framework.GameComponent, IInputHandler
    {

        KeyboardState kbState;
        KeyboardState prevKBState;

        public InputHandler(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IInputHandler), this);
        }

        public override void Initialize()
        {

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (kbState.IsKeyDown(Keys.Escape))
                Game.Exit();
            base.Update(gameTime);
        }

        public KeyboardState KBState
        {
            get
            {
                return kbState;
            }
            set
            {
                kbState = value;
            }
        }

        public KeyboardState PrevKBState
        {
            get
            {
                return prevKBState;
            }
            set
            {
                prevKBState = value;
            }
        }
    }
}
