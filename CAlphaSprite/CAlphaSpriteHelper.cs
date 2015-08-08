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
    /// Contains some helper methods for the CAlphaSprite class.
    /// </summary>
    static class CAlphaSpriteHelper
    {
        static int Rmask = 0xF800; // 1111100000000000
        static int Gmask = 0x3E0;  // 0000001111100000
        static int Amask = 0x1F;   // 0000000000011111

        //static int Wmask = 0xE0;
        
        /// <summary>
        /// Returns a System.Drawing.Color from a given CAlphaSprite color (16-bit unsigned integer).
        /// </summary>
        /// <param name="n">16-bit unsigned integer which will be converted.</param>
        /// <returns></returns>
        public static Color GetColorFrom16bit(UInt16 n, byte segOffset)
        {
            //RGA 5:6:5
            //byte R = (byte)((n >> 11) << 3);
            //byte G = (byte)(((n << 5) >> 5) << 2);
            //byte A = (byte)((n << 11) >> 8);
            //RGA 5:6:5 Sprite
            //uint n2 = n;
            //byte R = (byte)((n2 >> 11) << 3);
            //byte G = (byte)(((n2 << 21) >> 26) << 2);// ((((n >> 5) << 5) >> 5) << 2);
            //byte A = (byte)(((n2 << 27) >> 27) << 3);// >> 11) << 3);
            //Color color = Color.FromArgb(A, R, G, 0);
            //return color;
            //RGBA 4:4:4:4
            //byte R = (byte)((n >> 12) << 4);
            //byte G = (byte)((((n >> 8) << 8) >> 8) << 4);
            //byte B = (byte)((n >> 4) << 4);
            //byte A = (byte)((n << 3));
            //Color color = Color.FromArgb(A, R, G, B);
            //return color;

            //Color color = Color.FromArgb(A, G, R, 0);

            //return color;

            byte R = (byte)((n & Rmask) >> 8);
            byte G = (byte)((n & Gmask) >> 2);
            byte A = (byte)((n & Amask) << 3);

            Color color = Color.FromArgb(A, R, 0, G);

            return color;
        }

        /// <summary>
        /// Returns a CAlphaSprite color (16-bit unsigned int) from a given System.Drawing.Color
        /// </summary>
        /// <param name="c">The System.Drawing.Color which will be converted.</param>
        /// <returns></returns>
        public static UInt16 Get16bitFromColor(Color c)
        {
            //return (UInt16)(((c.R >> 4) << 12) | ((c.G >> 4) << 8) | ((c.B >> 5) << 5) | (c.A >> 3));
            //return (UInt16)(((UInt16)(c.R >> 3) << 11) | ((UInt16)(c.G >> 2) << 5) | (c.A >> 3));
            /*UInt16 R = c.R;
            UInt16 G = c.G;
            UInt16 A = c.A;

            UInt16 _R = (UInt16)((R >> 3) << 11);//(UInt16)((c.R >> 3) << 11);
            UInt16 _G = (UInt16)((G >> 2) << 5);
            UInt16 _A = (UInt16)((A >> 3));

            UInt16 _short = (UInt16)(_R | _G | _A);

            return _short;*/

            return (UInt16)(((c.R >> 3) << 11) | ((c.G >> 3) << 5) | ((c.A >> 3)));


            /*if (c.A == 0xFF)
            {
                return (UInt16)(((c.R >> 3) << 11) | ((c.G >> 3) << 6) | ((c.B >> 3)));
            }
            else
            {
                return (UInt16)(((c.R >> 3) << 11) | ((c.G >> 3) << 6) | 0x20 | ((c.A >> 3)));
            }*/
        }
    }
}