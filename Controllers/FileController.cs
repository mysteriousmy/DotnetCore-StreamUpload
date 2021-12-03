using catdriveupload.Filter;
using catdriveupload.Until;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace catdriveupload.Controllers
{
    public class FileController : ControllerBase
    {
        private static string path = "";
        private static bool isUpload = false;
        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        [DisableFormValueModelBinding]
        public async Task<ContentResult> FileUpload()
        {
            var result = "<script>alert('不允许未经验证的上传！');self.location=document.referrer;</script>";
            var Content = new ContentResult();
            Content.ContentType = "text/html;charset=utf-8";
            if (isUpload)
            {
                var filename = $"/mnt/disk1/${path}/";
                //string filename = @"G:\UploadingFiles\";
                try
                {
                    FormValueProvider formModel;
                    formModel = await Request.StreamFiles(filename);
                    result = "<script>alert('上传成功！');self.location=document.referrer;</script>";
                    Content.Content = string.Format(result);
                    return Content;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //foreach (string fn in FileStreamingHelper.filename)
                    //{
                    //    System.IO.File.Delete($"{filename}\\{fn}");
                    //}
                    result = "<script>alert('出错,上传失败');self.location=document.referrer;</script>";
                    Content.Content = string.Format(result);
                    return Content;
                }
            }
            Content.Content = string.Format(result);
            return Content;
        }
        [HttpPost("check")]
        public IActionResult checkUser(Auth auth)
        {
            Console.WriteLine(auth.username);
            if ("xxxx".Equals(auth.username) && "xxxxxx".Equals(auth.password))
            {
                path = auth.path;
                isUpload = true;
                return Ok(new ResponseCode { code = "200", msg="验证成功！"});
            }
            else
            {
                return BadRequest(new ResponseCode { code = "400", msg = "用户名或密码错误！" });
            }
        }
    }
   
}
