using System;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;

namespace WizardPlanet.Services;

public class GlobeGenerationService : IGlobeGenerationService
{
    /// <summary>
    /// Creates a sequence of frames depicting a rotating globe from a flat map image.
    /// This implementation uses parallel processing for both the initial map calculation
    /// and the final frame generation for maximum performance.
    /// </summary>
    /// <param name="sourceImagePath">The file path of the source equirectangular map image.</param>
    /// <param name="outputFolder">The folder where the output frames will be saved.</param>
    /// <param name="numFrames">The total number of frames to generate for a full 360-degree rotation.</param>
    public static void CreateGlobeRotationSequence(string sourceImagePath, string outputFolder, int numFrames = 360)
    {
        // 1. Setup and Image Loading
        Directory.CreateDirectory(outputFolder);

        using Mat sourceImage = Cv2.ImRead(sourceImagePath);
        if (sourceImage.Empty())
        {
            Console.WriteLine("Error: Could not read the source image.");
            return;
        }

        var hSrc = sourceImage.Rows;
        var wSrc = sourceImage.Cols;
        int hDst = wSrc, wDst = wSrc;
        var centerXDst = wDst / 2;
        var centerYDst = hDst / 2;
        var radius = Math.Min(centerXDst, centerYDst);

        // 2. Pre-calculate the base mapping arrays in parallel (IMPROVED)
        using var mapXBase = new Mat(hDst, wDst, MatType.CV_32FC1);
        using var mapYBase = new Mat(hDst, wDst, MatType.CV_32FC1);
        var mapXIndexer = mapXBase.GetGenericIndexer<float>();
        var mapYIndexer = mapYBase.GetGenericIndexer<float>();

        // Parallelize the outer loop to drastically reduce the initial setup time.
        // Each thread works on a different row (yDst), so there are no data conflicts.
        Parallel.For(0, hDst, yDst =>
        {
            for (var xDst = 0; xDst < wDst; xDst++)
            {
                double dx = xDst - centerXDst;
                double dy = yDst - centerYDst;
                var r = Math.Sqrt(dx * dx + dy * dy);

                if (r <= radius)
                {
                    var theta = Math.Atan2(dy, dx);
                    mapYIndexer[yDst, xDst] = (float)(r / radius * hSrc);
                    mapXIndexer[yDst, xDst] = (float)((theta + Math.PI) / (2 * Math.PI) * wSrc);
                }
                else
                {
                    mapYIndexer[yDst, xDst] = -1;
                    mapXIndexer[yDst, xDst] = -1;
                }
            }
        });

        // 3. Generate each frame in parallel by shifting the map
        var shiftPerFrame = (double)wSrc / numFrames;

        Parallel.For(0, numFrames, i =>
        {
            var shiftX = i * shiftPerFrame;

            using var mapXShifted = new Mat();
            using var projectedImage = new Mat();
            Cv2.Add(mapXBase, new Scalar(shiftX), mapXShifted);

            var shiftedIndexer = mapXShifted.GetGenericIndexer<float>();
            for (int y = 0; y < hDst; y++)
            {
                for (int x = 0; x < wDst; x++)
                {
                    if (shiftedIndexer[y, x] != -1)
                    {
                        shiftedIndexer[y, x] %= wSrc;
                    }
                }
            }
                
            Cv2.Remap(
                src: sourceImage,
                dst: projectedImage,
                map1: mapXShifted,
                map2: mapYBase,
                interpolation: InterpolationFlags.Linear,
                borderMode: BorderTypes.Constant,
                borderValue: new Scalar(0, 0, 0)
            );
                
            var filename = $"frame_{i:D4}.png";
            var outputPath = Path.Combine(outputFolder, filename);
            projectedImage.ImWrite(outputPath);
                
            Console.WriteLine($"Image saved to {outputPath}");
        });
    }

    public Task CreateGlobeRotationSequenceAsync(string sourceImagePath, string outputFolder, int numFrames = 360)
    {
        return Task.Run(() =>
        {
            CreateGlobeRotationSequence(sourceImagePath, outputFolder, numFrames);
        });
    }
}