using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Ahp;

namespace IoTRecommendation.Web.ViewModels;

public sealed class AhpViewModel
{
    public AhpResult Result { get; set; } = null!;
    public List<CriterionDefinition> CriteriaDefinitions { get; set; } = new();
}
