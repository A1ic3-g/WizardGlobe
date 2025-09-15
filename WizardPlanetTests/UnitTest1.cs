using WizardPlanet.Services;

namespace WizardPlanetTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test1()
    {
        var globeGenerator = new GlobeGenerationService();
        await globeGenerator.CreateGlobeRotationSequenceAsync("green_grid.png",
            "green_grid_anim", 
            360);
        Assert.Pass();
    }
}