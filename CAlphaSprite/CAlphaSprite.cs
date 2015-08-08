/*
[HEADER]
byte[4] BodyLen

byte[2] Width
byte[2] Height

body[height]  
[/HEADER]

[BODY]
--[ byte[1] SegCount
----[ byte[1] Offset
----[ byte[1] PixCount
[/BODY]

[FOOTER]
byte[Height*2] LineLengths
[/FOOTER]
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace SpriteLib
{
    /// <summary>
    /// Represents a single DarkEden Alpha Sprite.
    /// </summary>
    public class CAlphaSprite
    {
        //For internal use only. DarkEden does not use these.
        public int ByteCount;
        public string Name { get; set; }

        //The following fields are, in order, an accurate representation of an Alpha DarkEden sprite. (ASP)

        /// <summary>
        /// The length in bytes of the sprite's body (all its lines and segments)
        /// </summary>
        public UInt32 BodyLength;
        /// <summary>
        /// The sprite's Width.
        /// </summary>
        public UInt16 Width;
        /// <summary>
        /// The sprite's Height;
        /// </summary>
        public UInt16 Height;
        /// <summary>
        /// The sprite's collection of lines.
        /// </summary>
        public Line[] Lines;
        /// <summary>
        /// A line of segments.
        /// </summary>
        public struct Line
        {
            /// <summary>
            /// The line's length.
            /// This isn't used for reading by ODKSpriteLib, but the game uses it.
            /// </summary>
            //public UInt16 Length;
            /// <summary>
            /// The number of segments in this line.
            /// </summary>
            public byte SegmentCount;
            /// <summary>
            /// The segments in this line.
            /// </summary>
            public Segment[] Segments;
        }
        /// <summary>
        /// A segment inside a line of segments.
        /// </summary>
        public struct Segment
        {
            /// <summary>
            /// The offset of the segment in reference to the beggining of the line or to the last segment.
            /// </summary>
            public byte Offset;
            /// <summary>
            /// The amount of pixels this segment contains.
            /// </summary>
            public byte PixCount;
            /// <summary>
            /// The pixels of this segment represented by 16-bit integers.
            /// </summary>
            public UInt16[] Pixels;
        }

        public UInt16[] LineLengths;

        /// <summary>
        /// Initializes a new instance of CAlphaSprite from a filestream
        /// </summary>
        /// <param name="file">The filestream as it would be read by DarkEden.</param>
        public CAlphaSprite(ref FileStream file, string name = null)
        {
            if (name != null) this.Name = name;

            byte[] _blen = new byte[4];
            byte[] _width = new byte[2];
            byte[] _height = new byte[2];

            file.Read(_blen, 0, 4);
            file.Read(_width, 0, 2);
            file.Read(_height, 0, 2);

            this.BodyLength = BitConverter.ToUInt32(_blen, 0);
            this.Width = BitConverter.ToUInt16(_width, 0);
            this.Height = BitConverter.ToUInt16(_height, 0);

            this.Lines = new Line[this.Height];

            int pixelcount = Width * Height;

            for (int y = 0; y < Height; y++)
            {
                byte[] _segc = new byte[1];

                file.Read(_segc, 0, 1);

                Lines[y].SegmentCount = _segc[0];

                Lines[y].Segments = new Segment[Lines[y].SegmentCount];

                for (int s = 0; s < Lines[y].SegmentCount; s++)
                {
                    byte[] _offs = new byte[2];
                    byte[] _pixcount = new byte[2];

                    file.Read(_offs, 0, 1);
                    file.Read(_pixcount, 0, 1);

                    Lines[y].Segments[s].Offset = _offs[0];
                    Lines[y].Segments[s].PixCount = _pixcount[0];

                    Lines[y].Segments[s].Pixels = new UInt16[Lines[y].Segments[s].PixCount];

                    for (int i = 0; i < Lines[y].Segments[s].PixCount; i++)
                    {
                        byte[] _pix = new byte[2];

                        file.Read(_pix, 0, 2);

                        Lines[y].Segments[s].Pixels[i] = BitConverter.ToUInt16(_pix, 0);
                    }
                }
            }

            this.LineLengths = new UInt16[Height];
            for (int y = 0; y < Height; y++ )
            {
                byte[] _llen = new byte[2];

                file.Read(_llen, 0, 2);

                UInt16 llen = BitConverter.ToUInt16(_llen, 0);

                this.LineLengths[y] = llen;
            }

            this.ByteCount = this.GetBytes().Length;
        }

        /// <summary>
        /// Initializes a new instance of CAlphaSprite from the given System.Drawing.Image
        /// Warning: Lossy conversions take place.
        /// DarkEden Alpha Sprites have 2 color channels (5 and 6 bits) and 1 alpha channel (5 bits) 
        /// whereas System.Drawing.Image is 32-bit in RGBA
        /// Colors are approximated.
        /// </summary>
        /// <param name="srcimg"></param>
        public CAlphaSprite(Image srcimg)
        {
            this.BodyLength = 0;
            this.Width = (UInt16)srcimg.Width;
            this.Height = (UInt16)srcimg.Height;
            this.LineLengths = new UInt16[this.Height];

            Bitmap srcbmp = new Bitmap(srcimg);

            this.Lines = new Line[srcimg.Height];

            for(int y = 0; y < srcimg.Height; y++)
            {
                this.Lines[y] = new Line();
                UInt16 llength = 1;
                List<Segment> lsegs = new List<Segment>();

                for(int x = 0; x < srcimg.Width;)
                {
                    Segment seg = new Segment();
                    llength += 2;
                    List<UInt16> pixels = new List<UInt16>();

                    byte offs = 0;

                    while(srcbmp.GetPixel(x, y).A == 0)
                    {
                        x++;

                        if (x >= srcimg.Width) break;

                        else offs += 1;
                    }
                    seg.Offset = offs;

                    if (x >= srcimg.Width)
                    {
                        llength -= 2;
                        break;
                    }

                    Color c = srcbmp.GetPixel(x, y);
                    while (c.A > 0)
                    {
                        pixels.Add(CAlphaSpriteHelper.Get16bitFromColor(c));
                        llength += 2;

                        x++;

                        if (x >= srcimg.Width) break;

                        c = srcbmp.GetPixel(x, y);
                    }

                    seg.PixCount = (byte)pixels.Count;

                    seg.Pixels = pixels.ToArray();

                    lsegs.Add(seg);
                }

                this.LineLengths[y] = llength;
                this.BodyLength += llength;
                this.Lines[y].SegmentCount = (byte)lsegs.Count;
                this.Lines[y].Segments = lsegs.ToArray();
            }

            this.ByteCount = this.GetBytes().Length;
        }

        /// <summary>
        /// Gets an array of bytes which is a valid representation of this CAlphaSprite instance.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Stream fs = new MemoryStream();

            byte[] _blen = BitConverter.GetBytes(this.BodyLength);
            byte[] _width = BitConverter.GetBytes(this.Width);
            byte[] _height = BitConverter.GetBytes(this.Height);

            fs.Write(_blen, 0, 4);
            fs.Write(_width, 0, 2);
            fs.Write(_height, 0, 2);

            int lc = 0;
            foreach (Line l in this.Lines)
            {
                byte[] _segc = BitConverter.GetBytes(l.SegmentCount);
                fs.Write(_segc, 0, 1);

                int sc = 0;
                foreach (Segment s in l.Segments)
                {
                    byte[] _segoffs = BitConverter.GetBytes(s.Offset);
                    byte[] _pixcount = BitConverter.GetBytes(s.PixCount);

                    fs.Write(_segoffs, 0, 1);
                    fs.Write(_pixcount, 0, 1);

                    byte[] _seg = new byte[s.PixCount * 2];
                    Buffer.BlockCopy(s.Pixels, 0, _seg, 0, s.PixCount * 2);

                    fs.Write(_seg, 0, _seg.Length);

                    sc++;
                }

                lc++;
            }

            for (int i = 0; i < this.Height; i++ )
            {
                byte[] _llen = BitConverter.GetBytes(this.LineLengths[i]);
                fs.Write(_llen, 0, 2);
            }

            fs.Position = 0;
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            fs.Dispose();

            return buffer;
        }

        /// <summary>
        /// Returns an instance of System.Drawing.Image which represents visually this CAlphaSprite instance.
        /// A lossless conversion takes place.
        /// </summary>
        /// <returns></returns>
        public Image ToImage()
        {
            Bitmap bmp = new Bitmap(1, 1);

            if (this.Height > 0 && this.Width > 0)
            {
                bmp = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
                bmp.MakeTransparent();
                int lc = 0;
                foreach (Line l in this.Lines)
                {
                    int sc = 0;
                    int sco = 0;
                    foreach (Segment s in l.Segments)
                    {
                        sco += (int)s.Offset;

                        for (int i = 0; i < s.PixCount; i++)
                        {
                            int x = sco;
                            int y = lc;

                            Color color = CAlphaSpriteHelper.GetColorFrom16bit(s.Pixels[i], s.Offset);

                            try
                            {
                                /* Try Catch reason:
                                 * Some segments will overflow the boundaries of the sprite..
                                 * This used to crash the old editor, but a try catch solves it.
                                 * I'll investigate the reason later, but there seems to be absolutely no data loss.*/
                                bmp.SetPixel(x, y, color);
                            }
                            catch { }

                            sco++;
                        }

                        sc++;
                    }

                    lc++;
                }
               
            }

            return bmp;
        }
    }
}
