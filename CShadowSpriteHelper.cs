using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Contains some helper methods for the CShadowSprite class.
    /// </summary>
    static class CShadowSpriteHelper
    {
        /// <summary>
        /// Returns a System.Drawing.Color from a given CShadowSprite color (16-bit unsigned integer).
        /// </summary>
        /// <param name="n">16-bit unsigned integer which will be converted.</param>
        /// <returns></returns>
        /*public static Color GetColorFrom16bit(UInt16 n)
        {
            byte R = (byte)((n >> 11) << 3);
            byte G = (byte)((((n >> 5) << 5) >> 5) << 2);
            byte B = (byte)(((n << 11) >> 11) << 3);

            Color color = Color.FromArgb(R, G, B);

            return color;
        }

        /// <summary>
        /// Returns a CShadowSprite color (16-bit unsigned int) from a given System.Drawing.Color
        /// </summary>
        /// <param name="c">The System.Drawing.Color which will be converted.</param>
        /// <returns></returns>
        public static UInt16 Get16bitFromColor(Color c)
        {
            return (UInt16)(((c.R >> 3) << 11) | ((c.G >> 2) << 5) | ((c.B >> 3)));
        }*/
    }
}
