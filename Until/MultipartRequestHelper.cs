using Microsoft.Net.Http.Headers;
using System.IO;

namespace catdriveupload.Until
{
    public class MultipartRequestHelper
    {
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary");
            }
            if(boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Mutipart boundary length limit {lengthLimit} exceeded");
            }
            return boundary;
        }

        public static bool IsMutipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType);
        }
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }
        public static string GetFileContentInputName(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition.Name.Value;
        }
        public static string GetFileName(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition.FileName.Value;
        }
    }
}
