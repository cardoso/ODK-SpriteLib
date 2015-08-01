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
    /// Contains some helper methods for the CSprite class.
    /// </summary>
    public static class CSpriteHelper
    {
        static int Rmask = 0xF800; // 1111100000000000
        static int Gmask = 0x7E0;  // 0000011111100000
        static int Bmask = 0x1F;   // 0000000000011111

        /// <summary>
        /// Returns a System.Drawing.Color from a given CSprite color (16-bit unsigned integer).
        /// </summary>
        /// <param name="n">16-bit unsigned integer which represents a color.</param>
        /// <returns></returns>
        public static Color GetColorFrom16bit(UInt16 n)
        {
            byte R = (byte)((n & Rmask) >> 8);
            byte G = (byte)((n & Gmask) >> 3);
            byte B = (byte)((n & Bmask) << 3);

            Color color = Color.FromArgb(R, G, B);

            return color;
        }

        /// <summary>
        /// Returns a CSprite color (16-bit unsigned int) from a given System.Drawing.Color
        /// </summary>
        /// <param name="c">The System.Drawing.Color which will be converted.</param>
        /// <returns></returns>
        public static UInt16 Get16bitFromColor(Color c)
        {
            return (UInt16)(((c.R >> 3) << 11) | ((c.G >> 2) << 5) | ((c.B >> 3)));
        }
    }
}
