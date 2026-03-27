using System;
using System.Collections.Generic;

namespace MvcProject.ViewModels.ProjectTask
{
    public class TaskDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "In Progress";
        public string Priority { get; set; } = "Medium";
        public string ProjectName { get; set; } = string.Empty;
        public string AssigneeName { get; set; } = string.Empty;
        public string AssigneeRole { get; set; } = string.Empty;
        public string AssigneeAvatarUrl { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int TimeLoggedHours { get; set; }
        public int TimeLoggedMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        public List<TaskCommentViewModel> Comments { get; set; } = new List<TaskCommentViewModel>();
        public List<TaskAttachmentViewModel> Attachments { get; set; } = new List<TaskAttachmentViewModel>();
    }

    //public class TaskCommentViewModel
    //{
    //    public int Id { get; set; }
    //    public string AuthorName { get; set; } = string.Empty;
    //    public string AuthorAvatarUrl { get; set; } = string.Empty;
    //    public DateTime CreatedAt { get; set; }
    //    public string Content { get; set; } = string.Empty;
    //    public int Likes { get; set; }
    //    public List<TaskCommentViewModel> Replies { get; set; } = new List<TaskCommentViewModel>();
    //}

    public class TaskAttachmentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string SizeDisplay { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string IconCode { get; set; } = "description";
        public string AccentColorClass { get; set; } = "text-secondary";
        public string BgColorClass { get; set; } = "bg-secondary";
    }
}
