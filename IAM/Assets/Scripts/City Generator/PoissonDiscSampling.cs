using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PoissonDiscSampling
{
    static  float sqrt2 = Mathf.Sqrt(2);
    const float minRadius = 0.01f;

    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30, float popDecimation = 0, string seed = "")
    {
        int pSeed = seed == "" ? (int)System.DateTime.Now.Ticks : seed.GetHashCode();
        System.Random prng = new System.Random(pSeed);
        //System.Random prng = new System.Random(42069);

        List<Vector2> sampledPoints = new List<Vector2>();
        radius = Mathf.Max(minRadius, radius);
        float cellSize = GetCellSize(radius);

        //uses 1 based indexing, 0 is unoccupied
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x/cellSize),Mathf.CeilToInt(sampleRegionSize.y/cellSize)];

        List<Vector2> spawnPoints = new List<Vector2>();

        //spawnPoints.Add(new Vector2(Random.value*sampleRegionSize.x,Random.value*sampleRegionSize.y));
        spawnPoints.Add(new Vector2((float)prng.NextDouble()*sampleRegionSize.x, (float)prng.NextDouble() * sampleRegionSize.y));
        while(spawnPoints.Count > 0)
        {
           // int spawnIndex = Random.Range(0, spawnPoints.Count);
            int spawnIndex = prng.Next(0, spawnPoints.Count);

            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool candidateAccepted = false;
            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                //float angle = Random.value * Mathf.PI * 2;
                float angle = (float)prng.NextDouble() * Mathf.PI * 2;

                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
               // Vector2 candidatePoint = spawnCenter + dir * Random.Range(radius, 2 * radius);
                Vector2 candidatePoint = spawnCenter + dir * (((float)prng.NextDouble()*radius)+radius);

                if (IsValid(candidatePoint, sampleRegionSize, cellSize, radius, sampledPoints, grid))
                {
                    sampledPoints.Add(candidatePoint);
                    spawnPoints.Add(candidatePoint);
                    grid[(int)(candidatePoint.x / cellSize), (int)(candidatePoint.y / cellSize)] = sampledPoints.Count;
                    candidateAccepted = true;
                    break;
                }//end if is valid
            }   // end for num sampling
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }// end if candidate accepted
        }// end while
        return DecreasePopulation( sampledPoints,popDecimation);
    }

    static float GetCellSize(float r)
    {
        return r / sqrt2;
    }

    static bool IsValid(Vector2 candidatePoint, Vector2 sampleRegionSize, float cellSize,float radius,List<Vector2> points, int[,] grid)
    {
        if(candidatePoint.x >= 0 && candidatePoint.x < sampleRegionSize.x && candidatePoint.y >= 0 && candidatePoint.y < sampleRegionSize.y)
        {//inside of region
            int cellX = (int)(candidatePoint.x / cellSize);
            int cellY = (int)(candidatePoint.y / cellSize);

            int searchStartX = Mathf.Max(0,cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0)-1);

            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int idx = grid[x, y] - 1;
                    if(idx != -1)
                    {
                        float dist = (candidatePoint - points[idx]).sqrMagnitude;
                        if(dist < radius*radius)
                        {
                            return false;
                        }
                    }
                }// end for y
            }// end for x
            return true;
        }// end if inside regions
        return false;
    }


    static List<Vector2> DecreasePopulation(List<Vector2> pop, float decimationProbability)
    {
        System.Random prng = new System.Random((int)System.DateTime.Now.Ticks);
        //pop.RemoveAll((v) => { return Random.value < decimationProbability; });
        pop.RemoveAll((v) => { return prng.NextDouble() < decimationProbability; });
        return pop;
    }

    static List<Vector3> DecreasePopulation(List<Vector3> pop, float decimationProbability)
    {
        System.Random prng = new System.Random((int)System.DateTime.Now.Ticks);
        //pop.RemoveAll((v) => { return Random.value < decimationProbability; });
        pop.RemoveAll((v) => { return prng.NextDouble() < decimationProbability; });
        return pop;
    }

    [MenuItem("Log Circle/Circle")]
    static void LogCircleValues()
    {
        const float numPoints = 16f;

        for (int i = 0; i < numPoints; i++)
        {
            float percent = (float)i / numPoints;

            float angleRad = percent * Mathf.PI * 2f;

            Vector2 dir = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
            Debug.Log(dir);
        }
    }
}

