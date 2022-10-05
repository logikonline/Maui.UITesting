﻿#if IOS || MACCATALYST
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Automation
{
	public class iOSApplication : Application
	{
		public override Platform DefaultPlatform => Platform.Ios;

		public override async Task<string> GetProperty(string elementId, string propertyName)
		{
			var selector = new ObjCRuntime.Selector(propertyName);
			var getSelector = new ObjCRuntime.Selector("get" + System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(propertyName));

			var element = (await FindElements(e => e.Id?.Equals(elementId) ?? false))?.FirstOrDefault();

			if (element is not null && element.PlatformElement is NSObject nsobj)
			{
				if (nsobj.RespondsToSelector(selector))
				{
					var v = nsobj.PerformSelector(selector)?.ToString();
					if (v != null)
						return v;
				}

				if (nsobj.RespondsToSelector(getSelector))
				{
					var v = nsobj.PerformSelector(getSelector)?.ToString();
					if (v != null)
						return v;
				}
			}

			return string.Empty;
		}

		public override Task<IEnumerable<IElement>> GetElements()
		{
			var root = GetRootElements(-1);

			return Task.FromResult(root);
		}


		IEnumerable<IElement> GetRootElements(int depth)
		{
			var children = new List<Element>();

			var scenes = UIApplication.SharedApplication.ConnectedScenes?.ToArray();

			var hadScenes = false;

			if (scenes?.Any() ?? false)
			{
				foreach (var scene in scenes)
				{
					if (scene is UIWindowScene windowScene)
					{
						foreach (var window in windowScene.Windows)
						{
							children.Add(window.GetElement(this, 1, depth));
							hadScenes = true;
						}
					}
				}
			}


			if (!hadScenes)
			{
				if (!OperatingSystem.IsMacCatalystVersionAtLeast(15, 0) && !OperatingSystem.IsIOSVersionAtLeast(15, 0))
				{
					foreach (var window in UIApplication.SharedApplication.Windows)
					{
						children.Add(window.GetElement(this, 1, depth));
					}
				}
			}

			return children;
		}

		public override Task<IEnumerable<IElement>> FindElements(Predicate<IElement> matcher)
		{
			var windows = GetRootElements(-1);

			var matches = new List<IElement>();
			Traverse(windows, matches, matcher);

			return Task.FromResult<IEnumerable<IElement>>(matches);
		}

		public override Task<PerformActionResult> PerformAction(string action, string elementId, params string[] arguments)
			=> Task.FromResult(new PerformActionResult { Status = -1 });

		void Traverse(IEnumerable<IElement> elements, IList<IElement> matches, Predicate<IElement> matcher)
		{
			foreach (var e in elements)
			{
				if (matcher(e))
					matches.Add(e);

				if (e.PlatformElement is UIView uiView)
				{
					var children = uiView.Subviews?.Select(s => s.GetElement(this, e.Id, 1, 1))
						?.ToList() ?? new List<Element>();
					Traverse(children, matches, matcher);
				}
			}
		}
	}
}
#endif