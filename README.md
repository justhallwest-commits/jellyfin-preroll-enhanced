# Pre-Roll Videos Enhanced Plugin for Jellyfin

![Jellyfin Logo](https://jellyfin.org/images/logo.svg)

## 🎬 Universal Pre-Roll Plugin for Movies & TV Shows

**The ONLY Jellyfin pre-roll plugin that works on ALL clients including Fire TV, Roku, mobile apps, and web browsers.**

---

## ✨ Key Features

### Universal Client Support
- **Works on EVERY Jellyfin client** - Fire TV, Roku, iOS, Android, Web, Jellyfin Media Player, etc.
- Uses **server-side session interception** instead of client-dependent intro providers
- No client-specific hacks or workarounds needed

### Complete Content Coverage
- ✅ Movies
- ✅ TV Shows (series level)
- ✅ TV Episodes (individual episodes)

### Intelligent Metadata Selection
Choose pre-rolls based on rich metadata matching:
- **Name matching** - Play specific pre-rolls for specific titles
- **Year matching** - Match pre-rolls by release year (e.g., "1985")
- **Decade matching** - Match by decade (e.g., "1980s", "1990s")
- **Genre matching** - Overlap genres between content and pre-rolls
- **Studio matching** - Match by production studio
- **Seasonal matching** - Time-based pre-rolls (Christmas, Halloween, etc.)
- **AND/OR logic** - Require ALL rules to match or just ANY

### Smart Features
- **Rating enforcement** - Don't show R-rated pre-rolls before PG content
- **Seasonal filtering** - Automatically exclude out-of-season content
- **Loop prevention** - Prevent pre-rolls from triggering multiple times
- **Random fallback** - If no rules match, play a random pre-roll

---

## 🆚 Why This Plugin vs. Cinema Mode?

| Feature | Cinema Mode | Pre-Roll Enhanced |
|---------|-------------|-------------------|
| **Universal Client Support** | ❌ Only works on web/JMP | ✅ Works on ALL clients |
| **Fire TV Support** | ❌ No | ✅ Yes |
| **Roku Support** | ❌ No (Issue #39) | ✅ Yes |
| **TV Show Support** | ❌ Movies only | ✅ Full TV support |
| **Implementation** | Client-dependent IIntroProvider | Server-side session management |
| **Metadata Rules** | ✅ Yes | ✅ Yes (same features) |
| **Seasonal Tags** | ✅ Yes | ✅ Yes |

**Cinema Mode uses `IIntroProvider`**, which means:
- Only clients that call Jellyfin's `/Intros` API will see pre-rolls
- Fire TV, Roku, and many mobile apps **don't call this API**
- Your pre-rolls simply won't play on those clients

**Pre-Roll Enhanced uses session interception**, which means:
- The server intercepts playback start events
- The server commands the client to play the pre-roll
- Works regardless of client implementation

---

## 📦 Installation

### Method 1: Plugin Repository (Recommended)

1. In Jellyfin Dashboard, go to **Plugins → Repositories**
2. Add this repository URL:
   ```
   https://raw.githubusercontent.com/justhallwest-commits/jellyfin-preroll-enhanced/main/manifest.json
   ```
3. Go to **Catalog**, find **Pre-Roll Videos Enhanced**
4. Click **Install**
5. Restart Jellyfin

### Method 2: Manual Installation

1. Download the latest release ZIP from [GitHub Releases](https://github.com/justhallwest-commits/jellyfin-preroll-enhanced/releases)
2. Extract to your Jellyfin plugins directory:
   - **Linux**: `/var/lib/jellyfin/plugins/PreRollEnhanced/`
   - **Windows**: `%AppData%\Jellyfin\Server\plugins\PreRollEnhanced\`
   - **Unraid**: `/mnt/user/appdata/jellyfin/plugins/PreRollEnhanced/`
3. Restart Jellyfin

---

## ⚙️ Configuration

### Step 1: Create a Pre-Roll Library

1. Create a new folder for your pre-roll videos (e.g., `/mnt/media/PreRolls/`)
2. In Jellyfin, add a **new library** with type **"Movies"**
3. Point it to your pre-roll folder
4. Let Jellyfin scan and add metadata

**Why "Movies" type?** Jellyfin doesn't have a "Pre-Rolls" content type, so we use Movies to get full metadata support (tags, genres, studios, ratings).

### Step 2: Configure the Plugin

1. Go to **Dashboard → Plugins → Pre-Roll Videos Enhanced**
2. Select your pre-roll library from the dropdown
3. Enable pre-rolls for Movies, TV Shows, and/or Episodes
4. Configure metadata selection rules (see below)
5. Save and restart Jellyfin

### Step 3: Add Metadata to Pre-Rolls

To use metadata selection rules, add tags and metadata to your pre-roll videos:

#### Example: Year-Based Pre-Roll
1. Edit a pre-roll video
2. Add tag: `1985`
3. This pre-roll will play before 1985 movies when "Match Year" is enabled

#### Example: Decade-Based Pre-Roll
1. Edit a pre-roll video
2. Add tag: `1980s`
3. This pre-roll will play before any 1980s content when "Match Decade" is enabled

#### Example: Seasonal Pre-Roll
1. Edit a pre-roll video
2. Add tag: `christmas`
3. Configure seasonal tag in plugin:
   - Tag: `christmas`
   - Start Date: December 1
   - End Date: December 25
4. This pre-roll will only play during that date range

#### Example: Genre-Based Pre-Roll
1. Edit a pre-roll video
2. Add genres: `Action`, `Sci-Fi`
3. This pre-roll will play before Action or Sci-Fi content when "Match Genre" is enabled

---

## 🎯 Metadata Selection Rules

### How Rules Work

1. **If NO rules are enabled** → Plugin plays a random pre-roll
2. **If rules ARE enabled** → Plugin searches for matches
   - With **OR logic** (default): Plays pre-rolls matching ANY rule
   - With **AND logic**: Plays pre-rolls matching ALL rules
3. **If NO matches found** → Falls back to random pre-roll

### Available Rules

| Rule | Matches On | Example |
|------|-----------|---------|
| **Match Name** | Pre-roll tag = content name | Tag: "Back to the Future" |
| **Match Year** | Pre-roll tag = release year | Tag: "1985" |
| **Match Decade** | Pre-roll tag = decade | Tag: "1980s" |
| **Match Seasonal** | Pre-roll has active seasonal tag | Tag: "christmas" (Dec 1-25) |
| **Match Genre** | Overlapping genres | Both have "Action" |
| **Match Studios** | Overlapping studios | Both have "Universal Pictures" |

### Example Configurations

#### Configuration 1: Seasonal + Decade
```
✓ Match Decade
✓ Match Seasonal
○ Require ALL Tags (OR logic)
```
**Result**: Plays pre-rolls from the same decade OR with active seasonal tags

#### Configuration 2: Genre + Year (Strict)
```
✓ Match Genre
✓ Match Year
✓ Require ALL Tags (AND logic)
```
**Result**: Plays pre-rolls that match BOTH genre AND year

#### Configuration 3: Random Only
```
○ All rules disabled
```
**Result**: Always plays a random pre-roll

---

## 🎃 Seasonal Tags

### What Are Seasonal Tags?

Seasonal tags let you play specific pre-rolls during certain times of the year (holidays, seasons, events).

### Example Setup

```
Tag: christmas
Start Date: December 1
End Date: December 25

Tag: halloween
Start Date: October 20
End Date: October 31

Tag: summer
Start Date: June 1
End Date: August 31
```

### How It Works

1. Add seasonal tag names to your pre-roll videos (e.g., `christmas`, `halloween`)
2. Define the date ranges in plugin configuration
3. Enable "Match Seasonal" rule
4. Pre-rolls with active seasonal tags will be prioritized
5. Enable "Ignore Out of Season" to completely exclude off-season pre-rolls

---

## 🔐 Rating Enforcement

When **Enforce Rating Limit** is enabled:
- Pre-roll ratings cannot exceed content ratings
- Example: R-rated pre-rolls won't play before PG-13 movies
- Unrated pre-rolls are considered safe for all audiences

**Rating Hierarchy** (US ratings):
```
G < PG < PG-13 / TV-14 < R / TV-MA < NC-17
```

---

## 🔄 Loop Prevention

**Problem**: Without loop prevention, pre-rolls can trigger multiple times:
- User plays a movie → Pre-roll plays → Movie plays
- User seeks to beginning → Pre-roll plays AGAIN
- Fire TV backs out and resumes → Pre-roll plays AGAIN

**Solution**: Loop prevention tracks recently played items and skips pre-rolls within the configured time window.

**Default**: 30 seconds
**Recommended**: 30-60 seconds

---

## 🏗️ Technical Architecture

### How It Works Internally

```
User presses Play
    ↓
SessionManager.PlaybackStart event fires
    ↓
PreRollManager checks:
    - Is content type enabled? (Movies/TV/Episodes)
    - Loop prevention OK?
    - Find matching pre-roll based on metadata rules
    ↓
PreRollManager builds PlayRequest:
    ItemIds = [PreRollVideoId, OriginalContentId]
    ↓
SessionManager.SendPlayCommand to client
    ↓
Client receives server command and plays:
    1. Pre-roll video
    2. Original content
```

### Why This Works on All Clients

**Client-side approach (Cinema Mode)**:
```
Client → "Do you have intros for this?" → Server
Server → "Here are the intros" → Client
Client → Decides whether to play them (many don't)
```

**Server-side approach (Pre-Roll Enhanced)**:
```
Server detects playback → "Play THESE items in THIS order" → Client
Client → Must obey server command (standard Jellyfin protocol)
```

---

## 🐛 Troubleshooting

### Pre-rolls aren't playing

**Check:**
1. Is a pre-roll library selected in config?
2. Does the library contain video files?
3. Is the content type enabled? (Movies/TV Shows/Episodes)
4. Check Jellyfin logs: Dashboard → Logs → Filter for "PreRollEnhanced"

### Pre-rolls play multiple times

**Solution:**
1. Increase Loop Prevention value (try 60 seconds)
2. Check for conflicting plugins

### Specific pre-rolls never play

**Check:**
1. Do they have matching metadata/tags?
2. Are selection rules configured correctly?
3. If seasonal, is the current date within range?
4. If rating enforcement enabled, does rating match?

### Config page shows empty library dropdown

**This is the known Jellyfin 10.11 bug** from your existing plugin. Try these fixes:
1. Hard refresh the page (Ctrl+Shift+R / Cmd+Shift+R)
2. Clear browser cache
3. Wait 5-10 seconds for polling to populate dropdown
4. Check browser console for errors

---

## 📊 Comparison with Existing Solutions

### vs. Your Original Pre-Roll Videos Plugin
- ✅ **Same universal client approach** (session interception)
- ✅ **Adds rich metadata selection** (year, decade, genre, studios, seasonal)
- ✅ **Adds rating enforcement**
- ✅ **Better organized code and config UI**

### vs. Cinema Mode Plugin
- ✅ **Universal client support** (Cinema Mode is web/JMP only)
- ✅ **TV show support** (Cinema Mode is movies only)
- ✅ **Same metadata features** (we copied the good parts)

---

## 📝 Pre-Roll Library Organization Tips

### Recommended Structure

```
/mnt/media/PreRolls/
├── Seasonal/
│   ├── Christmas Lights.mp4           [Tag: christmas]
│   ├── Halloween Intro.mp4            [Tag: halloween]
│   └── Summer Blockbuster.mp4         [Tag: summer]
├── Decades/
│   ├── 80s Retro Intro.mp4           [Tag: 1980s]
│   ├── 90s VHS Tracking.mp4          [Tag: 1990s]
│   └── 2000s Widescreen.mp4          [Tag: 2000s]
├── Genres/
│   ├── Action Trailer.mp4             [Genre: Action]
│   ├── Horror Warning.mp4             [Genre: Horror]
│   └── Comedy Bumper.mp4              [Genre: Comedy]
└── Generic/
    ├── Feature Presentation 1.mp4     [No tags]
    ├── Feature Presentation 2.mp4     [No tags]
    └── Now Playing.mp4                [No tags]
```

### Metadata Best Practices

1. **Use consistent tag naming** - All lowercase, no spaces
   - ✅ `1980s`, `christmas`, `summer`
   - ❌ `1980's`, `Christmas`, `Summer Time`

2. **Combine tags for specificity**
   - Example: Tag a pre-roll with both `1980s` AND `action`
   - Plays when: Decade OR Genre matches

3. **Have generic fallbacks**
   - Always keep some pre-rolls with NO tags
   - These play when rules don't match

4. **Test your rules**
   - Play content from different years/genres
   - Verify correct pre-rolls are selected

---

## 🚀 Future Enhancements

Planned features for future releases:
- [ ] Multiple pre-rolls per playback (trailer bumper + feature presentation)
- [ ] User-level preferences (per-user enable/disable)
- [ ] Recently played pre-roll tracking (avoid repeats)
- [ ] Library-specific rules (different rules for different libraries)
- [ ] Integration with existing Jellyfin Cinema Mode setting
- [ ] Pre-roll statistics and analytics

---

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test on Jellyfin 10.11+
5. Submit a pull request

---

## 📄 License

MIT License - see LICENSE file for details

---

## 🙏 Credits

- **Cinema Mode Plugin** by CherryFloors - Inspiration for metadata selection rules
- **Jellyfin Team** - For the excellent media server platform
- **Community** - For testing and feedback

---

## 💬 Support

- **GitHub Issues**: [Report bugs or request features](https://github.com/justhallwest-commits/jellyfin-preroll-enhanced/issues)
- **Jellyfin Forum**: [Join the discussion](https://forum.jellyfin.org)
- **Discord**: Jellyfin Discord server (#plugins channel)

---

## 📌 Version History

### v1.0.0 (2026-03-28)
- Initial release
- Universal client support via session interception
- Support for Movies, TV Shows, and Episodes
- Metadata selection rules (Name, Year, Decade, Genre, Studios)
- Seasonal tag system
- Rating enforcement
- Loop prevention
- Jellyfin 10.11+ compatibility

---

**Built with ❤️ for the Jellyfin community**
