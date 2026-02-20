using Cleo.Cli.Models;
using Cleo.Core.UseCases.RefreshPulse;

namespace Cleo.Cli.Services;

/// <summary>
/// Defines the contract for evaluating session status and PR outcomes.
/// </summary>
internal interface ISessionStatusEvaluator
{
    StatusViewModel Evaluate(RefreshPulseResponse response);
}
