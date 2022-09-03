﻿using Grpc.Core;
using Microsoft.Maui.Automation.RemoteGrpc;

namespace Microsoft.Maui.Automation
{
    public interface IApplication
    {
        public Platform DefaultPlatform { get; }

        public Task<IEnumerable<Element>> GetElements(Platform platform);

        public Task<IEnumerable<Element>> FindElements(Platform platform, Func<Element, bool> matcher);
        
        public Task<string> GetProperty(Platform platform, string elementId, string propertyName);
    }
}