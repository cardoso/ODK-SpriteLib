using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Contains some helper methods for the CSpritePack class.
    /// </summary>
    public static class CSpritePackHelper
    {
        /// <summary>
        /// Loads a Sprite Pack from an existing file.
        /// </summary>
        /// <param name="filename">The path of the sprite pack file (.spk)</param>
        /// <returns></returns>
        public static CSpritePack LoadFromFile(string filename)
        {
            DirectoryInfo dir = new DirectoryInfo(filename);

            CSpritePack spk;

            FileStream filestream = File.Open(filename, FileMode.Open);

            spk = new CSpritePack(ref filestream, dir.Name);

            filestream.Close();

            return spk;
        }
    }
}