using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using System.Text;
using System.Security.Cryptography;

namespace Blog.Controllers
{
    public class AccountsController : Controller
    {
        private BlogModel model = new BlogModel();

        /* Attempting to secure the password by hashing the password with random numbers
         * so that if password is decrypted, it can be used only if server generates same 
         * render number
         */
        public ActionResult Login(string name, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                Random random = new Random();
                byte[] randomData = new byte[sizeof(long)];
                random.NextBytes(randomData);
                string newNonce = BitConverter.ToUInt64(randomData, 0).ToString("X16");
                Session["Nonce"] = newNonce;
                return View(model: newNonce);
            }
            
            //If the hash is present, validate it
            Administrator administrator = model.Administrators.Where(x => x.Name == name).FirstOrDefault();
            //get the current random number
            string nonce = Session["Nonce"] as string;
            if (administrator == null || string.IsNullOrWhiteSpace(nonce))
            {
                return RedirectToAction("Index", "Posts");
            }

            
            string computedHash;
            //Generate the hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashInput = Encoding.ASCII.GetBytes(administrator.Password + nonce);
                byte[] hashData = sha256.ComputeHash(hashInput);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte value in hashData)
                {
                    stringBuilder.AppendFormat("{0:X2}", value);
                }
                computedHash = stringBuilder.ToString();
            }

            //Administrator is allowd logging in if the two hashes match
            Session["IsAdmin"] = (computedHash.ToLower() == hash.ToLower());
            return RedirectToAction("Index", "Posts");
        }

        //This handler clears out current random number and admin session
        public ActionResult Logout()
        {
            Session["Nonce"] = null;
            Session["IsAdmin"] = null;
            return RedirectToAction("Index", "Posts");
        }

    }
}
