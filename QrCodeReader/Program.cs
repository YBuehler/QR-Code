using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using FreeImageAPI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace QrCodeReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //      CreateQrCode();
            var img = GetImages(@"C:\Users\yb\Desktop\Von GEF 1.pdf");
            var test = ReadQrCode(img);
        }

        private static void CreateQrCode()
        {
            IBarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 80,
                    Height = 80,
                    Margin = 0
                }
            };
            Bitmap aztecBitmap;
            var result = writer.Write("C2001C57-5297-42F1-ADCC-0F98DA10836F;Verfuegung;2016");
            aztecBitmap = new Bitmap(result);
            var bitmap = ResizeImage(aztecBitmap, 70, 70);

            SaveImage(aztecBitmap, @"C:\Temp\test1.jpg");
            SaveImage(bitmap, @"C:\Temp\test2.jpg");
        }

        public static Bitmap CropImage(Image source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            var bmp = new Bitmap(section.Width, section.Height);

            var g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }


        private static void SaveImage(Bitmap bitmap, string name)
        {
            var section = new Rectangle(new Point(bitmap.Width - 100, bitmap.Height - 100), new Size(350, 350));

            var croppedImage = CropImage(bitmap, section);

            using (var stream = new FileStream(name, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var aztecAsBytes = ImageToByte(croppedImage);
                stream.Write(aztecAsBytes, 0, aztecAsBytes.Length);
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            var converter = new ImageConverter();
            return (byte[]) converter.ConvertTo(img, typeof (byte[]));
        }

        
        private static Image GetImages(string filename)
        {

            FileStream fs = File.OpenRead(filename);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, (int)fs.Length);

            List<System.Drawing.Image> ImgList = new List<System.Drawing.Image>();

            iTextSharp.text.pdf.RandomAccessFileOrArray RAFObj = null;
            iTextSharp.text.pdf.PdfReader PDFReaderObj = null;
            iTextSharp.text.pdf.PdfObject PDFObj = null;
            iTextSharp.text.pdf.PdfStream PDFStremObj = null;

            try
            {
                RAFObj = new iTextSharp.text.pdf.RandomAccessFileOrArray(data);
                PDFReaderObj = new iTextSharp.text.pdf.PdfReader(RAFObj, null);

                for (int i = 0; i <= PDFReaderObj.XrefSize - 1; i++)
                {
                    PDFObj = PDFReaderObj.GetPdfObject(i);

                    if ((PDFObj != null) && PDFObj.IsStream())
                    {
                        PDFStremObj = (iTextSharp.text.pdf.PdfStream)PDFObj;
                        iTextSharp.text.pdf.PdfObject subtype = PDFStremObj.Get(iTextSharp.text.pdf.PdfName.SUBTYPE);

                        if ((subtype != null) && subtype.ToString() == iTextSharp.text.pdf.PdfName.IMAGE.ToString())
                        {
                            byte[] bytes = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw((iTextSharp.text.pdf.PRStream)PDFStremObj);

                            if ((bytes != null))
                            {
                                try
                                {
                                    System.IO.MemoryStream MS = new System.IO.MemoryStream(bytes);

                                    MS.Position = 0;

                                    FIBITMAP dib = FreeImage.LoadFromStream(MS, FREE_IMAGE_LOAD_FLAGS.JPEG_FAST);
                                    Bitmap bitmap = FreeImage.GetBitmap(dib);

                                    
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception();
                                }
                            }
                        }
                    }
                }
                PDFReaderObj.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }




            //var pageNum = 1;

            //var pdf = new PdfReader(filename);
            //var pdfDocument = new PdfDocument(pdf);

            //for (var page = 1; page < pdfDocument.GetNumberOfPages(); page++)
            //{
            //    var pg = pdfDocument.GetPage(page);
            //    var pgStreamCount = pg.GetContentStreamCount();
            //    var pgStream = pg.GetContentStream(0);
                
            //    var pdfXObject = new PdfImageXObject(pgStream);
            //    var filter = pdfXObject.GetPdfObject().Get(PdfName.Filter);
            //    PdfArray filters = new PdfArray();
            //    if (filter != null)
            //    {
            //        if (filter.GetObjectType() == PdfObject.NAME)
            //        {
            //            filters.Add(filter);
            //        }
            //        else
            //        {
            //            if (filter.GetObjectType() == PdfObject.ARRAY)
            //            {
            //                filters = ((PdfArray)filter);
            //            }
            //        }
            //    }
            //    for (int i = filters.Size() - 1; i >= 0; i--)
            //    {
            //        PdfName filterName = (PdfName)filters.Get(i);
            //        if (PdfName.DCTDecode.Equals(filterName))
            //        {
            //            //return ImageType.JPEG;
            //        }
            //        else
            //        {
            //            if (PdfName.JBIG2Decode.Equals(filterName))
            //            {
            //                //return ImageType.JBIG2;
            //            }
            //            else
            //            {
            //                if (PdfName.JPXDecode.Equals(filterName))
            //                {
            //                    //return ImageType.JPEG2000;
            //                }
            //            }
            //        }
            //    }

            //    // None of the previous types match
            //    PdfObject colorspace = pdfXObject.GetPdfObject().Get(PdfName.ColorSpace);
            //}


            //var res = (PdfDictionary) PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));
            //var xobj = (PdfDictionary) PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
            //if (xobj == null)
            //{
            //    return null;
            //}
            //foreach (PdfName name in xobj.Keys)
            //{
            //    PdfObject obj = xobj.Get(name);
            //    if (!obj.IsIndirect())
            //    {
            //        continue;
            //    }
            //    var tg = (PdfDictionary) PdfReader.GetPdfObject(obj);
            //    var type = (PdfName) PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));
            //    if (!type.Equals(PdfName.IMAGE))
            //    {
            //        continue;
            //    }
            //    short xrefIndex =
            //        Convert.ToInt16(((PRIndirectReference) obj).Number.ToString(CultureInfo.InvariantCulture));
            //    PdfObject pdfObj = pdf.GetPdfObject(xrefIndex);
            //    var pdfStrem = (PdfStream) pdfObj;

            //    var pdfImage =
            //        new PdfImageObject((PRStream) pdfStrem);

            //    pdfImage.

            //    var img = pdfImage.GetDrawingImage();


            //    string path = @"C:\Temp\test3.jpg";
            //    var parms = new EncoderParameters(1);
            //    parms.Param[0] = new EncoderParameter(Encoder.Compression, 0);
            //    ImageCodecInfo jpegEncoder =
            //        ImageCodecInfo.GetImageEncoders().ToList().Find(x => x.FormatID == ImageFormat.Jpeg.Guid);
            //    img.Save(path, jpegEncoder, parms);

            //    return img;
            return null;
        }

        private static Result ReadQrCode(Image img)
        {
            var uri = new Uri(@"C:\Temp\test1.jpg");
            Bitmap image;
            try
            {
                image = (Bitmap) img;
            }
            catch (Exception)
            {
                throw new FileNotFoundException("Resource not found: " + uri);
            }

            using (image)
            {
                LuminanceSource source;
                source = new BitmapLuminanceSource(image);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                var result = new MultiFormatReader().decode(bitmap);
                if (result != null)
                {
                    var test = result;
                }
                else
                {
                    var test = result;
                }
                return result;
            }
        }

        public static Bitmap ResizeImage(Image image, float width, float height)
        {
            var brush = new SolidBrush(Color.Black);

            var scale = Math.Min(width/image.Width, height/image.Height);

            var bmp = new Bitmap((int) width, (int) height);
            var graph = Graphics.FromImage(bmp);

            // uncomment for higher quality output
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;

            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            var scaleWidth = (int) (image.Width*scale);
            var scaleHeight = (int) (image.Height*scale);

            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            graph.DrawImage(image,
                new Rectangle(((int) width - scaleWidth)/2, ((int) height - scaleHeight)/2, scaleWidth, scaleHeight));

            return bmp;
        }
    }
}