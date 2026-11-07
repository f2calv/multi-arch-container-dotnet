using Microsoft.Extensions.Options;

namespace CasCap.Models;

/// <summary>Validates application configuration without runtime reflection.</summary>
[OptionsValidator]
internal sealed partial class AppConfigValidator : IValidateOptions<AppConfig>
{
}
