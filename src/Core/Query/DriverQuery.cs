﻿using Microsoft.Maui.Automation.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Automation.Querying
{
	public class DriverQuery
	{
		public const int DefaultAutoWaitMilliseconds = 3000;
		public const int DefaultAutoWaitRetryMilliseconds = 200;

		public DriverQuery(IDriver driver)
		{
			Driver = driver;
			Query = new Query();
		}

		public DriverQuery(IDriver driver, Query query)
		{
			Driver = driver;
			Query = query;
		}

		public DriverQuery(IDriver driver, Platform automationPlatform)
		{
			Driver = driver;
			Query = new Query(automationPlatform);
		}

		public readonly IDriver Driver;
		public readonly Query Query;

		public async Task<IElement?> Element(int autoWaitMs = DefaultAutoWaitMilliseconds, int retryDelayMs = DefaultAutoWaitRetryMilliseconds)
			=> (await AutoWait(autoWaitMs, retryDelayMs, false).ConfigureAwait(false)).FirstOrDefault();

		public async Task<bool> NoElements(int autoWaitMs = DefaultAutoWaitMilliseconds, int retryDelayMs = DefaultAutoWaitRetryMilliseconds)
			=> !(await AutoWait(autoWaitMs, retryDelayMs, true).ConfigureAwait(false)).Any();

		public async Task<IEnumerable<IElement>> Elements(int autoWaitMs = DefaultAutoWaitMilliseconds, int retryDelayMs = DefaultAutoWaitRetryMilliseconds)
			=> await AutoWait(autoWaitMs, retryDelayMs, false).ConfigureAwait(false);

		public TaskAwaiter<IEnumerable<IElement>> GetAwaiter()
			=> AutoWait().GetAwaiter();

		async Task<IEnumerable<IElement>> AutoWait(int autoWaitMs = DefaultAutoWaitMilliseconds, int retryDelayMs = DefaultAutoWaitRetryMilliseconds, bool waitForNone = false)
		{
			var waited = 0;

			while (waited < autoWaitMs || autoWaitMs <= 0)
			{
				// See which automation platform to use
				var platform = Query.AutomationPlatform ?? Driver.Configuration.AutomationPlatform;

				var elements = await Driver.GetElements(platform).ConfigureAwait(false);

				var results = await Query.Execute(Driver, elements);

				var anyResults = results.Any();

				if (waitForNone)
				{
					// Wait until no results found
					if (autoWaitMs <= 0 || !anyResults)
					{
						if (anyResults)
							throw new ElementsStillFoundException(Query);

						return results;
					}
				}
				else
				{
					// Wait until we find 1 or more
					if (autoWaitMs <= 0 || anyResults)
						return results;
				}

				Thread.Sleep(retryDelayMs);
				waited += retryDelayMs;
			}

			if (waitForNone)
				throw new ElementsStillFoundException(Query);
			else
				throw new ElementsNotFoundException(Query);
		}

	}
}
