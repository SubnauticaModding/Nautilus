using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldStreaming;
using System.IO;
namespace SMLHelper.V2.BiomeThings
{
    static class BatchOctreesStreamerExtensions
    {
        public static string GetTmpPath(this BatchOctreesStreamer batchOctreesStreamer, Int3 batchId)
        {
            var tmpPathPrefix = Path.Combine(LargeWorldStreamer.main.tmpPathPrefix, "CompiledOctreesCache");

            if (!Directory.Exists(tmpPathPrefix))
            {
                Directory.CreateDirectory(tmpPathPrefix);
            }

            var fileName = $"compiled-batch-{batchId.x}-{batchId.y}-{batchId.z}.optoctrees";
            var fullPath = Path.Combine(tmpPathPrefix, fileName);

            return fullPath;
        }

        public static void SetBatchOctree(this BatchOctreesStreamer batchOctreesStreamer, Int3 absoluteOctreeId, VoxelandData.OctNode root)
        {
            var numOctreesPerBatch = batchOctreesStreamer.numOctreesPerBatch;

            var batchId = Int3.FloorDiv(absoluteOctreeId, numOctreesPerBatch);
            var batch = batchOctreesStreamer.GetBatch(batchId);

            var octreeId = absoluteOctreeId - (batchId * numOctreesPerBatch);

            Logger.Debug($"numOctreesPerBatch = {numOctreesPerBatch}, batchId = {batchId}, octreeId = {octreeId}, absoluteOctreeId = {absoluteOctreeId}");

            batch.SetOctree(octreeId, root);
        }

        public static void WriteBatchOctrees(this BatchOctreesStreamer batchOctreesStreamer)
        {
            var batches = batchOctreesStreamer.batches;
            foreach (var batchOctrees in batches)
            {
                if (batchOctrees != null && batchOctrees.IsLoaded() && (batchOctrees.GetIsDirty()))
                {
                    Logger.Info($"Octrees of batch {batchOctrees.id} is dirty. Writing to temp save data prior saving to save slot.");
                    batchOctrees.WriteOctrees();
                }
            }
        }
    }
}
