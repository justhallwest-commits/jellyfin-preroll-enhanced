using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.PreRollEnhanced.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PreRollEnhanced
{
    /// <summary>
    /// Manages pre-roll playback using session interception for universal client support
    /// </summary>
    public class PreRollManager : IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<PreRollManager> _logger;
        
        // State tracking for loop prevention
        private readonly Dictionary<string, DateTime> _lastPlayedItems = new Dictionary<string, DateTime>();
        private readonly object _lockObject = new object();

        public PreRollManager(
            ISessionManager sessionManager,
            ILibraryManager libraryManager,
            ILogger<PreRollManager> logger)
        {
            _sessionManager = sessionManager;
            _libraryManager = libraryManager;
            _logger = logger;

            // Subscribe to playback start events
            _sessionManager.PlaybackStart += OnPlaybackStart;
        }

        /// <summary>
        /// Handle playback start events and inject pre-rolls
        /// </summary>
        private async void OnPlaybackStart(object? sender, PlaybackProgressEventArgs e)
        {
            try
            {
                if (Plugin.Instance == null)
                {
                    return;
                }

                var config = Plugin.Instance.Configuration;
                
                // Check if pre-rolls are enabled for this content type
                if (!ShouldPlayPreRoll(e.Item, config))
                {
                    return;
                }

                // Loop prevention check
                if (IsRecentlyPlayed(e.Item, config.LoopPreventionSeconds))
                {
                    _logger.LogDebug("Pre-roll skipped for {ItemName} - recently played within {Seconds}s", 
                        e.Item.Name, config.LoopPreventionSeconds);
                    return;
                }

                // Get pre-roll video based on metadata selection rules
                var preRollVideo = await GetPreRollVideoAsync(e.Item, config);

                if (preRollVideo == null)
                {
                    _logger.LogDebug("No suitable pre-roll found for {ItemName}", e.Item.Name);
                    return;
                }

                // Record this playback to prevent loops
                RecordPlayback(e.Item);

                // Inject pre-roll via session manager
                await InjectPreRollAsync(e.Session, preRollVideo, e.Item);
                
                _logger.LogInformation("Playing pre-roll '{PreRollName}' before '{ItemName}' for user {User}", 
                    preRollVideo.Name, e.Item.Name, e.Users.FirstOrDefault()?.Username ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error injecting pre-roll");
            }
        }

        /// <summary>
        /// Determine if pre-rolls should play for this content type
        /// </summary>
        private bool ShouldPlayPreRoll(BaseItem item, PluginConfiguration config)
        {
            return item switch
            {
                Movie => config.EnableForMovies,
                Series => config.EnableForTvShows,
                Episode => config.EnableForEpisodes,
                _ => false
            };
        }

        /// <summary>
        /// Check if item was recently played (loop prevention)
        /// </summary>
        private bool IsRecentlyPlayed(BaseItem item, int preventionSeconds)
        {
            lock (_lockObject)
            {
                var key = item.Id.ToString();
                if (_lastPlayedItems.TryGetValue(key, out var lastPlayed))
                {
                    var elapsed = (DateTime.UtcNow - lastPlayed).TotalSeconds;
                    return elapsed < preventionSeconds;
                }
                return false;
            }
        }

        /// <summary>
        /// Record that this item has been played
        /// </summary>
        private void RecordPlayback(BaseItem item)
        {
            lock (_lockObject)
            {
                var key = item.Id.ToString();
                _lastPlayedItems[key] = DateTime.UtcNow;

                // Cleanup old entries (keep last hour only)
                var cutoff = DateTime.UtcNow.AddHours(-1);
                var keysToRemove = _lastPlayedItems
                    .Where(kvp => kvp.Value < cutoff)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var k in keysToRemove)
                {
                    _lastPlayedItems.Remove(k);
                }
            }
        }

        /// <summary>
        /// Get a pre-roll video using metadata-based selection rules
        /// </summary>
        private async Task<Video?> GetPreRollVideoAsync(BaseItem contentItem, PluginConfiguration config)
        {
            if (string.IsNullOrEmpty(config.PreRollLibraryId))
            {
                return null;
            }

            // Get all videos from pre-roll library
            var preRollLibrary = _libraryManager.GetItemById(new Guid(config.PreRollLibraryId));
            if (preRollLibrary == null)
            {
                return null;
            }

            var query = new InternalItemsQuery
            {
                Parent = preRollLibrary,
                IncludeItemTypes = new[] { nameof(Video), nameof(Movie) },
                Recursive = true
            };

            var allPreRolls = _libraryManager.GetItemList(query)
                .OfType<Video>()
                .ToList();

            if (allPreRolls.Count == 0)
            {
                return null;
            }

            // Apply filters
            var candidates = allPreRolls;

            // Filter out seasonal pre-rolls that are out of season
            if (config.IgnoreOutOfSeason && config.SeasonalTags.Any())
            {
                candidates = FilterSeasonalOutOfSeason(candidates, config.SeasonalTags);
            }

            // Filter by rating if enabled
            if (config.EnforceRatingLimit && !string.IsNullOrEmpty(contentItem.OfficialRating))
            {
                candidates = FilterByRating(candidates, contentItem.OfficialRating);
            }

            // Apply metadata selection rules
            var matches = ApplySelectionRules(candidates, contentItem, config);

            // Return random match or fallback to random from all candidates
            return matches.Any() 
                ? matches.OrderBy(_ => Guid.NewGuid()).FirstOrDefault()
                : candidates.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        }

        /// <summary>
        /// Filter out pre-rolls with seasonal tags that are currently out of season
        /// </summary>
        private List<Video> FilterSeasonalOutOfSeason(List<Video> preRolls, List<SeasonalTag> seasonalTags)
        {
            var seasonalTagNames = seasonalTags.Select(st => st.Tag.ToLowerInvariant()).ToHashSet();
            var activeSeasonalTags = seasonalTags
                .Where(st => st.IsInSeason())
                .Select(st => st.Tag.ToLowerInvariant())
                .ToHashSet();

            return preRolls.Where(pr =>
            {
                var tags = pr.Tags?.Select(t => t.ToLowerInvariant()).ToList() ?? new List<string>();
                var hasSeasonalTag = tags.Any(t => seasonalTagNames.Contains(t));

                // If it has a seasonal tag, only include if that tag is in season
                // If it has no seasonal tag, always include
                return !hasSeasonalTag || tags.Any(t => activeSeasonalTags.Contains(t));
            }).ToList();
        }

        /// <summary>
        /// Filter pre-rolls by rating to not exceed content rating
        /// </summary>
        private List<Video> FilterByRating(List<Video> preRolls, string contentRating)
        {
            // Rating hierarchy (simplified US ratings)
            var ratingHierarchy = new Dictionary<string, int>
            {
                { "G", 1 },
                { "PG", 2 },
                { "PG-13", 3 },
                { "TV-14", 3 },
                { "R", 4 },
                { "TV-MA", 4 },
                { "NC-17", 5 }
            };

            if (!ratingHierarchy.TryGetValue(contentRating, out var maxRatingValue))
            {
                maxRatingValue = 5; // Unknown ratings allow all
            }

            return preRolls.Where(pr =>
            {
                // Empty or unrated pre-rolls are considered suitable for all
                if (string.IsNullOrEmpty(pr.OfficialRating) || 
                    pr.OfficialRating.Equals("Unrated", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return ratingHierarchy.TryGetValue(pr.OfficialRating, out var preRollRating) 
                    && preRollRating <= maxRatingValue;
            }).ToList();
        }

        /// <summary>
        /// Apply metadata-based selection rules to find matching pre-rolls
        /// </summary>
        private List<Video> ApplySelectionRules(List<Video> candidates, BaseItem contentItem, PluginConfiguration config)
        {
            var rules = config.Rules;
            var matches = new List<Video>();

            // If no rules are enabled, return all candidates
            if (!rules.MatchName && !rules.MatchYear && !rules.MatchDecade && 
                !rules.MatchSeasonal && !rules.MatchGenre && !rules.MatchStudios)
            {
                return candidates;
            }

            foreach (var candidate in candidates)
            {
                var candidateTags = candidate.Tags?.Select(t => t.ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
                var ruleMatches = new List<bool>();

                // Name match (tag-based)
                if (rules.MatchName)
                {
                    var nameTag = contentItem.Name.ToLowerInvariant();
                    ruleMatches.Add(candidateTags.Contains(nameTag));
                }

                // Year match (tag-based)
                if (rules.MatchYear && contentItem.ProductionYear.HasValue)
                {
                    var yearTag = contentItem.ProductionYear.Value.ToString();
                    ruleMatches.Add(candidateTags.Contains(yearTag.ToLowerInvariant()));
                }

                // Decade match (tag-based - e.g., "1980s")
                if (rules.MatchDecade && contentItem.ProductionYear.HasValue)
                {
                    var decade = (contentItem.ProductionYear.Value / 10) * 10;
                    var decadeTag = $"{decade}s".ToLowerInvariant();
                    ruleMatches.Add(candidateTags.Contains(decadeTag));
                }

                // Seasonal match
                if (rules.MatchSeasonal)
                {
                    var activeSeasonalTags = config.SeasonalTags
                        .Where(st => st.IsInSeason())
                        .Select(st => st.Tag.ToLowerInvariant())
                        .ToHashSet();
                    ruleMatches.Add(candidateTags.Any(t => activeSeasonalTags.Contains(t)));
                }

                // Genre match
                if (rules.MatchGenre)
                {
                    var contentGenres = contentItem.Genres?.Select(g => g.ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
                    var candidateGenres = candidate.Genres?.Select(g => g.ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
                    ruleMatches.Add(contentGenres.Overlaps(candidateGenres));
                }

                // Studios match
                if (rules.MatchStudios)
                {
                    var contentStudios = contentItem.Studios?.Select(s => s.ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
                    var candidateStudios = candidate.Studios?.Select(s => s.ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
                    ruleMatches.Add(contentStudios.Overlaps(candidateStudios));
                }

                // Determine if this candidate matches based on AND/OR logic
                bool isMatch = rules.RequireAllTags 
                    ? ruleMatches.All(m => m)  // ALL must match
                    : ruleMatches.Any(m => m);  // ANY must match

                if (isMatch && ruleMatches.Any())
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        /// <summary>
        /// Inject pre-roll into playback session using server-side command
        /// </summary>
        private async Task InjectPreRollAsync(SessionInfo session, Video preRollVideo, BaseItem originalItem)
        {
            try
            {
                // Build play command with pre-roll followed by original content
                var playRequest = new PlayRequest
                {
                    ItemIds = new[] { preRollVideo.Id, originalItem.Id },
                    PlayCommand = PlayCommand.PlayNow,
                    StartPositionTicks = 0
                };

                // Send play command to client via session manager
                await _sessionManager.SendPlayCommand(
                    session.Id,
                    session.Id,
                    playRequest,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to inject pre-roll for session {SessionId}", session.Id);
            }
        }

        public void Dispose()
        {
            _sessionManager.PlaybackStart -= OnPlaybackStart;
        }
    }
}
