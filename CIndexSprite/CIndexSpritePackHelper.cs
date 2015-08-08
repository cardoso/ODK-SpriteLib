using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Contains some helper methods for the CIndexSpritePack class.
    /// </summary>
    public static class CIndexSpritePackHelper
    {
        /// <summary>
        /// Loads an Index Sprite Pack from an existing file.
        /// </summary>
        /// <param name="filename">The path of the index sprite pack file (.ispk)</param>
        /// <returns></returns>
        public static CIndexSpritePack LoadFromFile(string filename)
        {
            DirectoryInfo dir = new DirectoryInfo(filename);

            CIndexSpritePack spk;

            FileStream filestream = File.Open(filename, FileMode.Open);

            spk = new CIndexSpritePack(ref filestream, dir.Name);

            return spk;
        }

        public static byte[] GenerateIndexFile(ref CIndexSpritePack[] packlist)
        {
            MemoryStream stream = new MemoryStream();
            stream.Position = 2;

            UInt16 sprcount = 0;
            UInt32 offset = 2;

            for (int q = 0; q < packlist.Length; q++)
            {
                sprcount += (UInt16)packlist[q].Sprites.Count;

                for (int i = 0; i < packlist[q].Sprites.Count; i++)
                {
                    stream.Write(BitConverter.GetBytes(offset), 0, 4);

                    offset += (UInt32)packlist[q].Sprites[i].ByteCount;

                    //packlist[i] = null;
                }
            }

            stream.Position = 0;
            stream.Write(BitConverter.GetBytes(sprcount), 0, 2);
            stream.Position = 0;

            byte[] file = new byte[stream.Length];
            stream.Read(file, 0, file.Length);

            return file;
        }
    }
}