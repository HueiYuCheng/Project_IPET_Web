﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project_IPET.Models;
using Project_IPET.Models.EF;
using Project_IPET.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_IPET.Controllers
{
    public class Front_HomeController : Controller
    {
        private readonly ILogger<Front_HomeController> _logger;

        private readonly MyProjectContext _myProject;

        public Front_HomeController(ILogger<Front_HomeController> logger, MyProjectContext myProject)
        {
            _logger = logger;
            _myProject = myProject;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult BlogView(CPostViewModel postFilter)
        {
            var posts = new CPostFilterFactory(_myProject).PostFilter(postFilter)
                .Where(c => c.ReplyToPost == null)
                .Select(n => new CPostViewModel
            {
                PostId = n.PostId,
                Title = n.Title,
                PostContent = n.PostContent,
                PostDate = n.PostDate,
                LikeCount = n.LikeCount,
                PostImage = n.PostImage,
                MemberName = n.MemberName,
                MemberId = n.MemberId,

            }).Take(3).ToList();

            return PartialView(posts); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
