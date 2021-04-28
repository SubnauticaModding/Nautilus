using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace SMLHelper.V2.Interfaces
{
    public interface ICoordinatedSpawnHandler
    {
        void RegisterCoordinatedSpawn(SpawnInfo spawnInfo);
        
        void RegisterCoordinatedSpawns(List<SpawnInfo> spawnInfos);
        
        void RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, List<Vector3> coordinatesToSpawnTo);
    }
}