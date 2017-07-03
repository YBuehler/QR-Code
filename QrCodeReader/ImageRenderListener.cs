using System.Collections.Generic;
using System.Drawing;
using System.IO;
using FreeImageAPI;
using iTextSharp.text.pdf.parser;

namespace QrCodeReader
{
    public class ImageRenderListener : IRenderListener
    {
        public readonly List<string> ImageNames = new List<string>();
        public readonly List<Bitmap> Images = new List<Bitmap>();

        public void BeginTextBlock()
        {
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
        }

        public void EndTextBlock()
        {
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            var image = renderInfo.GetImage();
            if (image == null)
            {
                return;
            }
            var imageNumber = renderInfo.GetRef().Number;
            Bitmap bitmap;
            if (image.GetImageBytesType().FileExtension == "jp2")
            {
                // decode using FreeImage
                using (var msJp2 = new MemoryStream(image.GetImageAsBytes()))
                {
                    FIBITMAP dib = FreeImage.LoadFromStream(msJp2, FREE_IMAGE_LOAD_FLAGS.JPEG_FAST);
                    //FreeImage.Save(FREE_IMAGE_FORMAT.FIF_JPEG, dib, outputPath, FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYNORMAL);
                    using (var msJpg = new MemoryStream())
                    {
                        FreeImage.SaveToStream(dib, msJpg, FREE_IMAGE_FORMAT.FIF_JPEG, FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYSUPERB);
                        bitmap = new Bitmap(msJpg);
                    }
                }
            }
            else
            {
                bitmap = ByteArrayToImage(image.GetImageAsBytes());
            }
            ImageNames.Add($"Image{ imageNumber}.jpg");
            Images.Add(bitmap);
        }

        private Bitmap ByteArrayToImage(byte[] byteArrayIn)
        {
            var converter = new ImageConverter();
            var img = (Bitmap) converter.ConvertFrom(byteArrayIn);
            return img;
        }
    }
}