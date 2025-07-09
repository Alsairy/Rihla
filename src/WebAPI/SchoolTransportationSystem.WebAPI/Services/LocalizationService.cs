using Microsoft.Extensions.Localization;
using Rihla.WebAPI.Resources;

namespace Rihla.WebAPI.Services
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key);
        string GetLocalizedString(string key, params object[] arguments);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public LocalizationService(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
        }

        public string GetLocalizedString(string key)
        {
            return _localizer[key];
        }

        public string GetLocalizedString(string key, params object[] arguments)
        {
            return _localizer[key, arguments];
        }
    }
}
