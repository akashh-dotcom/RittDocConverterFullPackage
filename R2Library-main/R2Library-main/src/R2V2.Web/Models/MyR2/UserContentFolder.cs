#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.MyR2;

#endregion

namespace R2V2.Web.Models.MyR2
{
    [Serializable]
    public class UserContentFolder
    {
        //private readonly List<UserContentItem> _userContentItems;

        //Needed to create Session MyR2
        public UserContentFolder()
        {
            UserContentItems = new List<UserContentItem>();
        }

        public UserContentFolder(Core.MyR2.UserContentFolder userContentFolder, UserContentType userContentType)
        {
            Id = userContentFolder.Id;
            FolderName = userContentFolder.FolderName;
            DefaultFolder = userContentFolder.DefaultFolder;

            UserContentItems = new List<UserContentItem>();

            foreach (var userContentItem in userContentFolder.UserContentItems.OrderByDescending(x => x.CreationDate))
            {
                AddUserContentItem(new UserContentItem(userContentItem, userContentType));
            }
        }

        public int Id { get; set; }
        public string FolderName { get; set; }
        public bool DefaultFolder { get; set; }

        public List<UserContentItem> UserContentItems { get; set; }

        public void AddUserContentItem(UserContentItem userContentItem)
        {
            UserContentItems.Add(userContentItem);
        }

        public void RemoveUserContentItem(UserContentItem userContentItem)
        {
            UserContentItems.Remove(userContentItem);
        }
    }
}