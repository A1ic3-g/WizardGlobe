using System;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;

namespace WizardPlanet.Services;

public class GlobeGenerationService : IGlobeGenerationService
{
    /// <summary>
    /// Creates a sequence of frames depicting a rotating globe from a flat map image.
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

        // 2. Pre-calculate the base mapping arrays (once)
        using Mat mapXBase = new Mat(hDst, wDst, MatType.CV_32FC1);
        using Mat mapYBase = new Mat(hDst, wDst, MatType.CV_32FC1);
        // Create indexers for fast pixel access
        var mapXIndexer = mapXBase.GetGenericIndexer<float>();
        var mapYIndexer = mapYBase.GetGenericIndexer<float>();

        for (int yDst = 0; yDst < hDst; yDst++)
        {
            for (int xDst = 0; xDst < wDst; xDst++)
            {
                double dx = xDst - centerXDst;
                double dy = yDst - centerYDst;

                double r = Math.Sqrt(dx * dx + dy * dy);
                        
                if (r <= radius)
                {
                    double theta = Math.Atan2(dy, dx);
                    // Map polar coordinates (r, theta) to equirectangular coordinates (x, y)
                    mapYIndexer[yDst, xDst] = (float)(r / radius * hSrc);
                    mapXIndexer[yDst, xDst] = (float)((theta + Math.PI) / (2 * Math.PI) * wSrc);
                }
                else
                {
                    // Mark pixels outside the circle to be ignored by remap
                    mapYIndexer[yDst, xDst] = -1;
                    mapXIndexer[yDst, xDst] = -1;
                }
            }
        }
                
        // 3. Generate each frame by shifting the map
        double shiftPerFrame = (double)wSrc / numFrames;

        for (int i = 0; i < numFrames; i++)
        {
            double shiftX = i * shiftPerFrame;

            // Apply the circular shift to the pre-calculated x-map
            // We can do this efficiently using OpenCV's built-in functions
            using (Mat mapXShifted = new Mat())
            using (Mat projectedImage = new Mat())
            {
                Cv2.Add(mapXBase, new Scalar(shiftX), mapXShifted);
                        
                // Manually apply modulo where map value is not -1
                // This replicates the Python logic: (map_x_base + shift_x) % w_src
                var shiftedIndexer = mapXShifted.GetGenericIndexer<float>();
                for (int y = 0; y < hDst; y++)
                {
                    for (int x = 0; x < wDst; x++)
                    {
                        if(shiftedIndexer[y,x] != -1)
                        {
                            shiftedIndexer[y, x] = (float)(shiftedIndexer[y, x] % wSrc);
                        }
                    }
                }

                // Remap the source image using the calculated maps
                Cv2.Remap(
                    src: sourceImage,
                    dst: projectedImage,
                    map1: mapXShifted,
                    map2: mapYBase,
                    interpolation: InterpolationFlags.Linear,
                    borderMode: BorderTypes.Constant,
                    borderValue: new Scalar(0, 0, 0)
                );
                        
                // Save the resulting frame
                var filename = $"frame_{i:D4}.png";
                var outputPath = Path.Combine(outputFolder, filename);
                projectedImage.ImWrite(outputPath);
                Console.WriteLine($"Image saved to {outputPath}");
            }
        }
    }

    public Task CreateGlobeRotationSequenceAsync(string sourceImagePath, string outputFolder, int numFrames = 360)
    {
        return Task.Run(() =>
        {
            CreateGlobeRotationSequence(sourceImagePath, outputFolder, numFrames);
        });
    }
}