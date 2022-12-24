﻿using System;

namespace Whetstone.StoryEngine.LambdaUtilities
{
    public class InvalidRoleException : Exception
    {


        public InvalidRoleException(string message, string roleArn) : base(message)
        {

            RoleArn = roleArn;
        }

        public string RoleArn { get; private set; }

    }
}
