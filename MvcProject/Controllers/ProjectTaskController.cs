using Microsoft.AspNetCore.Mvc;
using MvcProject.Attributes;
using MvcProject.ViewModels.ProjectTask;
using System;
using System.Collections.Generic;

namespace MvcProject.Controllers
{
    public class ProjectTaskController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public IActionResult Create(int id)
        {

            return View();
        }
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]

        public IActionResult Edit(int id)
        {
            return View();
        }

        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]
        public IActionResult Delete(int id)
        {
            return View();
        }
        
        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager)]

        public IActionResult Assign(int id, string userId)
        {
            return View();
        }


        [ProjectAuthorize(ProjectRole.Admin, ProjectRole.Manager, ProjectRole.Member)]
        public IActionResult Detail(int id = 1)
        {
            // Mock data based on the Unified Final design prompt
            var model = new TaskDetailViewModel
            {
                Id = id,
                Title = "Finalize ad creative assets",
                Description = "Prepare the final visual direction for the Q4 global social campaign, including motion graphics and static display units.",
                Status = "In Progress",
                Priority = "High Priority",
                ProjectName = "Marketing Launch",
                AssigneeName = "Sarah Jenkins",
                AssigneeRole = "Lead Designer",
                AssigneeAvatarUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDZunMD0zJnzAIR6mJ_dNGLNns1gn8B6iWPIQK0yOYExA-gt-BspJ1HUlYslyWcdIQn4ggWpyABOgmIWdhZ1wf3-rAKdkvsbEv7mXiZzpJQoU43dNMoj07Efn5e0SvsjlvzsrehSIbcOXVdb4eLhqMjpFkRodUOtv4VFKq5sxleZRR0jczeEVLdEjQAZ1XeRVtyIsVi-x7f8MQ7M93E6VFnrHfrQk4TI1YRNqI7usZmE3KFBu_WzEHnlCQjflCh-KLpH7bAtDo6LrXn",
                DueDate = DateTime.Today.AddDays(3),
                TimeLoggedHours = 12,
                TimeLoggedMinutes = 45,
                CreatedAt = new DateTime(2026, 10, 10),
                UpdatedAt = DateTime.Now,
                Tags = new List<string> { "Marketing", "Creative" }
            };

            model.Attachments.Add(new TaskAttachmentViewModel 
            { 
                FileName = "campaign_brief.pdf", 
                SizeDisplay = "1.2 MB", 
                UploadedAt = DateTime.Now.AddHours(-2),
                IconCode = "picture_as_pdf",
                AccentColorClass = "text-danger",
                BgColorClass = "bg-danger"
            });
            
            model.Attachments.Add(new TaskAttachmentViewModel 
            { 
                FileName = "assets.zip", 
                SizeDisplay = "45.8 MB", 
                UploadedAt = DateTime.Now.AddHours(-4),
                IconCode = "folder_zip",
                AccentColorClass = "text-primary",
                BgColorClass = "bg-primary"
            });

            var comment1 = new TaskCommentViewModel
            {
                AuthorName = "Marcus Thorne",
                AuthorAvatarUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBn6cQ-BvDnPuXbjR8ppMKmohWjyVSMzBzb9NNq5MMiL8T9UtSQPSXmlpetfdLxT-fvyqtQYwpFrLpayZ9jbFobnk98Xdq7kFnVc_ln4PRaR7d7E_DsLErpK2oOXL2vQ4MOVChgxPXgOegaKWwDlF2xeshw1DYNzfq7iY_FfwIYSIRqiFHFvlU-h1q7SPritq1E7RiQUqyLOmpSovMHmDHP9fvn55Hz4rTWbIguAWvNsJYGtlE92h1QzBcjP3SgzcQusdzv-98MMftu",
                CreatedAt = DateTime.Now.AddHours(-2),
                Likes = 2,
                Content = "I've just uploaded the updated brief. We decided to pivot the messaging on the static banners to focus more on 'Velocity' than 'Integration'. Please check the new copy on page 4."
            };

            var reply1 = new TaskCommentViewModel
            {
                AuthorName = "Sarah Jenkins",
                AuthorAvatarUrl = model.AssigneeAvatarUrl,
                CreatedAt = DateTime.Now.AddHours(-1),
                Content = "Got it, Marcus. Updating the typography on the statics now. Should be ready in an hour."
            };

            comment1.Replies.Add(reply1);
            model.Comments.Add(comment1);

            return View(model);
        }
    }
}
