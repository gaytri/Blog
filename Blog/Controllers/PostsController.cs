using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using System.Text;

namespace Blog.Controllers
{
    public class PostsController : Controller
    {
        //accessing the model
        private BlogModel model = new BlogModel();
        private const int PostsPerPage = 4;

        public ActionResult Index(int? id)
        {
            int pageNumber = id ?? 0;
            //Get a list of posts where date is valid and order by date
            IEnumerable<Post> posts =
                (from post in model.Posts
                 where post.DateTime < DateTime.Now
                 orderby post.DateTime descending
                 select post).Skip(pageNumber * PostsPerPage).Take(PostsPerPage + 1);
            ViewBag.IsPreviousLinkVisible = pageNumber > 0;
            ViewBag.IsNextLinkVisible = posts.Count() > PostsPerPage;
            ViewBag.PageNumber = pageNumber;
            //Tell the view if administrator is logged in
            ViewBag.IsAdmin = IsAdmin;
            return View(posts.Take(PostsPerPage));
        }

        //To view the complete details of the selected post
        public ActionResult Details(int id)
        {
            Post post = GetPost(id);
            ViewBag.IsAdmin = IsAdmin;
            return View(post);
        }

        //Take care of the dangerous input
        [ValidateInput(false)]
        public ActionResult Comment(int id, string name, string email, string body)
        {
            Post post = GetPost(id);
            Comment comment = new Comment();
            //Point the comment at the post
            comment.Post = post;
            comment.DateTime = DateTime.Now;
            comment.Name = name;
            comment.Email = email;
            comment.Body = body;
            model.AddToComments(comment);
            model.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult Tags(string id)
        {
            Tag tag = GetTag(id);
            ViewBag.IsAdmin = IsAdmin;
            return View("Index", tag.Posts);
        }

        [ValidateInput(false)]
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

        //Delete action only for the Administrator 
        public ActionResult Delete(int id)
        {
            if (IsAdmin)
            {
                Post post = GetPost(id);
                model.DeleteObject(post);
                model.SaveChanges();
            }
            //After delete render to the Index page displaying rest of the posts
            return RedirectToAction("Index");

        }

        public ActionResult DeleteComment(int id)
        {
            if (IsAdmin)
            {
                Comment comment = model.Comments.Where(x => x.ID == id).First();
                model.DeleteObject(comment);
                model.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //An action to prompt the user to enter data into the posts
        public ActionResult Edit(int? id)
        {
            //Get the requested post and accumulate list of current tagnames
            Post post = GetPost(id);
            StringBuilder tagList = new StringBuilder();
            foreach (Tag tag in post.Tags)
            {
                tagList.AppendFormat("{0} ", tag.Name);
            }
            //pass this list to the view
            ViewBag.Tags = tagList.ToString();
            //Return a view of this post
            return View(post);
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


        public bool IsAdmin { get { return true; /*return Session["IsAdmin"] != null && (bool)Session["IsAdmin"];  */} }

        
    }
}
