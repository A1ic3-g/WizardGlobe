using System.Threading.Tasks;

namespace WizardPlanet.Services;

public interface IGlobeGenerationService
{
    Task CreateGlobeRotationSequenceAsync(string sourceImagePath, string outputFolder, int numFrames = 360);
}