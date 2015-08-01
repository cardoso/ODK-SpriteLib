using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteLib
{
    /// <summary>
    /// Represents a DarkEden Shadow Sprite Package (SSPK)
    /// </summary>
    public class CShadowSpritePack
    {
        /// <summary>
        /// Name of the Shadow Sprite Pack for internal use.
        /// </summary>
        public string Name;

        /// <summary>
        /// The collection of sprites which belong to this instance of CShadowSpritePack
        /// </summary>
        public List<CShadowSprite> Sprites { get; private set; }

        /// <summary>
        /// Determines whether or not this instance was properly initialized.
        /// </summary>
        public bool Initialized = false;

        /// <summary>
        /// Initializes a new instance of CShadowSpritePack from a file stream.
        /// </summary>
        /// <param name="file">The file stream as it would be read by DarkEden.</param>
        public CShadowSpritePack(ref FileStream file, string name = null)
        {
            if (file.Length == 0) return;

            if (name != null) this.Name = name;

            Sprites = new List<CShadowSprite>();

            byte[] _spc = new byte[2];

            file.Read(_spc, 0, 2);

            UInt16 spritecount = BitConverter.ToUInt16(_spc, 0);

            for (int i = 0; i < spritecount; i++)
            {

                string str_sprname = "{0}[{1}]";
                object[] objs_sprname = { this.Name, i };

                CShadowSprite ssp = new CShadowSprite(ref file, String.Format(str_sprname, objs_sprname));

                Sprites.Add(ssp);
            }

            this.Initialized = true;
        }

        /// <summary>
        /// Initializes an instance of CShadowSpritePack with no sprites.
        /// </summary>
        public CShadowSpritePack()
        {
            this.Sprites = new List<CShadowSprite>();
            this.Initialized = true;
        }

        /// <summary>
        /// Save this CShadowSpritePack instance to a SSPK file.
        /// This can be read by DarkEden if accompanied by a SSPKI file (Index).
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="makeSspki"></param>
        public void SaveToFile(string filename, bool makeSspki = true)
        {
            FileStream file = File.Create(filename);

            byte[] _sprcount = BitConverter.GetBytes(this.Sprites.Count);

            file.Write(_sprcount, 0, 2);

            foreach (CShadowSprite s in Sprites)
            {
                byte[] sprbytes = s.GetBytes();
                file.Write(sprbytes, 0, sprbytes.Length);
            }

            if(makeSspki)
            {
                string spkiname = Path.GetDirectoryName(filename) + 
                    "/" + Path.GetFileNameWithoutExtension(filename) + ".sspki";

                FileStream spkifile = File.Create(spkiname);

                byte[] spkibytes = GenerateIndexFile();

                spkifile.Write(spkibytes, 0, spkibytes.Length);

                spkifile.Close();
            }

            file.Close();
        }

        /// <summary>
        /// Returns a byte array which represents a valid Shadow Sprite Package Index file (SSPKI) for this CShadowSpritePack instance.
        /// It's required by DarkEden to read the corresponding Shadow Sprite Package.
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