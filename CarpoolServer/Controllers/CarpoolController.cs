using CarpoolServerBL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        [Route("Login")]
        [HttpGet]
        public User Login([FromQuery] string email, [FromQuery] string pass)
        {
            User user = context.Login(email, pass);

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

        [Route("AdultSignUp")]
        [HttpGet]
        public Adult AdultSignUp([FromQuery] string email, [FromQuery] string userName, [FromQuery] string pass,
            [FromQuery] string fName, [FromQuery] string lName, [FromQuery] DateTime birthDate,
            [FromQuery] string phoneNumber, [FromQuery] string photo, [FromQuery] string city,
            [FromQuery] string neighborhood, [FromQuery] string street, [FromQuery] string houseNum)
        {
            User user = new User()
            {
                Email = email,
                UserName = userName,
                UserPswd = pass,
                FirstName = fName,
                LastName = lName,
                BirthDate = birthDate,
                PhoneNum = phoneNumber,
                Photo = photo,
                City = city,
                Neighborhood = neighborhood,
                Street = street,
                HouseNum = houseNum
            };

            Adult adult = new Adult()
            {
                IdNavigation = user
            };

            //Check user name and password
            if (adult != null)
            {
                this.context.AddAdult(adult);
                HttpContext.Session.SetObject("theUser", user);
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

        [Route("KidSignUp")]
        [HttpGet]
        public Kid KidSignUp([FromQuery] string email, [FromQuery] string userName, [FromQuery] string pass,
            [FromQuery] string fName, [FromQuery] string lName, [FromQuery] DateTime birthDate,
            [FromQuery] string phoneNumber, [FromQuery] string photo, [FromQuery] string city,
            [FromQuery] string neighborhood, [FromQuery] string street, [FromQuery] string houseNum)
        {
            User user = new User()
            {
                Email = email,
                UserName = userName,
                UserPswd = pass,
                FirstName = fName,
                LastName = lName,
                BirthDate = birthDate,
                PhoneNum = phoneNumber,
                Photo = photo,
                City = city,
                Neighborhood = neighborhood,
                Street = street,
                HouseNum = houseNum
            };

            Kid kid = new Kid()
            {
                IdNavigation = user
            };

            //Check user name and password
            if (kid != null)
            {
                this.context.AddKid(kid);
                HttpContext.Session.SetObject("theUser", user);
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

        [Route("GetString")]
        [HttpGet]
        public string GetString()
        {
            return "HELLO WORLD";
        }
    }
}