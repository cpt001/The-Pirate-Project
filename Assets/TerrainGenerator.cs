using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private enum TerrainSize
    {
        Tiny,
        Small,
        Medium,
        Large,
        Enormous,
    }
    [SerializeField] private TerrainSize terrainSize;
    [SerializeField] private int numberOfPeaks;
    private enum TerrainShape
    {
        U,
        S,
        I,
        L,
        W,
    }
    [SerializeField] private TerrainShape terrainShape;
    [SerializeField] private float slopeSeverity;

    /// <summary>
    /// Get the terrain size
    /// Generate the number of requested peaks
    /// Multiply the distance based on island size (random value, set by enum)
    /// Move the peaks into the requested terrain pattern
    /// Link peaks together from north to south?
    /// Generate local grid
    /// Set height of each local grid space in a descending pattern from each peak
    /// -Height needs to reduce at a lerp speed, to produce more natural peaks
    /// Create a mesh that connects each of the grid spaces together
    /// Terrain generated?
    /// </summary>

    public void GenerateTerrain()
    {
        switch (terrainSize)
        {
            case TerrainSize.Tiny:
                {
                    break;
                }
            case TerrainSize.Small:
                {
                    break;
                }
            case TerrainSize.Medium:
                {
                    break;
                }
            case TerrainSize.Large:
                {
                    break;
                }
            case TerrainSize.Enormous:
                {
                    break;
                }
        }
    }
}
