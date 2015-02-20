using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using System.Collections;
using System;
using System.IO;
using TikaOnDotNet;
using System.Collections.Generic;
using Contrib.Indexing.Documents.Helper;
namespace Contrib.Indexing.Documents.Handlers
{
    /// <summary>
    /// Library used to extract text from different documents formats
    /// Adobe PDF - .pdf
    /// Microsoft Word - .doc and .docx
    /// Microsoft Excel - .xls and .xlsx
    /// Microsoft PowerPoint - .ppt and .pptx
    /// Rich Text Format - .rtf
    /// Link : https://kevm.github.io/tikaondotnet/
    /// 
    /// </summary>
    public class DocumentsIndexingHandler : ContentHandler
    {
        private readonly IStorageProvider _storageProvider;
        public DocumentsIndexingHandler(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            Logger = NullLogger.Instance;

            OnIndexing<DocumentPart>((context, part) =>
            {
                string textToIndex = GetTextToIndex(part);
                if (!string.IsNullOrEmpty(textToIndex))
                {
                    context.DocumentIndex.Add("orchard-allowed-documents", textToIndex).Analyze().Store();
                }
            });
        }

       
        private string GetTextToIndex(DocumentPart part)
        {
            var mediaPart = ((ContentItem)part.ContentItem).As<MediaPart>();
            var textToIndex = String.Empty;
            if (mediaPart != null)
            {
                var extension = Path.GetExtension(mediaPart.FileName);

                if (AllowedExtension(extension))
                {
                    var document = _storageProvider.GetFile(Path.Combine(mediaPart.FolderPath, mediaPart.FileName));

                    var documentStream = document.OpenRead();
                    if (documentStream == null) { return textToIndex; }
                    byte[] data = StreamHelper.ReadToEnd(documentStream);
                    if (data == null) { return textToIndex; }

                    // Text Extraction
                    var extractor = new TextExtractor();
                    var contentResult = extractor.Extract(data);
                    textToIndex = contentResult.Text;
                }
            }
            return textToIndex;
        }


       
        private bool AllowedExtension(string extension)
        {
            var extensions = new List<string>() 
            {
                ".pdf",
                ".doc", ".docx", 
                ".xls", ".xlsx",
                ".ppt", ".pptx",
                ".rtf"
            };
            return extensions.Contains(extension);
        }
    }
}