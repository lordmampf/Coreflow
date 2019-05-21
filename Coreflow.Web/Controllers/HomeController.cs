﻿using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Coreflow.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Coreflow.Web.Helper;
using Coreflow.Objects.CodeCreatorFactory;
using Coreflow.Validation;

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

            var fmodel = FlowDefinitionModelMappingHelper.GenerateModel(wfdef);

            FlowDefinitionModelStorage.StoreModel(fmodel, false);

            return RedirectToAction(nameof(Editor), new { id = wfdef.Identifier });
        }

        public IActionResult Editor(Guid id)
        {
            if (id == Guid.Empty)
                return RedirectToAction(nameof(Flows));

            var fmodel = FlowDefinitionModelStorage.GetModel(id);

            var fdef = FlowDefinitionModelMappingHelper.GenerateFlowDefinition(fmodel);
            var result = FlowValidationHelper.Validate(fdef);

            if (result.Messages.Count > 0)
            {
                return RedirectToAction(nameof(Validator), new { id = fmodel.Identifier });
            }

            return View(fmodel);
        }

        public IActionResult Validator(Guid id)
        {
            if (id == Guid.Empty)
                return RedirectToAction(nameof(Flows));

            var fmodel = FlowDefinitionModelStorage.GetModel(id);

            var fdef = FlowDefinitionModelMappingHelper.GenerateFlowDefinition(fmodel);

            var result = FlowValidationHelper.Validate(fdef);

            if (result.Messages.Count <= 0)
            {
                return RedirectToAction(nameof(Editor), new { id = fmodel.Identifier });
            }

            var corrector = CorrectorHelper.GetCorrectors(fdef, result.Messages);

            return View(new ValidatorModel()
            {
                Correctors = corrector,
                FlowDefinition = fmodel,
                ValidationResult = result
            });
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

            FlowDefinitionModelStorage.ResetFlow(id);

            string factoryIdentifier = CallFlowCreatorFactory.GetIdentifier(id);
            Program.CoreflowInstance.CodeCreatorStorage.RemoveFactory(factoryIdentifier);

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
