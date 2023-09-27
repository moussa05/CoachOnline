using ClosedXML.Excel;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Model;
using CoachOnline.Statics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace CoachOnline.Helpers
{
    public enum FileType { Attachment, Image, Video, StudentCard, Document }
    public static class Extensions
    {
        public static readonly char[] PasswordMandatory = { '$', '!', '@', '%', '^', '&', '*', '(', ')' };

        public static bool IsPasswordSecure(string password)
        {
            if (!password.Any(PasswordMandatory.Contains)
                    || !password.Any(char.IsLower)
                    || !password.Any(char.IsDigit)
                    || !password.Any(char.IsUpper)
                    || password.Length < 5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static User WithoutPassword(this User user)
        {
            user.Password = null;
            return user;
        }

        public static bool IsEmailCorrect(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                throw new CoachOnlineException("Provided email address has an incorrect format.", CoachOnlineExceptionState.IncorrectFormat);
            }
        }

        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            int id = 0;
            if (int.TryParse(claim?.Value?.ToString(), out id))
            {
                return id;
            }
            return null;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var claim = principal.FindFirst(ClaimTypes.Role);


            return claim.Value;
        }

        public static string WriteToExcel<T>(List<T> data, string worksheetName = "data")
        {
            if (data == null || !data.Any())
            {
                return null;
            }

            using (var mem = new MemoryStream())
            using (var workbook = new XLWorkbook())
            {


                var wsData = workbook.AddWorksheet(worksheetName);

                var props = data.First().GetType().GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    wsData.Cell(1, i + 1).Value = props[i].Name;
                }

                wsData.Cell(2, 1).InsertData(data);
                var randomFileName = LetsHash.RandomString(10)+$"_{DateTime.Now.ToString("ddMMyyHHmmss")}"+".xlsx";
                var path = $"{ConfigData.Config.EnviromentPath}wwwroot/tempfiles/{randomFileName}";

                workbook.SaveAs(path);

                return path;
            }


        }

        public static byte[] WriteToExcelReturnByteArray<T>(List<T> data, string worksheetName="data")
        {
            if(data==null || !data.Any())
            {
                return null;
            }

            using (var mem = new MemoryStream())
            using (var workbook = new XLWorkbook())
            {
               

                var wsData = workbook.AddWorksheet(worksheetName);

                var props = data.First().GetType().GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    wsData.Cell(1, i+1).Value = props[i].Name;
                }

                wsData.Cell(2, 1).InsertData(data);
                workbook.SaveAs(mem);

                var result = mem.ToArray();

                return result;
            }

        
        }

        // public static void SaveDocument(string base64, string name)
        // {
        //     using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
        //     {
        //         using (Bitmap bm2 = new Bitmap(ms))
        //         {
        //             bm2.Save($"{ConfigData.Config.EnviromentPath}wwwroot/documents/" + $"{name}.{extention}");
        //         }
        //     }
        // }

        public static async Task<string> SaveDocumentAsync(string base64, string name, string extension)
        {
            string finalHashName = string.IsNullOrEmpty(extension) ? name : $"{name}.{extension}";
            Console.WriteLine("Hasnhane created");
            using (MemoryStream ms = new MemoryStream())
            {
                Console.WriteLine("Memory stream created");
                await ms.WriteAsync(Convert.FromBase64String(base64));
                Console.WriteLine("Converted to basecreated");
                await Task.Factory.StartNew(() =>
                {
                    using (Bitmap bm2 = new Bitmap(ms))
                    {
                        Console.WriteLine("Bitmap ok");

                        bm2.Save($"{ConfigData.Config.EnviromentPath}wwwroot/documents/" + $"{finalHashName}");

                    }
                });
            }

            return finalHashName;
        }

        public static void SaveImage(string base64, string name)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                using (Bitmap bm2 = new Bitmap(ms))
                {
                    bm2.Save($"{ConfigData.Config.EnviromentPath}wwwroot/images/" + $"{name}.jpg");
                }
            }
        }

        public static async Task SaveImageAsync(string base64, string name)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(Convert.FromBase64String(base64));
                await Task.Factory.StartNew(() =>
                {
                    using (Bitmap bm2 = new Bitmap(ms))
                    {
                        bm2.Save($"{ConfigData.Config.EnviromentPath}wwwroot/images/" + $"{name}.jpg");
                    }
                });
            }
        }

        public static string SaveImageFormUrl(string picUrl, string name)
        {
            try
            {
                var nameReady = $"{name}.jpg";
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(picUrl);

                    using (MemoryStream mem = new MemoryStream(data))
                    {
                        using (var yourImage = Image.FromStream(mem))
                        {
                            // If you want it as Jpeg
                            yourImage.Save($"{ConfigData.Config.EnviromentPath}wwwroot/images/" + $"{nameReady}");
                        }
                    }

                }

                return nameReady;
            }
            catch(Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return "";
            }
        }

        public static async Task<string> SaveAttachmentAsync(string base64, string name, string extension)
        {
            string finalHashName = string.IsNullOrEmpty(extension) ? name : $"{name}.{extension}";
            Console.WriteLine("Hasnhane created");
            using (MemoryStream ms = new MemoryStream())
            {
                Console.WriteLine("Memory stream created");
                await ms.WriteAsync(Convert.FromBase64String(base64));
                Console.WriteLine("Converted to basecreated");
                await Task.Factory.StartNew(() =>
                {
                    using (Bitmap bm2 = new Bitmap(ms))
                    {
                        Console.WriteLine("Bitmap ok");

                        bm2.Save($"{ConfigData.Config.EnviromentPath}wwwroot/attachments/" + $"{finalHashName}");

                    }
                });
            }

            return finalHashName;
        }

        public static string GenerateRandomFileName(FileType type, string idAssociated, string extension)
        {
            return $"{DateTime.Now.Ticks}_{idAssociated}_{type}.{extension}";
        }

        public static void CheckExist(this object obj, string name=null)
        {
            if (obj == null)
            {
                string myObj = name != null ? name : "Item";
                
                throw new CoachOnlineException($"{myObj} does not exist.", CoachOnlineExceptionState.NotExist);
            }
        }

        public static int ToInteger(this string s)
        {
            int result = int.Parse(s);

            return result;
        }
        public static bool IsVideo(this ITusFile file)
        {
            string[] mediaTypes = { "video/x-flv", "application/x-mpegURL", "video/mp4", "application/x-mpegURL", "video/MP2T", "video/3gpp", "video/quicktime", "video/x-msvideo", "video/vnd.mpegurl", "video/x-ms-wmv", "video/x-dv", "video/x-sgi-movie", "video/mpeg", "video/avi", "video/x-m4v", "audio/mpeg3", "audio/mpeg", "audio/x-mpeg-3" };



            if (file == null)
            {
                return false;
            }
            var metadata = file.GetMetadataAsync(new System.Threading.CancellationToken());
            metadata.Wait();
            var md = metadata.Result;
            if (md == null)
            {
                return false;
            }
            var _type = md["filetype"].GetString(System.Text.Encoding.UTF8);

            //if (_type.Contains("video/"))
            //{
            //    return true;
            //}


            Console.WriteLine("Uploaded video type " + _type);
            var existInTypes = mediaTypes.Where(z => z.ToLower() == _type.ToLower()).FirstOrDefault();
            return (existInTypes != null);

        }

        public static bool IsAudio(this ITusFile file)
        {
            string[] mediaTypes = { "audio/mpeg", "audio/x-m4a" };



            if (file == null)
            {
                return false;
            }
            var metadata = file.GetMetadataAsync(new System.Threading.CancellationToken());
            metadata.Wait();
            var md = metadata.Result;
            if (md == null)
            {
                return false;
            }
            var _type = md["filetype"].GetString(System.Text.Encoding.UTF8);

            //if (_type.Contains("video/"))
            //{
            //    return true;
            //}

            Console.WriteLine("Uploaded video type " + _type);
            var existInTypes = mediaTypes.Where(z => z.ToLower() == _type.ToLower()).FirstOrDefault();
            return (existInTypes != null);

        }


        public static void RemoveFile(string Filename, FileType fileType)
        {
            string path = "";

            switch (fileType)
            {
                case FileType.Attachment:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/attachments/";
                    break;
                case FileType.Image:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/images/";
                    break;
                case FileType.Video:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/uploads/";
                    break;
                case FileType.StudentCard:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/student_cards/";
                    break;
                case FileType.Document:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/document/";
                    break;
                default:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/attachments/";
                    break;
            }

            if (File.Exists($"{ConfigData.Config.EnviromentPath}wwwroot/attachments/{Filename}"))
            {
                File.Delete($"{ConfigData.Config.EnviromentPath}wwwroot/attachments/{Filename}");
            }

            if (fileType == FileType.Document) {
                if (File.Exists($"{path}{Filename}"))
                {
                    File.Delete($"{path}{Filename}");
                }
            }
        }


        public static string RemoveDiacritics(string str)
        {
            if (str == null) return null;
            var chars =
                from c in str.Normalize(NormalizationForm.FormD).ToCharArray()
                let uc = CharUnicodeInfo.GetUnicodeCategory(c)
                where uc != UnicodeCategory.NonSpacingMark
                select c;

            var cleanStr = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

            cleanStr = cleanStr.Replace("Ł", "L").Replace("ł", "l");
            return cleanStr;
        }

        static readonly string HTML_TAG_PATTERN = "<.*?>";
        public static string RemoveHTMLTags(string body)
        {
                return Regex.Replace
                  (body, HTML_TAG_PATTERN, string.Empty);
            
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }



    //public enum FileType2
    //{
    //    UserPhoto,
    //    StudentCard
    //}

}
