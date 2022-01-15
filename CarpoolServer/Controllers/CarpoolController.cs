using CarpoolServerBL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarpoolServer.Controllers
{
    [Route("CarpoolAPI")]
    [ApiController]
    public class CarpoolController : ControllerBase
    {
        #region Add connection to the db context using dependency injection
        CarpoolDBContext context;
        public CarpoolController(CarpoolDBContext context)
        {
            this.context = context;
        }
        #endregion

        //set the user default photo image name
        public const string DEFAULT_PHOTO = "defaultphoto.jpg";

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
        public Adult AdultSignUp([FromBody] Adult adult)
        {
            //Check user name and password
            if (adult != null)
            {
                this.context.AdultSignUp(adult);

                //Copy defualt image for this adult
                var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                System.IO.File.Copy(pathFrom, pathTo);

                //adult.IdNavigation.Photo = $"{adult.Id}.jpg";
                //this.context.SaveChanges();

                HttpContext.Session.SetObject("theUser", adult);
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

                //this.context.AddKid(currentAdult, kid);

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

                this.context.AddAdult(currentAdult, adult);

                //Copy defualt image for this adult
                var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                System.IO.File.Copy(pathFrom, pathTo);

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
            //Check user name and password
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