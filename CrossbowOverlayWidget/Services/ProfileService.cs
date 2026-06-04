using System;
using System.Linq;
using CrossbowOverlayWidget.Models;

namespace CrossbowOverlayWidget.Services
{
    public class ProfileService
    {
        private AppConfig _config;

        public event EventHandler<GameProfile> ProfileChanged;

        public void Initialize(AppConfig config)
        {
            _config = config;
        }

        public GameProfile GetActive()
        {
            return _config?.Profiles?.FirstOrDefault(p => p.Id == _config.ActiveProfileId)
                ?? _config?.Profiles?.FirstOrDefault();
        }

        public void CreateProfile(string name, string exe)
        {
            var profile = new GameProfile
            {
                Name = name,
                GameExecutable = exe
            };
            _config.Profiles.Add(profile);
        }

        public void SwitchProfile(string profileId)
        {
            _config.ActiveProfileId = profileId;
            var profile = GetActive();
            if (profile != null)
            {
                profile.LastUsed = DateTime.UtcNow;
                ProfileChanged?.Invoke(this, profile);
            }
        }

        public void DeleteProfile(string profileId)
        {
            _config.Profiles.RemoveAll(p => p.Id == profileId);
            if (_config.ActiveProfileId == profileId && _config.Profiles.Count > 0)
                _config.ActiveProfileId = _config.Profiles[0].Id;
        }
    }
}
