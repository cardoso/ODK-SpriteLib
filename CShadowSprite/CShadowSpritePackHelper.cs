using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Contains some helper methods for the CShadowSpritePack class.
    /// </summary>
    public static class CShadowSpritePackHelper
    {
        /// <summary>
        /// Loads a Shadow Sprite Pack from an existing file.
        /// </summary>
        /// <param name="filename">The path of the shadow sprite pack file (.sspk)</param>
        /// <returns></returns>
        public static CShadowSpritePack LoadFromFile(string filename)
        {
            DirectoryInfo dir = new DirectoryInfo(filename);

            CShadowSpritePack sspk;

            FileStream filestream = File.Open(filename, FileMode.Open);


            sspk = new CShadowSpritePack(ref filestream, dir.Name);

            filestream.Close();

            return sspk;
        }
    }
}