using CarpoolServerBL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SendGridLib;

namespace CarpoolServer.Controllers
{
    [Route("CarpoolAPI")]
    [ApiController]
    public class CarpoolController : ControllerBase
    {
        #region Add connection to the db context using dependency injection
        CarpoolDBContext context;
        //string serverEmail;
        public CarpoolController(CarpoolDBContext context)
        {
            this.context = context;
            //this.serverEmail = Startup.ServerEmail;
        }
        #endregion

        //set the user default photo image name
        public const string DEFAULT_PHOTO = "defaultphoto.jpg";

        #region SendEmail
        private static void SendEmail(string subject, string body, string to, string toName, string from, string fromName, string pswd, string smtpUrl)
        {
            var fromAddress = new MailAddress(from, fromName);
            var toAddress = new MailAddress(to, toName);
            string fromPassword = pswd;

            var smtp = new SmtpClient
            {
                Host = smtpUrl,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        private static async void SendEmail2(string subject, string body, string to, string toName, string from, string fromName, string pswd, string smtpUrl)
        {
            await MailSender.SendEmail(fromName, to, toName, subject, body, "");
        }

        [Route("SendEmailHelper")]
        [HttpGet]
        public void SendEmailHelper([FromQuery] string body, [FromQuery] string to, [FromQuery] string toName)
        {
            string subject = "התראה";
            string from = Startup.ServerEmail;
            string fromName = "Carpool App";
            string pswrd = Startup.ServerEmailPassword;
            SendEmail2(subject, body, to, toName, from, fromName, pswrd, "smtp.gmail.com");
        }
        #endregion

        #region Login
        [Route("Login")]
        [HttpGet]
        public User Login([FromQuery] string email, [FromQuery] string pass)
        {
            User user = this.context.Login(email, pass);

            //Check user name and password
            if (user != null)
            {
                HttpContext.Session.SetObject("theUser", user);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;

                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return user;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region AdultSignUp
        [Route("AdultSignUp")]
        [HttpPost]
        public User AdultSignUp([FromBody] Adult adult)
        {
            //Check user name and password
            if (adult != null)
            {
                User user = this.context.AdultSignUp(adult);

                if (user != null)
                {
                    try
                    {
                        //Copy defualt image for this adult
                        var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                        var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                        System.IO.File.Copy(pathFrom, pathTo);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    HttpContext.Session.SetObject("theUser", user);
                    Response.StatusCode = (int)System.Net.HttpStatusCode.OK;

                    //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                    return user;
                }
                else
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }


                //try
                //{
                //    //Copy defualt image for this adult
                //    var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                //    var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                //    System.IO.File.Copy(pathFrom, pathTo);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //}

                ////adult.IdNavigation.Photo = $"{adult.Id}.jpg";
                ////this.context.SaveChanges();

                //HttpContext.Session.SetObject("theUser", adult);
                //Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                ////Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                //return adult;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region AddKid
        [Route("AddKid")]
        [HttpPost]
        public Kid AddKid([FromBody] Kid kid)
        {
            //Check user name and password
            if (kid != null)
            {
                User currentUser = HttpContext.Session.GetObject<User>("theUser");
                Adult currentAdult = new Adult()
                {
                    IdNavigation = currentUser
                };

                try
                {
                    context.Kids.Add(kid);
                    context.SaveChanges();

                    //ילד שכבר קיים להורה
                    KidsOfAdult existKidOfAdult = context.KidsOfAdults.Where(a => a.AdultId == currentAdult.IdNavigation.Id).FirstOrDefault();
                    if (existKidOfAdult != null)
                    {
                        int existKidId = existKidOfAdult.KidId;
                        //אוסף כל ההורים השייכים לילד שקיים
                        IQueryable<KidsOfAdult> kidsOfAdults = context.KidsOfAdults.Where(k => k.KidId == existKidId);

                        if (kidsOfAdults != null)
                        {
                            //הוספת ההורים הקיימים לילד החדש
                            foreach (KidsOfAdult kidsOf in kidsOfAdults)
                            {
                                KidsOfAdult newKidsOfAdult = new KidsOfAdult()
                                {
                                    AdultId = kidsOf.AdultId,
                                    KidId = kid.Id
                                };
                                context.KidsOfAdults.Add(newKidsOfAdult);
                            }
                        }
                    }
                    //אם לא קיימים עוד ילדים אז להוסיף את ההורה לילד החדש
                    else
                    {
                        KidsOfAdult kidsOfAdult = new KidsOfAdult()
                        {
                            AdultId = currentAdult.IdNavigation.Id,
                            KidId = kid.Id
                        };
                        context.KidsOfAdults.Add(kidsOfAdult);
                    }
                    context.SaveChanges();

                    //Copy defualt image for this adult
                    var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                    var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{kid.Id}.jpg");
                    System.IO.File.Copy(pathFrom, pathTo);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return kid;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region AddAdult
        [Route("AddAdult")]
        [HttpPost]
        public Adult AddAdult([FromBody] Adult adult)
        {
            //Check user name and password
            if (adult != null)
            {
                User currentUser = HttpContext.Session.GetObject<User>("theUser");
                Adult currentAdult = new Adult()
                {
                    IdNavigation = currentUser
                };

                try
                {
                    this.context.AddAdult(currentAdult, adult);

                    //Copy defualt image for this adult
                    var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                    var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                    System.IO.File.Copy(pathFrom, pathTo);                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return adult;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region UpdateUser
        [Route("UpdateUser")]
        [HttpPost]
        public User UpdateUser([FromBody] User user)
        {
            //If user is null the request is bad
            if (user == null)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return null;
            }

            User currentUser = HttpContext.Session.GetObject<User>("theUser");
            //Check if user logged in and its ID is the same as the contact user ID
            if (currentUser != null && currentUser.Id == user.Id)
            {
                User updatedUser = context.UpdateUser(currentUser, user);

                if (updatedUser == null)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return updatedUser;

                ////Now check if an image exist for the contact (photo). If not, set the default image!
                //var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", DEFAULT_PHOTO);
                //var targetPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{user.Id}.jpg");
                //System.IO.File.Copy(sourcePath, targetPath);

                //return the contact with its new ID if that was a new contact
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region AddActivity
        [Route("AddActivity")]
        [HttpPost]
        public Activity AddActivity([FromBody] Activity activity)
        {
            if (activity != null)
            {
                this.context.AddActivity(activity);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return activity;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region JoinToActivity
        [Route("JoinToActivity")]
        [HttpPost]
        public KidsInActivity JoinToActivity([FromBody] KidsInActivity kidsIn)
        {
            if (kidsIn != null)
            {
                this.context.JoinToActivity(kidsIn);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return kidsIn;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region AddCarpool
        [Route("AddCarpool")]
        [HttpPost]
        public Carpool AddCarpool([FromBody] Carpool carpool)
        {
            if (carpool != null)
            {
                this.context.AddCarpool(carpool);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return carpool;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region JoinToCarpool
        [Route("JoinToCarpool")]
        [HttpPost]
        public KidsInCarpool JoinToCarpool([FromBody] KidsInCarpool kidsIn)
        {
            if (kidsIn != null)
            {
                this.context.JoinToCarpool(kidsIn);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return kidsIn;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region GetAllKids
        [Route("GetAllKids")]
        [HttpGet]
        public List<Kid> GetAllKids([FromQuery] Adult adult)
        {
            User currentUser = HttpContext.Session.GetObject<User>("theUser");
            Adult currentAdult = new Adult()
            {
                IdNavigation = currentUser
            };

            List<Kid> kids = this.context.GetAllKids(currentAdult);

            if (kids != null)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return kids;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion        

        #region GetKidActivities
        [Route("GetKidActivities")]
        [HttpPost]
        public List<Activity> GetAllActivities([FromBody] Kid kid)
        {
            if (kid != null)
            {
                List<Activity> activities = this.context.GetKidActivities(kid);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return activities;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region GetKidCarpools
        [Route("GetKidCarpools")]
        [HttpPost]
        public List<Carpool> GetKidCarpools([FromBody] Kid kid)
        {
            if (kid != null)
            {
                List<Carpool> carpools = this.context.GetKidCarpools(kid);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return carpools;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region GetAllKidsCarpools
        [Route("GetAllKidsCarpools")]
        [HttpPost]
        public List<Carpool> GetAllKidsCarpools([FromBody] Adult adult)
        {
            if (adult != null)
            {
                List<Carpool> carpools = this.context.GetAllKidsCarpools(adult);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return carpools;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region GetAdultCarpools
        [Route("GetAdultCarpools")]
        [HttpPost]
        public List<Carpool> GetAdultCarpools([FromBody] Adult adult)
        {
            if (adult != null)
            {
                List<Carpool> carpools = this.context.GetAdultCarpools(adult);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return carpools;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region GetCarpoolsInActivity
        [Route("GetCarpoolsInActivity")]
        [HttpPost]
        public List<Carpool> GetCarpoolsInActivity([FromBody] Activity activity)
        {
            if (activity != null)
            {
                List<Carpool> carpools = this.context.GetCarpoolsInActivity(activity);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return carpools;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region GetKidsInCarpool
        [Route("GetKidsInCarpool")]
        [HttpPost]
        public List<Kid> GetKidsInCarpool([FromBody] Carpool carpool)
        {
            List<Kid> kids = this.context.GetKidsInCarpool(carpool);

            if (kids != null)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return kids;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region AddRequestToJoinCarpool
        [Route("AddRequestToJoinCarpool")]
        [HttpGet]
        public bool AddRequestToJoinCarpool([FromQuery] int kidId, [FromQuery] int carpoolId)
        {

            bool addRequest = this.context.AddRequestToJoinCarpool(kidId, carpoolId);
            if (addRequest)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region GetRequestsToJoinCarpool
        [Route("GetRequestsToJoinCarpool")]
        [HttpGet]
        public List<KidsInCarpool> GetRequestsToJoinCarpool([FromQuery] int adultId)
        {
            List<KidsInCarpool> requests = this.context.GetRequestsToJoinCarpool(adultId);
            if (requests != null)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return requests;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region ApproveRequestToJoinCarpool
        [Route("ApproveRequestToJoinCarpool")]
        [HttpGet]
        public bool ApproveRequestToJoinCarpool([FromQuery] int kidId, [FromQuery] int carpoolId)
        {
            bool approved = this.context.ApproveRequestToJoinCarpool(kidId, carpoolId);
            if (approved)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }         
        }
        #endregion

        #region DeclineRequestToJoinCarpool
        [Route("DeclineRequestToJoinCarpool")]
        [HttpGet]
        public bool DeclineRequestToJoinCarpool([FromQuery] int kidId, [FromQuery] int carpoolId)
        {
            bool declined = this.context.DeclineRequestToJoinCarpool(kidId, carpoolId);
            if (declined)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region CarpoolInProcess
        [Route("CarpoolInProcess")]
        [HttpGet]
        public bool CarpoolInProcess([FromQuery] int carpoolId)
        {
            bool succeed = this.context.CarpoolInProcess(carpoolId);
            if (succeed)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region CarpoolEnded
        [Route("CarpoolEnded")]
        [HttpGet]
        public bool CarpoolEnded([FromQuery] int carpoolId)
        {
            bool succeed = this.context.CarpoolEnded(carpoolId);
            if (succeed)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region IsEmailExist
        [Route("IsEmailExist")]
        [HttpGet]
        public bool IsEmailExist([FromQuery] string email)
        {
            bool isExist = this.context.EmailExist(email);
            if (isExist)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region IsUserNameExist
        [Route("IsUserNameExist")]
        [HttpGet]
        public bool IsUserNameExist([FromQuery] string userName)
        {
            bool isExist = this.context.UserNameExist(userName);
            if (isExist)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region IsActivityExist
        [Route("IsActivityExist")]
        [HttpGet]
        public bool IsActivityExist([FromQuery] int activityId)
        {
            bool isExist = this.context.ActivityExist(activityId);
            if (isExist)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }
        #endregion

        #region IsKidInActiveCarpool
        [Route("IsKidInActiveCarpool")]
        [HttpPost]
        public Carpool IsKidInActiveCarpool([FromBody] Kid kid)
        {
            if (kid != null)
            {
                Carpool carpool = this.context.IsKidInActiveCarpool(kid);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return carpool;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }
        #endregion

        #region UploadImage
        [Route("UploadImage")]
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            User user = HttpContext.Session.GetObject<User>("theUser");
            //Check if user logged in and its ID is the same as the contact user ID
            if (user != null)
            {
                if (file == null)
                {
                    return BadRequest();
                }

                try
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", file.FileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }


                    return Ok(new { length = file.Length, name = file.FileName });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return BadRequest();
                }
            }
            return Forbid();
        }
        #endregion
    }
}