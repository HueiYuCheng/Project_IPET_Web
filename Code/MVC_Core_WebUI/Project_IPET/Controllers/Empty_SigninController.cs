﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_IPET.Models;
using Project_IPET.Models.EF;
using Project_IPET.Services;
using Project_IPET.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project_IPET.Controllers
{
    public class Empty_SigninController : Controller
    {
        private readonly MyProjectContext _context;
        private readonly IEmailSenderService _emailSender;

        public Empty_SigninController(MyProjectContext context, IEmailSenderService emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(CLoginViewModel vModel)
        {
            Member customer = _context.Members.FirstOrDefault(m => m.UserId == vModel.txtAccount);
            if (customer != null && customer.Password.Equals(vModel.txtPassword))
            {
                string json = JsonSerializer.Serialize(customer);
                HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, json);
                return customer.RoleId == 1 ? RedirectToAction("Index", "Front_Home") : RedirectToAction("Index", "Back_Home");
            }
            return View();
        }


        public IActionResult ForgetPassword()
        {
            return View();
        }

        public async Task TestAction()
        {
            await _emailSender.SendEmailAsync("accoutforsendmail@gmail.com", "This is a test", $"Enter email body here");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
