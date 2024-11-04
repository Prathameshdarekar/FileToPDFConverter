using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Aspose.Words;
using Aspose.Cells;
using Aspose.Pdf;
using PdfDocument = Aspose.Pdf.Document;
using WordDocument = Aspose.Words.Document;


namespace FileToPdfConverterApp.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, string outputFileName)
        {
            if (file != null && file.ContentLength > 0)
            {
                int fileSizeLimit = 10 * 1024 * 1024; // 10MB
                if (file.ContentLength > fileSizeLimit)
                {
                    ViewBag.Message = "File size exceeds the 10MB limit.";
                    return View("Index");
                }

                string extension = Path.GetExtension(file.FileName).ToLower();
                byte[] pdfBytes = null;

                try
                {
                    switch (extension)
                    {
                        case ".txt":
                            pdfBytes = ConvertTextToPdf(file);
                            break;
                        case ".doc":
                        case ".docx":
                            pdfBytes = ConvertWordToPdf(file);
                            break;
                        case ".xls":
                        case ".xlsx":
                            pdfBytes = ConvertExcelToPdf(file);
                            break;
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                            pdfBytes = ConvertImageToPdf(file);
                            break;
                        case ".ppt":
                        case ".pptx":
                            pdfBytes = ConvertPowerPointToPdf(file);
                            break;
                        case ".html":
                            pdfBytes = ConvertHtmlToPdf(file);
                            break;
                        default:
                            ViewBag.Message = "Unsupported file format.";
                            return View("Index");
                    }

                    // Ensure the file name is safe and add .pdf extension if missing
                    outputFileName = string.IsNullOrWhiteSpace(outputFileName) ? "ConvertedFile" : outputFileName.Trim();
                    if (!outputFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        outputFileName += ".pdf";
                    }

                    // Create the GeneratedFiles directory if it doesn't exist
                    string directoryPath = Server.MapPath("~/GeneratedFiles");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Save the generated PDF file to the directory
                    string generatedFilePath = Path.Combine(directoryPath, outputFileName);
                    System.IO.File.WriteAllBytes(generatedFilePath, pdfBytes);

                    ViewBag.PdfFileName = outputFileName; // Store the PDF file name for the view
                    ViewBag.Message = "File converted successfully!"; // Success message
                    return View("Index"); // Return to the same view
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error during conversion: " + ex.Message;
                    return View("Index");
                }
            }
            else
            {
                ViewBag.Message = "Please select a file to upload.";
                return View("Index");
            }
        }




        private byte[] ConvertPowerPointToPdf(HttpPostedFileBase file)
        {
            using (var presentation = new Aspose.Slides.Presentation(file.InputStream))
            using (var stream = new MemoryStream())
            {
                presentation.Save(stream, Aspose.Slides.Export.SaveFormat.Pdf);
                return stream.ToArray();
            }
        }

        private byte[] ConvertHtmlToPdf(HttpPostedFileBase file)
        {
            using (var htmlStream = file.InputStream)
            using (var outputStream = new MemoryStream())
            {
                // Load the HTML document
                var htmlDocument = new Aspose.Html.HTMLDocument(htmlStream, "UTF-8");

                // Convert to PDF
                Aspose.Html.Rendering.Pdf.PdfRenderingOptions pdfOptions = new Aspose.Html.Rendering.Pdf.PdfRenderingOptions();
                Aspose.Html.Rendering.Pdf.PdfDevice pdfDevice = new Aspose.Html.Rendering.Pdf.PdfDevice(outputStream);
                Aspose.Html.Rendering.HtmlRenderer renderer = new Aspose.Html.Rendering.HtmlRenderer();
                renderer.Render(pdfDevice, htmlDocument);

                return outputStream.ToArray();
            }
        }

        private byte[] ConvertTextToPdf(HttpPostedFileBase file)
        {
            using (var reader = new StreamReader(file.InputStream))
            {
                string textContent = reader.ReadToEnd();
                using (var memoryStream = new MemoryStream())
                {
                    PdfDocument pdfDocument = new PdfDocument();
                    Page page = pdfDocument.Pages.Add();
                    page.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(textContent));
                    pdfDocument.Save(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        private byte[] ConvertWordToPdf(HttpPostedFileBase file)
        {
            using (var memoryStream = new MemoryStream())
            {
                WordDocument doc = new WordDocument(file.InputStream);
                doc.Save(memoryStream, Aspose.Words.SaveFormat.Pdf);
                return memoryStream.ToArray();
            }
        }

        private byte[] ConvertExcelToPdf(HttpPostedFileBase file)
        {
            using (var memoryStream = new MemoryStream())
            {
                Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook(file.InputStream);
                workbook.Save(memoryStream, Aspose.Cells.SaveFormat.Pdf);
                return memoryStream.ToArray();
            }
        }

        private byte[] ConvertImageToPdf(HttpPostedFileBase file)
        {
            using (var memoryStream = new MemoryStream())
            {
                PdfDocument pdfDocument = new PdfDocument();
                Page page = pdfDocument.Pages.Add();

                Aspose.Pdf.Image image = new Aspose.Pdf.Image();
                image.ImageStream = file.InputStream;

                page.Paragraphs.Add(image);
                pdfDocument.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public ActionResult Download(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string filePath = Path.Combine(Server.MapPath("~/GeneratedFiles"), fileName);

                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, "application/pdf", fileName);
                }
                else
                {
                    ViewBag.Message = "File not found.";
                    return View("Index"); // Redirect to the Index view
                }
            }
            ViewBag.Message = "No file selected.";
            return View("Index"); // Redirect to the Index view
        }

    }
}
