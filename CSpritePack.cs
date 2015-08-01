using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Represents a DarkEden Sprite Package (SPK)
    /// </summary>
    public class CSpritePack
    {
        /// <summary>
        /// Name of the Sprite Pack for internal use.
        /// </summary>
        public string Name;

        /// <summary>
        /// The collection of sprites which belong to this instance of CSpritePack
        /// </summary>
        public List<CSprite> Sprites { get; private set; }

        /// <summary>
        /// Determines whether or not this instance was properly initialized.
        /// </summary>
        public bool Initialized = false;

        /// <summary>
        /// Initializes a new instance of CSpritePack from a valid FileStream.
        /// </summary>
        /// <param name="file">The FileStream as it would be read by DarkEden.</param>
        public CSpritePack(ref FileStream file, string name = null)
        {
            if (file.Length == 0) return;

            if (name != null) this.Name = name;

            this.Sprites = new List<CSprite>();

            byte[] _spc = new byte[2];
            file.Read(_spc, 0, 2);
            UInt16 spritecount = BitConverter.ToUInt16(_spc, 0);

            //int o = 2;
            for (int i = 0; i < spritecount; i++)
            {
                string str_sprname = "{0}[{1}]";
                object[] objs_sprname = {this.Name, i};

                CSprite spr = new CSprite(ref file, String.Format(str_sprname, objs_sprname));

                this.Sprites.Add(spr);
            }

            this.Initialized = true;
        }

        /// <summary>
        /// Initializes an instance of CSpritePack with no sprites.
        /// </summary>
        public CSpritePack()
        {
            this.Sprites = new List<CSprite>();
            this.Initialized = true;
        }

        /// <summary>
        /// Save this CSpritePack instance to a SPK file.
        /// This can be read by DarkEden if accompanied by a SPKI file (Index).
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="makeSpki"></param>
        public void SaveToFile(string filename, bool makeSpki = true)
        {
            FileStream file = File.Create(filename);

            byte[] _sprcount = BitConverter.GetBytes(this.Sprites.Count);

            file.Write(_sprcount, 0, 2);

            foreach (CSprite s in Sprites)
            {
                byte[] sprbytes = s.GetBytes();
                file.Write(sprbytes, 0, sprbytes.Length);
            }

            if(makeSpki)
            {
                string spkiname = Path.GetDirectoryName(filename) + 
                    "/" + Path.GetFileNameWithoutExtension(filename) + ".spki";

                FileStream spkifile = File.Create(spkiname);

                byte[] spkibytes = GenerateIndexFile();

                spkifile.Write(spkibytes, 0, spkibytes.Length);

                spkifile.Close();
            }

            file.Close();
        }

        /// <summary>
        /// Returns a byte array which represents a valid Sprite Package Index file (SPKI) for this CSpritePack instance.
        /// It's required by DarkEden to read the corresponding Sprite Package.
        /// It contains the offsets of all sprites which makes possible for the game to stream assets.
        /// </summary>
        /// <returns>Byte array</returns>
        /// A separate class or structure will be created for this in a later version.
        public byte[] GenerateIndexFile()
        {
            UInt16 count = (UInt16)this.Sprites.Count;
            int[] offsets = new int[count];

            int o = 2;
            for (int i = 0; i < this.Sprites.Count; i++)
            {
                offsets[i] = o;
                o += this.Sprites[i].ByteCount;
            }

            byte[] file = new byte[2 + count * 4];

            BitConverter.GetBytes(count).CopyTo(file, 0);

            for (int i = 0; i < count; i++)
            {
                BitConverter.GetBytes(offsets[i]).CopyTo(file, 2 + i * 4);
            }

            return file;
        }

    }
}