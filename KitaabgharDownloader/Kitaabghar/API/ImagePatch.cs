using System;
using System.Drawing;

namespace Kitaabghar
{
    public static class ImagePatch
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static Image PatchImages(Image uImage, Image lImage, Image rImage)
        {
            var width = uImage.Width;
            var height = uImage.Height + lImage.Height;
            using (var bitmap = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);
                    //graphics.DrawImage(uImage, 0, 0);
                    //graphics.DrawImage(lImage, 0, uImage.Height);
                    //graphics.DrawImage(rImage, lImage.Width, uImage.Height);
                    graphics.DrawImage(uImage,new Rectangle(0,0,uImage.Width,uImage.Height));
                    graphics.DrawImage(lImage, new Rectangle(0, uImage.Height, lImage.Width, lImage.Height));
                    graphics.DrawImage(rImage, new Rectangle(lImage.Width, uImage.Height, rImage.Width, rImage.Height));
                }
                //var stream = new MemoryStream();
                //bitmap.Save(stream, ImageFormat.Gif);
                var pointer=bitmap.GetHbitmap();
                var image = Image.FromHbitmap(pointer);
                DeleteObject(pointer);

                return image;
            }

        }
    }
}
