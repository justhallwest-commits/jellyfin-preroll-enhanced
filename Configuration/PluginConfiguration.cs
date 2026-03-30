using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PreRollEnhanced.Configuration
{
    /// <summary>
    /// Plugin configuration with metadata-based selection rules
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// ID of the library containing pre-roll videos
        /// </summary>
        public string? PreRollLibraryId { get; set; }

        /// <summary>
        /// Enable pre-rolls for movies
        /// </summary>
        public bool EnableForMovies { get; set; } = true;

        /// <summary>
        /// Enable pre-rolls for TV shows
        /// </summary>
        public bool EnableForTvShows { get; set; } = true;

        /// <summary>
        /// Enable pre-rolls for TV episodes
        /// </summary>
        public bool EnableForEpisodes { get; set; } = true;

        /// <summary>
        /// Metadata Selection Rules
        /// </summary>
        public SelectionRules Rules { get; set; } = new SelectionRules();

        /// <summary>
        /// Seasonal tag definitions for time-based pre-roll selection
        /// </summary>
        public List<SeasonalTag> SeasonalTags { get; set; } = new List<SeasonalTag>();

        /// <summary>
        /// Ignore pre-rolls with seasonal tags that are out of season
        /// </summary>
        public bool IgnoreOutOfSeason { get; set; } = true;

        /// <summary>
        /// Enforce that pre-roll rating does not exceed content rating
        /// </summary>
        public bool EnforceRatingLimit { get; set; } = true;

        /// <summary>
        /// Minimum seconds between pre-roll triggers for the same item (loop prevention)
        /// </summary>
        public int LoopPreventionSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Metadata-based selection rules for pre-rolls
    /// </summary>
    public class SelectionRules
    {
        /// <summary>
        /// Match pre-rolls with the same name tag as the content
        /// </summary>
        public bool MatchName { get; set; } = false;

        /// <summary>
        /// Match pre-rolls with the same year tag
        /// </summary>
        public bool MatchYear { get; set; } = false;

        /// <summary>
        /// Match pre-rolls with the same decade tag (e.g., "1980s")
        /// </summary>
        public bool MatchDecade { get; set; } = false;

        /// <summary>
        /// Match pre-rolls with seasonal tags currently in season
        /// </summary>
        public bool MatchSeasonal { get; set; } = false;

        /// <summary>
        /// Match pre-rolls with overlapping genres
        /// </summary>
        public bool MatchGenre { get; set; } = false;

        /// <summary>
        /// Match pre-rolls with overlapping studios
        /// </summary>
        public bool MatchStudios { get; set; } = false;

        /// <summary>
        /// Require ALL tags to match (AND logic) instead of ANY (OR logic)
        /// </summary>
        public bool RequireAllTags { get; set; } = false;
    }

    /// <summary>
    /// Defines a seasonal time period for pre-roll selection
    /// </summary>
    public class SeasonalTag
    {
        /// <summary>
        /// Tag name (e.g., "christmas", "halloween")
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// Start date (month/day) - year is ignored
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date (month/day) - year is ignored
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Check if this seasonal tag is currently active
        /// </summary>
        public bool IsInSeason()
        {
            var now = DateTime.Now;
            var startDay = new DateTime(now.Year, StartDate.Month, StartDate.Day);
            var endDay = new DateTime(now.Year, EndDate.Month, EndDate.Day);

            // Handle year wrap-around (e.g., Dec 20 - Jan 5)
            if (endDay < startDay)
            {
                return now >= startDay || now <= endDay;
            }

            return now >= startDay && now <= endDay;
        }
    }
}
