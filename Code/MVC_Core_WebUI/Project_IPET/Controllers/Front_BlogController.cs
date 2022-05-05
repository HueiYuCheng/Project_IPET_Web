﻿using Microsoft.AspNetCore.Mvc;
using Project_IPET.Models.EF;
using Project_IPET.Services;
using Project_IPET.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_IPET.Controllers
{

    public class Front_BlogController : Controller
    {
        private readonly MyProjectContext _myProject;


        public Front_BlogController(MyProjectContext myProject)
        {

            _myProject = myProject;

        }
        public IActionResult Index()
        {
            int countbypage =6;
            int totalpost = _myProject.Posts.Count();
            CTools tools = new CTools();
            tools.Page(countbypage, totalpost, out int tatalpage);

            ViewBag.page = tatalpage;

            return View();
        }
        [HttpPost]
        public IActionResult ListView(int inputpage)
        {
            int page = 1;
            int countbypage = 6;
            int totalpost = _myProject.Posts.Count();


            if (inputpage > 0)
                page = inputpage;

            CTools tools = new CTools();

            tools.Page(countbypage, totalpost, out int tatalpage);

            var posts = _myProject.Posts.Where(c => c.ReplyToPost == null).Select(n => new CPostViewModel
            {

                PostId = n.PostId,
                Title = n.Title,
                PostContent = n.PostContent,
                PostDate = n.PostDate,
                LikeCount = n.LikeCount,
                PostImage = n.PostImage,
                Tag = n.Tag,
                MemberName = n.Member.Name,
                MemberId = n.MemberId,

            }).Skip((page - 1) * countbypage).Take(countbypage).ToList();



            return PartialView(posts);
        }

        public ActionResult PostView()
        {
            return View();
        }

        public ActionResult CreatePost()
        {
            return View();
        }


        public IActionResult TestProductComment(string productname)
        {

            productname = "室內成貓-貓飼料";

            var productcomment = _myProject.Comments
                                 .OrderByDescending(d=>d.CommentDate)
                                 .Where(p=>p.Product.ProductName == productname)
                                 .Select(n => new CCommentViewModel {

                                     MemberName = n.OrderDetail.Order.Member.Name,
                                     MemberImage = n.OrderDetail.Order.Member.Avatar,
                                     Rating = n.Rating,
                                     CommentDate = n.CommentDate,
                                     CommentContent = n.CommentContent,
                                    
                                 }).ToList();

            return View(productcomment);
        }

    }
}
