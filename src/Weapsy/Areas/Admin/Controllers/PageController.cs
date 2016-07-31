﻿using System;
using Weapsy.Mvc.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weapsy.Reporting.Pages;
using Weapsy.Domain.Model.Pages;
using Weapsy.Domain.Model.Pages.Commands;
using Weapsy.Core.Dispatcher;
using Weapsy.Mvc.Context;
using AutoMapper;
using System.Collections.Generic;

namespace Weapsy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PageController : BaseAdminController
    {
        private readonly IPageFacade _pageFacade;
        private readonly ICommandSender _commandSender;
        private readonly IMapper _mapper;

        public PageController(IPageFacade pageFacade,
            ICommandSender commandSender,
            IMapper mapper,
            IContextService contextService)
            : base(contextService)
        {
            _pageFacade = pageFacade;
            _commandSender = commandSender;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _pageFacade.GetAllForAdminAsync(SiteId);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _pageFacade.GetDefaultAdminModelAsync(SiteId);
            return View(model);
        }

        public async Task<IActionResult> Save(PageAdminModel model)
        {
            var command = _mapper.Map<CreatePage>(model);
            command.SiteId = SiteId;
            command.Id = Guid.NewGuid();
            command.PagePermissions = GetSelectedPermissions(model.PagePermissions);
            await Task.Run(() => _commandSender.Send<CreatePage, Page>(command));
            return new NoContentResult();
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await _pageFacade.GetAdminModelAsync(SiteId, id);
            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> Update(PageAdminModel model)
        {
            var command = _mapper.Map<UpdatePageDetails>(model);
            command.SiteId = SiteId;
            command.PagePermissions = GetSelectedPermissions(model.PagePermissions);
            await Task.Run(() => _commandSender.Send<UpdatePageDetails, Page>(command));
            return new NoContentResult();
        }

        private List<PagePermission> GetSelectedPermissions(IList<PagePermissionModel> models)
        {
            var result = new List<PagePermission>();

            foreach (var permission in models)
            {
                if (permission.Selected)
                    result.Add(new PagePermission
                    {
                        PageId = permission.PageId,
                        RoleId = permission.RoleId,
                        Type = permission.Type
                    });
            }

            return result;
        }
    }
}
