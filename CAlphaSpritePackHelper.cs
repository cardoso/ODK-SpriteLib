using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Contains some helper methods for the CAlphaSpritePack class.
    /// </summary>
    public static class CAlphaSpritePackHelper
    {
        /// <summary>
        /// Loads an Alpha Sprite Pack from an existing file.
        /// </summary>
        /// <param name="filename">The path of the alpha sprite pack file (.aspk)</param>
        /// <returns></returns>
        public static CAlphaSpritePack LoadFromFile(string filename)
        {
            DirectoryInfo dir = new DirectoryInfo(filename);

            CAlphaSpritePack aspk;

            FileStream filestream = File.Open(filename, FileMode.Open);

            //byte[] buffer = new byte[filestream.Length];
            //filestream.Read(buffer, 0, buffer.Length);

            aspk = new CAlphaSpritePack(ref filestream, dir.Name);

            filestream.Close();

            return aspk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packlist"></param>
        /// <returns></returns>
        public static byte[] GenerateIndexFile(ref CAlphaSpritePack[] packlist)
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