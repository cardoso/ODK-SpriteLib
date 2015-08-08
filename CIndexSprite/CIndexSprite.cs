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
    /// Represents a single DarkEden Index Sprite
    /// </summary>
    public class CIndexSprite
    {
        //For internal use only. DarkEden does not use these.
        public int ByteCount;
        public string Name;
        public int CKPixCount;

        //The following fields are, in order, an accurate representation of an index sprite (ISP).
        
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
            public UInt16 Length;
            /// <summary>
            /// The number of segments in this line.
            /// </summary>
            public UInt16 SegmentCount;
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
            public UInt16 Offset;
            /// <summary>
            /// The amount of ChromaKey pixels this segment contains
            /// </summary>
            public UInt16 CKPixelCount;
            /// <summary>
            /// The ChromaKey pixels of this segment
            /// </summary>
            public UInt16[] CKPixels;
            /// <summary>
            /// The amount of pixels this segment contains.
            /// </summary>
            public UInt16 PixCount;
            /// <summary>
            /// The pixels of this segment represented by 16-bit integers.
            /// </summary>
            public UInt16[] Pixels;
        }
        /// <summary>
        /// Initializes a new instance of CIndexSprite from a file stream
        /// </summary>
        /// <param name="file">The filestream as it would be read by DarkEden.</param>
        public CIndexSprite(ref FileStream file, string name = null)
        {
            if (name != null) this.Name = name;

            byte[] _width = new byte[2];
            byte[] _height = new byte[2];

            file.Read(_width, 0, 2);
            file.Read(_height, 0, 2);

            this.Width = BitConverter.ToUInt16(_width, 0);
            this.Height = BitConverter.ToUInt16(_height, 0);

            this.Lines = new Line[this.Height];

            int pixelcount = Width * Height;

            int offs = 4;
            for (int y = 0; y < Height; y++)
            {
                byte[] _llen = new byte[2];
                byte[] _segc = new byte[2];

                file.Read(_llen, 0, 2);
                file.Read(_segc, 0, 2);

                Lines[y].Length = BitConverter.ToUInt16(_llen, 0);
                Lines[y].SegmentCount = BitConverter.ToUInt16(_segc, 0);

                Lines[y].Segments = new Segment[Lines[y].SegmentCount];

                offs += 4;

                for (int s = 0; s < Lines[y].SegmentCount; s++)
                {
                    byte[] _segoffs = new byte[2];
                    byte[] _ckpixcount = new byte[2];

                    file.Read(_segoffs, 0, 2);
                    file.Read(_ckpixcount, 0, 2);

                    Lines[y].Segments[s].Offset = BitConverter.ToUInt16(_segoffs, 0);
                    Lines[y].Segments[s].CKPixelCount = BitConverter.ToUInt16(_ckpixcount, 0);

                    Lines[y].Segments[s].CKPixels = new UInt16[Lines[y].Segments[s].CKPixelCount];

                    offs += 4;
                    for(int i = 0; i < Lines[y].Segments[s].CKPixelCount; i++)
                    {
                        byte[] _ckpix = new byte[2];

                        file.Read(_ckpix, 0, 2);

                        Lines[y].Segments[s].CKPixels[i] = BitConverter.ToUInt16(_ckpix, 0);

                        offs += 2;

                        //internal use
                        this.CKPixCount++;
                    }

                    byte[] _pixcount = new byte[2];

                    file.Read(_pixcount, 0, 2);

                    Lines[y].Segments[s].PixCount = BitConverter.ToUInt16(_pixcount, 0);

                    Lines[y].Segments[s].Pixels = new UInt16[Lines[y].Segments[s].PixCount];

                    offs += 2;
                    for (int i = 0; i < Lines[y].Segments[s].PixCount; i++)
                    {
                        byte[] _pix = new byte[2];

                        file.Read(_pix, 0, 2);

                        Lines[y].Segments[s].Pixels[i] = BitConverter.ToUInt16(_pix, 0);

                        offs += 2;
                    }
                }
            }

            this.ByteCount = this.GetBytes().Length;
        }

        /// <summary>
        /// Initializes a new instance of CIndexSprite from the given System.Drawing.Image
        /// Warning: Lossy conversions take place.
        /// DarkEden Sprites are 16-bit in color and have no Alpha whereas System.Drawing.Image is 32-bit in color and have 0 to 255 Alpha.
        /// Colors are approximated. Pixels with zero alpha are ignored and pixels with more than zero alpha are considered opaque.
        /// </summary>
        /// <param name="srcimg"></param>
        public CIndexSprite(Image srcimg)
        {
            this.Width = (UInt16)srcimg.Width;
            this.Height = (UInt16)srcimg.Height;

            Bitmap srcbmp = new Bitmap(srcimg);

            this.Lines = new Line[srcimg.Height];

            for(int y = 0; y < srcimg.Height; y++)
            {
                this.Lines[y] = new Line();
                UInt16 llength = 2;
                List<Segment> lsegs = new List<Segment>();

                for(int x = 0; x < srcimg.Width;)
                {
                    llength += 3;

                    Segment seg = new Segment();

                    //Temporary ISPK workaround.
                    //seg.CKPixelCount = 0;//
                    //seg.CKPixels = null;//

                    List<UInt16> ckpixels = new List<UInt16>();//
                    List<UInt16> pixels = new List<UInt16>();

                    ushort offs = 0;

                    while(srcbmp.GetPixel(x, y).A == 0)
                    {
                        x++;

                        if (x >= srcimg.Width) break;

                        else offs += 1;
                    }
                    seg.Offset = offs;

                    if (x >= srcimg.Width)
                    {
                        llength -= 4;
                        break;
                    }

                    Color c = srcbmp.GetPixel(x, y);

                    while (c.A > 0 && CIndexSpriteHelper.IsChromaKeyColor(c))
                    {
                        ckpixels.Add(CIndexSpriteHelper.Get16bitFromColor(c));
                        this.CKPixCount++;

                        x++;

                        if (x >= srcimg.Width) break;

                        llength++;
                        c = srcbmp.GetPixel(x, y);
                    }

                    while (c.A > 0 && !CIndexSpriteHelper.IsChromaKeyColor(c))
                    {
                        pixels.Add(CIndexSpriteHelper.Get16bitFromColor(c));

                        x++;

                        if (x >= srcimg.Width) break;

                        llength++;
                        c = srcbmp.GetPixel(x, y);
                    }

                    seg.CKPixelCount = (UInt16)ckpixels.Count;
                    seg.CKPixels = ckpixels.ToArray();
                    seg.PixCount = (UInt16)pixels.Count;
                    seg.Pixels = pixels.ToArray();

                    lsegs.Add(seg);
                }

                this.Lines[y].Length = llength;
                this.Lines[y].SegmentCount = (UInt16)lsegs.Count;
                this.Lines[y].Segments = lsegs.ToArray();
            }

            this.ByteCount = this.GetBytes().Length;
        }

        /// <summary>
        /// Gets an array of bytes which is a valid representation of this CIndexSprite instance.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            Stream fs = new MemoryStream();

            byte[] _width = BitConverter.GetBytes(this.Width);
            byte[] _height = BitConverter.GetBytes(this.Height);

            fs.Write(_width, 0, 2);
            fs.Write(_height, 0, 2);

            int lc = 0;
            foreach (Line l in this.Lines)
            {
                byte[] _llength = BitConverter.GetBytes(l.Length);
                byte[] _segc = BitConverter.GetBytes(l.SegmentCount);
                fs.Write(_llength, 0, 2);
                fs.Write(_segc, 0, 2);

                int sc = 0;
                foreach (Segment s in l.Segments)
                {
                    byte[] _segoffs = BitConverter.GetBytes(s.Offset);
                    byte[] _ckpixcount = BitConverter.GetBytes(s.CKPixelCount);

                    fs.Write(_segoffs, 0, 2);
                    fs.Write(_ckpixcount, 0, 2);

                    if (s.CKPixelCount > 0)
                    {
                        byte[] _ckpixels = new byte[s.CKPixelCount * 2];
                        Buffer.BlockCopy(s.CKPixels, 0, _ckpixels, 0, s.CKPixelCount * 2);

                        fs.Write(_ckpixels, 0, _ckpixels.Length);
                    }

                    byte[] _pixcount = BitConverter.GetBytes(s.PixCount);

                    fs.Write(_pixcount, 0, 2);

                    byte[] _pixels = new byte[s.PixCount * 2];
                    Buffer.BlockCopy(s.Pixels, 0, _pixels, 0, s.PixCount * 2);

                    fs.Write(_pixels, 0, _pixels.Length);

                    sc++;
                }

                lc++;
            }

            fs.Position = 0;
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            fs.Dispose();

            return buffer;
        }

        /// <summary>
        /// Returns an instance of System.Drawing.Image which represents visually this CIndexSprite instance.
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

                        for (int i = 0; i < s.CKPixelCount; i++)
                        {
                            int x = sco;
                            int y = lc;

                            Color color = CIndexSpriteHelper.GetColorFrom16bit(s.CKPixels[i]);

                            try
                            {
                                bmp.SetPixel(x, y, color);
                            }
                            catch { }

                            sco++;
                        }

                        for (int i = 0; i < s.PixCount; i++)
                        {
                            int x = sco;
                            int y = lc;

                            Color color = CIndexSpriteHelper.GetColorFrom16bit(s.Pixels[i]);

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

        /// <summary>
        /// Applies the color key attribute to matching pixels.
        /// </summary>
        /// <returns></returns>
        public void ApplyColorKey()
        {
            foreach(Line l in Lines)
            {
                foreach(Segment s in l.Segments)
                {
                    foreach(ushort i in s.Pixels)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Removes the color key attribute from eligible segments.
        /// </summary>
        /// <returns></returns>
        public void RemoveColorKey()
        {

        }
    }
}
