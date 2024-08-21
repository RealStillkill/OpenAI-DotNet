﻿// Licensed under the MIT License. See LICENSE in the project root for license information.

using OpenAI.Extensions;
using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenAI
{
    public abstract class BaseResponse
    {
        /// <summary>
        /// The <see cref="OpenAIClient"/> this response was generated from.
        /// </summary>
        [JsonIgnore]
        public OpenAIClient Client { get; internal set; }

        /// <summary>
        /// The server-side processing time as reported by the API.  This can be useful for debugging where a delay occurs.
        /// </summary>
        [JsonIgnore]
        public TimeSpan ProcessingTime { get; internal set; }

        /// <summary>
        /// The organization associated with the API request, as reported by the API.
        /// </summary>
        [JsonIgnore]
        public string Organization { get; internal set; }

        /// <summary>
        /// The request id of this API call, as reported in the response headers.  This may be useful for troubleshooting or when contacting OpenAI support in reference to a specific request.
        /// </summary>
        [JsonIgnore]
        public string RequestId { get; internal set; }

        /// <summary>
        /// The version of the API used to generate this response, as reported in the response headers.
        /// </summary>
        [JsonIgnore]
        public string OpenAIVersion { get; internal set; }

        /// <summary>
        /// The maximum number of requests that are permitted before exhausting the rate limit.
        /// </summary>
        [JsonIgnore]
        public int? LimitRequests { get; internal set; }

        /// <summary>
        /// The maximum number of tokens that are permitted before exhausting the rate limit.
        /// </summary>
        [JsonIgnore]
        public int? LimitTokens { get; internal set; }

        /// <summary>
        /// The remaining number of requests that are permitted before exhausting the rate limit.
        /// </summary>
        [JsonIgnore]
        public int? RemainingRequests { get; internal set; }

        /// <summary>
        /// The remaining number of tokens that are permitted before exhausting the rate limit.
        /// </summary>
        [JsonIgnore]
        public int? RemainingTokens { get; internal set; }

        /// <summary>
        /// The time until the rate limit (based on requests) resets to its initial state.
        /// </summary>
        [JsonIgnore]
        public string ResetRequests { get; internal set; }

        /// <summary>
        /// The time until the rate limit (based on requests) resets to its initial state represented as a TimeSpan.
        /// </summary>
        public TimeSpan ResetRequestsTimespan { get => ConvertTimestampToTimespan(ResetRequests); }

        /// <summary>
        /// The time until the rate limit (based on tokens) resets to its initial state.
        /// </summary>
        [JsonIgnore]
        public string ResetTokens { get; internal set; }

        /// <summary>
        /// The time until the rate limit (based on tokens) resets to its initial state represented as a TimeSpan.
        /// </summary>
        [JsonIgnore]
        public TimeSpan ResetTokensTimespan { get => ConvertTimestampToTimespan(ResetTokens); }

        /// <summary>
        /// Takes a timestamp recieved from a OpenAI response header and converts to a TimeSpan
        /// </summary>
        /// <param name="timestamp">The timestamp received from an OpenAI header, eg x-ratelimit-reset-tokens</param>
        /// <returns>A TimeSpan that represents the timestamp provided</returns>
        /// <exception cref="ArgumentException">Thrown if the provided timestamp is not in the expected format, or if the match is not successful.</exception>
        private TimeSpan ConvertTimestampToTimespan(string timestamp)
        {
            /*
             * Regex Notes: 
             *  The gist of this regex is that it is searching for "timestamp segments", eg 1m or 144ms.
             *  Each segment gets matched into its respective named capture group, from which we further parse out the 
             *  digits. This allows us to take the string 6m45s99ms and insert the integers into a 
             *  TimeSpan object for easier use.
             *  
             *  Regex Performance Notes, against 100k randomly generated timestamps:
             *  Average performance: 0.0001ms
             *  Best case: 0ms
             *  Worst Case: 8ms
             *  Total Time: 10ms
             *  
             *  Inconsequential compute time
             */
            Regex tsRegex = new Regex(@"^(?<hour>\d+h)?(?<mins>\d+m(?!s))?(?<secs>\d+s)?(?<ms>\d+ms)?");
            Match match = tsRegex.Match(timestamp);
            if (!match.Success)
            {
                throw new ArgumentException($"Could not parse timestamp header. '{timestamp}'.");
            }

            /*
             * Note about Hours in timestamps:
             *  I have not personally observed a timestamp with an hours segment (eg. 1h30m15s1ms).
             *  Although their presense may not actually exist, we can still have this section in the parser, there is no
             *  negative impact for a missing hours segment because the capture groups are flagged as optional.
             */
            int hours = 0;
            if (match.Groups["hour"].Captures.Count > 0)
            {
                hours = int.Parse(match.Groups["hour"].Value.Replace("h", string.Empty));
            }

            int minutes = 0;
            if (match.Groups["mins"].Captures.Count > 0)
            {
                minutes = int.Parse(match.Groups["mins"].Value.Replace("m", string.Empty));
            }

            int seconds = 0;
            if (match.Groups["secs"].Captures.Count > 0)
            {
                seconds = int.Parse(match.Groups["secs"].Value.Replace("s", string.Empty));
            }

            int ms = 0;
            if (match.Groups["ms"].Captures.Count > 0)
            {
                ms = int.Parse(match.Groups["ms"].Value.Replace("ms", string.Empty));
            }

            return new TimeSpan(hours, minutes, seconds) + TimeSpan.FromMilliseconds(ms);
        }

        public string ToJsonString()
            => this.ToEscapedJsonString<object>();
    }
}
