# Quick Installation Guide

## For Your Unraid Jellyfin Server

### Option 1: Build and Install Manually (Recommended for Testing)

1. **Copy the entire `jellyfin-preroll-enhanced` folder to your Unraid server**
   ```bash
   # From your local machine
   scp -r jellyfin-preroll-enhanced/ root@YOUR_UNRAID_IP:/mnt/user/appdata/
   ```

2. **SSH into your Unraid server**
   ```bash
   ssh root@YOUR_UNRAID_IP
   ```

3. **Build the plugin**
   ```bash
   cd /mnt/user/appdata/jellyfin-preroll-enhanced
   
   # Make build script executable
   chmod +x build.sh
   
   # Build (requires .NET 9 SDK installed)
   ./build.sh 1.0.0
   ```

4. **Install to Jellyfin**
   ```bash
   # Create plugin directory
   mkdir -p /mnt/user/appdata/jellyfin/plugins/PreRollEnhanced
   
   # Extract the built plugin
   unzip -o release/preroll-enhanced_1.0.0.zip -d /mnt/user/appdata/jellyfin/plugins/PreRollEnhanced/
   ```

5. **Restart Jellyfin**
   - Go to Unraid web UI
   - Docker tab → Jellyfin → Stop → Start

### Option 2: Direct File Copy (If .NET 9 SDK Not Available)

If you can't install .NET 9 SDK on Unraid:

1. **Build on your local machine** (Windows/Mac/Linux with .NET 9 SDK)
   ```bash
   cd jellyfin-preroll-enhanced
   dotnet publish Jellyfin.Plugin.PreRollEnhanced.csproj --configuration Release --output bin
   ```

2. **Copy built files to Unraid**
   ```bash
   scp -r bin/* root@YOUR_UNRAID_IP:/mnt/user/appdata/jellyfin/plugins/PreRollEnhanced/
   ```

3. **Restart Jellyfin**

### Option 3: GitHub Repository (For Production Use)

1. **Push to GitHub** (after customizing)
   ```bash
   cd jellyfin-preroll-enhanced
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin https://github.com/YOUR_USERNAME/jellyfin-preroll-enhanced.git
   git push -u origin main
   ```

2. **Create a release tag**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **GitHub Actions will automatically:**
   - Build the plugin
   - Create a release with ZIP file
   - Update the manifest.json
   - Make it installable via Jellyfin plugin catalog

4. **In Jellyfin:**
   - Dashboard → Plugins → Repositories
   - Add: `https://raw.githubusercontent.com/YOUR_USERNAME/jellyfin-preroll-enhanced/main/manifest.json`
   - Install from catalog

## Verify Installation

1. Go to **Dashboard → Plugins**
2. You should see **Pre-Roll Videos Enhanced v1.0.0**
3. Click it to configure

## Initial Configuration

1. **Create a pre-roll library:**
   - Dashboard → Libraries → Add Library
   - Type: **Movies**
   - Folder: `/mnt/user/media/PreRolls/` (or wherever you store them)

2. **Configure plugin:**
   - Dashboard → Plugins → Pre-Roll Videos Enhanced
   - Select your pre-roll library
   - Enable for Movies/TV Shows/Episodes
   - Save and restart

3. **Test it:**
   - Play a movie or TV show on your Fire TV
   - Pre-roll should play first!

## Troubleshooting

### Plugin doesn't appear after install
- Check file permissions: `chown -R jellyfin:jellyfin /mnt/user/appdata/jellyfin/plugins/`
- Check Jellyfin logs: Dashboard → Logs

### Build fails
- Ensure .NET 9 SDK is installed: `dotnet --version`
- If not, install: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash`

### Library dropdown empty
- Wait 10 seconds for polling to populate
- Hard refresh page (Ctrl+Shift+R)
- This is a known Jellyfin 10.11 React dashboard issue

## Next Steps

See [README.md](README.md) for:
- Complete feature documentation
- Metadata selection rules guide
- Seasonal tags setup
- Best practices
