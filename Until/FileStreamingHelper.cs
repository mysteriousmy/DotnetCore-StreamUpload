using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace catdriveupload.Until
{
    public static class FileStreamingHelper
    {
        public static readonly FormOptions _defaultFormOptions = new FormOptions();
        public static List<string> filename = new List<string>();

        public static async Task<FormValueProvider> StreamFiles(this HttpRequest request, string targetDirectory)
        {
            if (!MultipartRequestHelper.IsMutipartContentType(request.ContentType))
            {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
            }
            var formAccimulator = new KeyValueAccumulator();
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();
            while(section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);
                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        if (!Directory.Exists(targetDirectory)){
                            Directory.CreateDirectory(targetDirectory);
                        }
                        var fileName = MultipartRequestHelper.GetFileName(contentDisposition);
                        filename.Add(fileName);
                        using(var targetFileStream = File.Create(targetDirectory + fileName))
                        {
                            await section.Body.CopyToAsync(targetFileStream);
                        }
                    }
                    else if(MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body, 
                            encoding,
                            detectEncodingFromByteOrderMarks:true,
                            leaveOpen: true))
                        {
                            var value = await streamReader.ReadToEndAsync();
                            if(String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = String.Empty;
                            }
                            formAccimulator.Append(key.Value, value);
                            if(formAccimulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }
            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccimulator.GetResults()),
                CultureInfo.CurrentCulture);
            return formValueProvider;
        }
        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            if(!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }
}
