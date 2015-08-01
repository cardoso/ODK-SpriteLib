using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Represents a DarkEden Alpha Sprite Package (ASPK)
    /// </summary>
    public class CAlphaSpritePack
    {
        /// <summary>
        /// Name of the Sprite Pack for internal use.
        /// </summary>
        public string Name;

        /// <summary>
        /// The collection of sprites which belong to this instance of CAlphaSpritePack
        /// </summary>
        public List<CAlphaSprite> Sprites { get; private set; }

        /// <summary>
        /// Determines whether or not this instance was properly initialized.
        /// </summary>
        public bool Initialized = false;

        /// <summary>
        /// Initializes a new instance of CAlphaSpritePack from a filestream.
        /// </summary>
        /// <param name="file">The filestream as it would be read by DarkEden.</param>
        public CAlphaSpritePack(ref FileStream file, string name = null)
        {
            if (file.Length == 0) return;

            if (name != null) this.Name = name;

            Sprites = new List<CAlphaSprite>();

            byte[] _spc = new byte[2];
            file.Read(_spc, 0, 2);
            UInt16 spritecount = BitConverter.ToUInt16(_spc, 0);

            for (int i = 0; i < spritecount; i++)
            {
                string str_sprname = "{0}[{1}]";
                object[] objs_sprname = {this.Name, i};

                CAlphaSprite spr = new CAlphaSprite(ref file, String.Format(str_sprname, objs_sprname));

                Sprites.Add(spr);
            }

            this.Initialized = true;
        }

        /// <summary>
        /// Initializes an instance of CAlphaSpritePack with no sprites.
        /// </summary>
        public CAlphaSpritePack()
        {
            this.Sprites = new List<CAlphaSprite>();
            this.Initialized = true;
        }

        /// <summary>
        /// Save this CAlphaSpritePack instance to an ASPK file.
        /// This can be read by DarkEden if accompanied by an ASPKI file (Index).
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="makeAspki"></param>
        public void SaveToFile(string filename, bool makeAspki = true)
        {
            FileStream file = File.Create(filename);

            byte[] _sprcount = BitConverter.GetBytes(this.Sprites.Count);

            file.Write(_sprcount, 0, 2);

            foreach (CAlphaSprite s in Sprites)
            {
                byte[] sprbytes = s.GetBytes();
                file.Write(sprbytes, 0, sprbytes.Length);
            }

            if(makeAspki)
            {
                string spkiname = Path.GetDirectoryName(filename) + 
                    "/" + Path.GetFileNameWithoutExtension(filename) + ".aspki";

                FileStream spkifile = File.Create(spkiname);

                byte[] spkibytes = GenerateIndexFile();

                spkifile.Write(spkibytes, 0, spkibytes.Length);

                spkifile.Close();
            }

            file.Close();
        }

        /// <summary>
        /// Returns a byte array which represents a valid Alpha Sprite Package Index file (ASPKI) for this CAlphaSpritePack instance.
        /// It's required by DarkEden to read the corresponding Alpha Sprite Package.
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