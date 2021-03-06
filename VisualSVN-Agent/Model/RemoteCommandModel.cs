﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.Model
{
    /*
     
     模型主要根据 CommandType 来确定每一个属性的实际意义
         
    */

    public enum CommandType
    {
        NoCommand = 0,
        SetRepositoryPermission,
        DelRepositoryPermission,
        CreatGroup,
        CreateUser,
        CreateRepository,
        DeleteRepository,
        CreateRepositoryFolders,
        DeleteRepositoryFolders,
        AddMemberToGroup,
        DelMemberOnGroup,
        CheckOut,
        Update,
        SetDirectoryAccessRule,
        SetDirectoryShare,
        AddHooks
    }

    public class RemoteCommand
    {
        // 机器标识符
        public string  mID { get; set; }
        public CommandType commandType { get; set; }


        /// <summary>
        /// SetRepositoryPermission Name
        /// DelRepositoryPermission Name
        /// CreateRepositoryFolders repositories
        /// DeleteRepositoryFolders repositories
        /// AddMemberToGroup userName
        /// DelMemberOnGroup userName
        /// SetDirectoryAccessRule identity
        /// </summary>
        public string  name { get; set; }


        /// <summary>
        /// SetRepositoryPermission repository
        /// DelRepositoryPermission repository
        /// CreateRepository repoName
        /// DeleteRepository repoName
        /// </summary>
        public string repoName { get; set; }


        /// <summary>
        /// CreatGroup groupName
        /// CreateUser userName
        /// AddMemberToGroup groupName
        /// DelMemberOnGroup groupName
        /// </summary>
        public string groupName { get; set; }


        /// <summary>
        /// CreateUser password
        /// </summary>
        public string password { get; set; }


        /// <summary>
        /// SetRepositoryPermission permission
        /// SetDirectoryAccessRule type
        /// </summary>
        public int? permission { get; set; }


        /// <summary>
        /// CreateRepositoryFolders message 
        /// DeleteRepositoryFolders message
        /// PS_SvnRepositoryHook Content
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// CreateRepositoryFolders folderName
        /// DeleteRepositoryFolders folderName
        /// SetDirectoryAccessRule  path
        /// </summary>
        public string Folders { get; set; }

        public string svnAccount { get; set; }
        public string svnPassword { get; set; }
        public string  svnRepoUrl { get; set; }
        public string  svnLocalPath { get; set; }
    }
}
