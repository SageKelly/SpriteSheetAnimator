using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpriteSheetAnimator
{
    class FilenameText
    {
        string sText;
        Vector2 position;
        int iListPosition;

        public FilenameText(string _text, Vector2 _pos, int i_list_pos)
        {
            sText = _text;
            position = _pos;
            iListPosition = i_list_pos;
        }

        #region Properties
        #region Text
        public string Text
        {
            get
            {
                return sText;
            }
            set
            {
                sText = value;
            }
        }
        #endregion
        #region Position Properties
        #region Position
        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        #endregion
        #region PosX
        public float PosX
        {
            get
            {
                return position.X;
            }
            set
            {
                position.X = value;
            }
        }
        #endregion
        #region PosY
        public float PosY
        {
            get
            {
                return position.Y;
            }
            set
            {
                position.Y = value;
            }
        }
        #endregion
        #endregion
        #region ListPosition
        public int ListPosition
        {
            get { return iListPosition; }
        }
#endregion
        #endregion
    }
}
