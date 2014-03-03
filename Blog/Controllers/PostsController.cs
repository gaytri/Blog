using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;

namespace Blog.Controllers
{
    public class PostsController : Controller
    {
        //accessing the model
        private BlogModel model = new BlogModel();

        public ActionResult Index()
        {
            return View();
        }

        //To enter data into the model
        public ActionResult Update(int? id, string title, string body, DateTime dateTime, string tags)
        {
            if (!IsAdmin)
            {
                return RedirectToAction("Index");
            }

            //Get the POst if it exists
            Post post = GetPost(id);
            //Once the Post is obtained, populate the properties of post
            post.Title = title;
            post.Body = body;
            post.DateTime = dateTime;
            //Tag is a list of tag objects, so clear it first
            post.Tags.Clear();

            //Verify that input sequence of tag names is not Null, then split sequence into a list of tag names
            tags = tags ?? string.Empty;
            string[] tagNames = tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //Adding the Tag for each item in the list
            foreach (string tagName in tagNames)
            {
                post.Tags.Add(GetTag(tagName));
            }

            //If this is a new Post, add it
            if (!id.HasValue)
            {
                model.AddToPosts(post);
            }
            //save the entries to the database
            model.SaveChanges();
            //Redirect to the Detailed view of the new created Post
            return RedirectToAction("Details", new { id = post.ID });

        }

        private Tag GetTag(string tagName)
        {
            //Either select the existing tag or create a new one
            return model.Tags.Where(x => x.Name == tagName).FirstOrDefault() ?? new Tag() { Name = tagName };
        }

        private Post GetPost(int? id)
        {
            //If the id is set, select the post else create a new one
            return id.HasValue ? model.Posts.Where(x => x.ID == id).First() : new Post() { ID = -1 };

        }


        // Returning true for now, modify later...
        public bool IsAdmin { get { return true; /* return Session["IsAdmin"] != null && (bool)Session["IsAdmin"];*/ } }
 

    }
}
