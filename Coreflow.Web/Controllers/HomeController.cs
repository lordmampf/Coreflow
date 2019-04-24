﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Coreflow.Web.Models;
using Microsoft.AspNetCore.Http;
using Coreflow.Helper;
using Coreflow.Interfaces;
using Coreflow.Objects;
using Coreflow.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Coreflow.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            FlowDefinition wfdef = Program.CoreflowInstance.FlowDefinitionFactory.Create("new");

            Program.CoreflowInstance.FlowDefinitionStorage.Remove(wfdef.Identifier);
            Program.CoreflowInstance.FlowDefinitionStorage.Add(wfdef);

            return RedirectToAction(nameof(Editor), new { id = wfdef.Identifier });
        }

        public IActionResult Editor(Guid id)
        {
            if (id == Guid.Empty)
                return RedirectToAction(nameof(Flows));

            FlowDefinition wfDef = Program.CoreflowInstance.FlowDefinitionStorage.GetDefinitions().FirstOrDefault(wf => wf.Identifier == id);

            var FlowDefinitionModel = FlowDefinitionModelMappingHelper.GenerateModel(wfDef);

            string serialized = FlowDefinitionModelSerializer.Serialize(FlowDefinitionModel);

            HttpContext.Session.SetString("FlowModel", serialized);

            return View(FlowDefinitionModel);
        }

        public IActionResult Flows()
        {
            var Flows = Program.CoreflowInstance.FlowDefinitionStorage.GetDefinitions();
            return View(Flows);
        }

        public IActionResult Instances()
        {
            var Flows = Program.CoreflowInstance.FlowInstanceStorage.GetInstances();
            return View(Flows);
        }

        public IActionResult DeleteFlow(Guid id)
        {
            Program.CoreflowInstance.FlowDefinitionStorage.Remove(id);
            return RedirectToAction(nameof(Flows));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Monaco()
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
