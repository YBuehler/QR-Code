using System.Drawing;
using System.IO;
using FreeImageAPI;

namespace QrCodeReader
{
    public class FoundImage
    {
        public FoundImage(int number, string fileExtension)
        {
            Number = number;
            FileExtension = fileExtension;
        }

        public int Number { get; }
        public string FileExtension { get; private set; }
        public byte[] ImageBytes { get; private set; }
        public string Name => $"Image{Number}.{FileExtension}";

        public Bitmap GetBitmap()
        {
            var converter = new ImageConverter();
            var img = (Bitmap)converter.ConvertFrom(ImageBytes);
            return img;
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public void SetBytes(byte[] imageBytes)
        {
            if (FileExtension == "jp2")
            {
                // decode using FreeImage
                using (var msJp2 = new MemoryStream(imageBytes))
                {
                    FIBITMAP dib = FreeImage.LoadFromStream(msJp2, FREE_IMAGE_LOAD_FLAGS.JPEG_FAST);
                    //FreeImage.Save(FREE_IMAGE_FORMAT.FIF_JPEG, dib, outputPath, FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYNORMAL);
                    using (var msJpg = new MemoryStream())
                    {
                        FreeImage.SaveToStream(dib, msJpg, FREE_IMAGE_FORMAT.FIF_JPEG,
                            FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYSUPERB);
                        msJpg.Seek(0, SeekOrigin.Begin);
                        ImageBytes = ReadFully(msJpg);
                    }
                }
            }
            else
            {
                ImageBytes = imageBytes;
            }

            // now for both streams, jpg is ok
            FileExtension = "jpg";
        }
    }
}