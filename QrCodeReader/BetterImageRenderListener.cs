using System.Collections.Generic;
using iTextSharp.text.pdf.parser;

namespace QrCodeReader
{
    public class BetterImageRenderListener: List<FoundImage>, IRenderListener
    {
        public void BeginTextBlock()
        {
            // do nothing
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
            // do nothing
        }

        public void EndTextBlock()
        {
            // do nothing
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            var image = renderInfo.GetImage();
            if (image == null)
            {
                return;
            }
            var foundImage = new FoundImage(renderInfo.GetRef().Number, image.GetImageBytesType().FileExtension);
            foundImage.SetBytes(image.GetImageAsBytes());
            Add(foundImage);
        }
    }
}
