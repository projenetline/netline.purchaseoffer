using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace netline.purchaseoffer.Controllers
{
    public class CommentController : Controller
    {
        // GET: Comment
        [SessionsController]
        public ActionResult Index()
        {
            return View();
        }

        ProjectUtil util = new ProjectUtil();

        public ActionResult Comment(string CommentId)
        {
            int ProjectId = util.getCommentProjectId(CommentId);



            Ntl_Request  offer = util.getResponses(ProjectId);

            return View(offer);


        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult saveComment(string comment, string commentId)
        {
            byte[] commentByte = Encoding.GetEncoding(1254).GetBytes(comment);
            util.updateComment(commentId, commentByte);

            return Json(Url.Action("Comment", "Comment", new { @CommentId = commentId }));
        }

        public ActionResult GetComment(int ProjectId)
        {
            int suggestedSupplier= util.getSuggestionSupplier(ProjectId);
            string  CommentPersonMail = util.getCommentPersonMail(ProjectId);
            string  CommentPerson = util.getCommentPerson(ProjectId);
            if (suggestedSupplier > 0 && !string.IsNullOrEmpty(CommentPersonMail))
            {            

              
                    Ntl_Comment ntl_Comment= new Ntl_Comment()
                    {
                        CommentGuid=Guid.NewGuid(),
                        PersonEmail=CommentPersonMail,
                        PersonName=CommentPerson,
                        ProjectId=ProjectId,

                    };
                    util.createComment(ntl_Comment);
                    string url=string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
                    util.sendCommentEmail(CommentPersonMail, ntl_Comment.CommentGuid, url);
                



            }


            return RedirectToAction("Confirm", "Demands", new { @ProjectId = ProjectId });
        }
    }
}