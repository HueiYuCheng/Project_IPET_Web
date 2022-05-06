﻿using Microsoft.AspNetCore.Mvc;
using Project_IPET.Models.EF;
using Project_IPET.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_IPET.Controllers
{
    public class Back_ProjectController : Controller
    {
        private readonly MyProjectContext _context;
        public Back_ProjectController(MyProjectContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IQueryable<CProjectViewModel> list = null;
            list = _context.ProjectDetails.Select(n => new CProjectViewModel
            {
                fId = n.PrjId,
                fTitle = n.Title,
                fGoal = (int)n.Goal,
                fStarttime = n.Starttime.ToString(),
                fEndtime = n.Endtime.ToString(),
                fFoundation = n.Foundation.FoundationName
            });
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }


        public IActionResult ProjectProduct(int Id)
        {
            var productlist = _context.PrjConnects.Where(n => n.PrjId == Id).Select(n=>n.ProductId);

            var list = new List<Product>();
            foreach (var item in productlist.ToList())
            {
                var prod = _context.Products.FirstOrDefault(n => n.ProductId == item);
                list.Add(prod);
            }
            return Json(list);
        }
        public IActionResult ProjectDetail(int Id)
        {
            var Project = _context.ProjectDetails.Where(n=>n.PrjId == Id).Select(p=>new
            {
                detailfoundation = p.Foundation.FoundationName,
                detailgoal = p.Goal,
                detailstarttime = p.Starttime.ToString(),
                detailendtime = p.Endtime.ToString(),
            });
            return Json(Project);
        }
    }
}
