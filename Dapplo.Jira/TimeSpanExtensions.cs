﻿#region Dapplo 2017 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2017 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.Jira
// 
// Dapplo.Jira is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.Jira is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.Jira. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

#region Usings

using System;
using System.Text;
using System.Text.RegularExpressions;
using Dapplo.Jira.Entities;

#endregion

namespace Dapplo.Jira
{
	/// <summary>
	///     These methods help to convert jira working time to real time and back
	/// </summary>
	public static class TimeSpanExtensions
	{
		/// <summary>
		/// </summary>
		/// <param name="workingTime">string from Jira</param>
		/// <param name="timeTrackingConfiguration">TimeTrackingConfiguration</param>
		/// <returns></returns>
		public static TimeSpan FromWorkingTime(string workingTime, TimeTrackingConfiguration timeTrackingConfiguration = null)
		{
			timeTrackingConfiguration = timeTrackingConfiguration ?? new TimeTrackingConfiguration();
			var result = TimeSpan.Zero;
			var weeksMatch = Regex.Match(workingTime, "(\\d+)w");
			if (weeksMatch.Success)
			{
				var weeks = int.Parse(weeksMatch.Groups[1].Value);
				var realHours = weeks * timeTrackingConfiguration.WorkingDaysPerWeek * timeTrackingConfiguration.WorkingHoursPerDay;
				result = result.Add(TimeSpan.FromHours(realHours));
			}
			var daysMatch = Regex.Match(workingTime, "(\\d+)d");
			if (daysMatch.Success)
			{
				var days = int.Parse(daysMatch.Groups[1].Value);
				var realHours = days * timeTrackingConfiguration.WorkingHoursPerDay;
				result = result.Add(TimeSpan.FromHours(realHours));
			}
			var hoursMatch = Regex.Match(workingTime, "(\\d+)h");
			if (hoursMatch.Success)
			{
				var hours = int.Parse(hoursMatch.Groups[1].Value);
				result = result.Add(TimeSpan.FromHours(hours));
			}
			var minutesMatch = Regex.Match(workingTime, "(\\d+)m");
			if (minutesMatch.Success)
			{
				var minutes = int.Parse(minutesMatch.Groups[1].Value);
				result = result.Add(TimeSpan.FromMinutes(minutes));
			}
			return result;
		}

		/// <summary>
		///     Create an increment from the timespan.
		///     increment has of (+/-)nn(y|M|w|d|h|m)
		///     If the plus/minus(+/-) sign is omitted, plus is assumed.
		///     nn: number; y: year, M: month; w: week; d: day; h: hour; m: minute.
		/// </summary>
		/// <param name="timeSpan">TimeSpan to convert</param>
		/// <returns>string</returns>
		public static string TimeSpanToIncrement(this TimeSpan? timeSpan)
		{
			if (!timeSpan.HasValue)
			{
				return "";
			}
			if (timeSpan.Value.TotalMilliseconds < 0)
			{
				return $"-{timeSpan.TimeSpanToIncrement()}";
			}
			return $"{timeSpan.TimeSpanToIncrement()}";
		}

		/// <summary>
		///     Create an increment from the timespan.
		///     increment has of (+/-)nn(y|M|w|d|h|m)
		///     If the plus/minus(+/-) sign is omitted, plus is assumed.
		///     nn: number; y: year, M: month; w: week; d: day; h: hour; m: minute.
		/// </summary>
		/// <param name="timeSpan">TimeSpan to convert</param>
		/// <param name="timeTrackingConfiguration">TimeTrackingConfiguration for calculating the values</param>
		/// <returns>string</returns>
		public static string ToWorkingTime(this TimeSpan? timeSpan, TimeTrackingConfiguration timeTrackingConfiguration = null)
		{
			if (!timeSpan.HasValue)
			{
				return "";
			}
			return timeSpan.Value.ToWorkingTime(timeTrackingConfiguration);
		}

		/// <summary>
		///     Create something that represents the jira working time format
		///     (+/-)nn(y|M|w|d|h|m)
		///     nn: number; y: year, M: month; w: week; d: day; h: hour; m: minute.
		/// </summary>
		/// <param name="timeSpan">TimeSpan to convert</param>
		/// <param name="timeTrackingConfiguration">TimeTrackingConfiguration for calculating the values</param>
		/// <returns>string</returns>
		public static string ToWorkingTime(this TimeSpan timeSpan, TimeTrackingConfiguration timeTrackingConfiguration = null)
		{
			timeTrackingConfiguration = timeTrackingConfiguration ?? new TimeTrackingConfiguration();

			var hours = timeSpan.Hours + timeSpan.Days * 24;
			var minutes = timeSpan.Minutes;

			// Calculate the days from the hours (one normal day has 24 hours, working hours can be 8 -> 3 working days)
			var workingDays = hours / timeTrackingConfiguration.WorkingHoursPerDay;

			// Calculate the working hours, by using the rest of the devision e.g 9 hours with 8 hour working day -> 1 hour leftover
			var workingHours = hours % timeTrackingConfiguration.WorkingHoursPerDay;

			// Calculate the weeks from the working days (5 days a week)
			var workingWeeks = Math.Floor(workingDays / timeTrackingConfiguration.WorkingDaysPerWeek);

			// Modify the workingDays to the rest -> 6 working days, 1w and 1d
			workingDays = workingDays % timeTrackingConfiguration.WorkingDaysPerWeek;

			var jiraTimeRange = new StringBuilder();
			if (workingWeeks > 0)
			{
				jiraTimeRange.AppendFormat("{0}w ", workingWeeks);
			}
			if (workingDays > 0)
			{
				jiraTimeRange.AppendFormat("{0}d ", workingDays);
			}
			if (workingHours > 0)
			{
				jiraTimeRange.AppendFormat("{0}h ", workingHours);
			}
			if (minutes > 0)
			{
				jiraTimeRange.AppendFormat("{0}m ", minutes);
			}
			return jiraTimeRange.ToString().Trim();
		}
	}
}