using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Worms.Hub.Gateway;

/// <summary>
/// Overloads the default detection of controllers in ASP.NET Core to include controllers marked as internal
/// rather than just public.
/// </summary>
internal sealed class InternalControllerProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo) => typeInfo.IsDefined(typeof(ApiControllerAttribute));
}
