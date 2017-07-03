using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using FreeImageAPI;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using Path = System.IO.Path;

namespace QrCodeReader
{
    internal class Program
    {
        private static string _path;

        private static void Main(string[] args)
        {
            //      CreateQrCode();
            _path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");
            //var fullPath1 = Path.Combine(_path, "Von GEF 1.pdf");
            //var fullPath1 = Path.Combine(_path, "Von Itree 1.pdf");
            //var code = ReadQRCodeFromPdf(fullPath1);
            var imgPath = Path.Combine(_path, @"extracted\Image14.jpg");
            Bitmap bitmap;
            using (var fs = File.OpenRead(imgPath))
            {
                bitmap = new Bitmap(fs);
            }
            var code = ReadQrCode(bitmap);
            Console.WriteLine();
            Console.WriteLine(code);
            Console.WriteLine();
            //var fullPath2 = Path.Combine(_path, "Von GEF 1.pdf");
            //var img2 = GetImages(fullPath2);
            //var fileName = "affolter.NET.Test.jpg";
            //CreateQrCode("C2001C57-5297-42F1-ADCC-0F98DA10836F;Verfuegung;2016", fileName);
            //var result = ReadQrCode(fileName);
            Console.ReadKey();
        }

        private static void CreateQrCode(string contents, string fileName)
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
            var result = writer.Write(contents);
            SaveImage(result, Path.Combine(_path, fileName));
            //aztecBitmap = new Bitmap(result);
            //var bitmap = ResizeImage(aztecBitmap, 70, 70);

            //SaveImage(aztecBitmap, Path.Combine(_path, "test1.jpg"));
            //SaveImage(bitmap, Path.Combine(_path, "test2.jpg"));
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

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            var converter = new ImageConverter();
            var img = (Image)converter.ConvertFrom(byteArrayIn);
            return img;
        }

        private static string ReadQRCodeFromPdf(string filename)
        {
            FileStream fs = File.OpenRead(filename);
            PdfReader reader = new PdfReader(fs);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            var listener = new BetterImageRenderListener();
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                parser.ProcessContent(i, listener);
            }

            // delete all existing
            var filePath = Path.Combine(_path, "extracted");
            var dir = Directory.CreateDirectory(filePath);
            foreach (var fi in dir.GetFiles()){
                fi.Delete();
            }

            for (int i = 0; i < listener.Count; ++i)
            {
                var img = listener[i];
                
                var path = Path.Combine(filePath, img.Name);
                Bitmap btmap;
                using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    btmap = img.GetBitmap();
                    btmap.Save(stream, ImageFormat.Jpeg);
                }
                Console.WriteLine(img.Name);
                var result = ReadQrCode(btmap);
                if (!string.IsNullOrWhiteSpace(result?.Text))
                {
                    return result.Text;
                }
            }
            return string.Empty;
        }

        private static Image GetImages2(string filename)
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
                        PdfObject filterType = PDFStremObj.Get(PdfName.FILTER);
                        if (filterType.Equals(PdfName.JBIG2DECODE))
                        {
                            //... it will not work
                        }
                        iTextSharp.text.pdf.PdfObject subtype = PDFStremObj.Get(iTextSharp.text.pdf.PdfName.SUBTYPE);

                        if ((subtype != null) && subtype.ToString() == iTextSharp.text.pdf.PdfName.IMAGE.ToString())
                        {
                            byte[] bytes = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw((iTextSharp.text.pdf.PRStream)PDFStremObj);

                            if ((bytes != null))
                            {
                                try
                                {
                                    var img = ByteArrayToImage(bytes);
                                    ImgList.Add(img);
                                    //System.IO.MemoryStream MS = new System.IO.MemoryStream(bytes);

                                    //MS.Position = 0;

                                    FIBITMAP dib = FreeImage.LoadEx("test.jp2");
                                    ////save the image out to disk    
                                    //FreeImage.Save(FREE_IMAGE_FORMAT.FIF_JPEG, dib, "test.jpg", FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYNORMAL);
                                    //or even turn it into a normal Bitmap for later use
                                    Bitmap bitmap = FreeImage.GetBitmap(dib);

                                    ////FIBITMAP dib = FreeImage.LoadFromStream(MS, FREE_IMAGE_LOAD_FLAGS.JPEG_FAST);
                                    ////Bitmap bitmap = FreeImage.GetBitmap(dib);

                                    //System.Drawing.Image ImgPDF = System.Drawing.Image.FromStream(MS);

                                    //ImgList.Add(ImgPDF);
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

        private static Bitmap LoadImage(string fileName)
        {
            var path = Path.Combine(_path, fileName);
            Bitmap image;
            try
            {
                image = new Bitmap(path);
            }
            catch (Exception)
            {
                throw new FileNotFoundException("Resource not found: " + path);
            }
            return image;
        }

        private static Result ReadQrCode(Bitmap image)
        {
            using (image)
            {
                LuminanceSource source = new BitmapLuminanceSource(image);
                var binarizer = new HybridBinarizer(source);
                var bitmap = new BinaryBitmap(binarizer);
                var reader = new QRCodeReader();
                var result = reader.decode(bitmap);
                if (result == null)
                {
                    result = new MultiFormatReader().decode(bitmap);
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