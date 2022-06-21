using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldStreaming;
using System.IO;
namespace SMLHelper.V2.BiomeThings
{
    static class BatchOctreesExtensions
    {
        private static List<BatchOctrees> dirtyBatches = new List<BatchOctrees>();

        static public void SetOctree(this BatchOctrees batchOctrees, Int3 octreeId, VoxelandData.OctNode root)
        {
            var allocator = batchOctrees.allocator;

            var octree = batchOctrees.GetOctree(octreeId);
            octree.Set(root, allocator);

            if (!dirtyBatches.Contains(batchOctrees))
            {
                dirtyBatches.Add(batchOctrees);
            }
        }

        static public void WriteOctrees(this BatchOctrees batchOctrees)
        {
            var streamer = batchOctrees.streamer;

            var tmpPath = streamer.GetTmpPath(batchOctrees.id);

            using (var binaryWriter = new BinaryWriter(File.OpenWrite(tmpPath)))
            {
                var version = 4;
                binaryWriter.WriteInt32(version);
                foreach (Octree octree in batchOctrees.octrees)
                {
                    octree.Write(binaryWriter);
                }
            }

            dirtyBatches.Remove(batchOctrees);
        }

        static public bool GetIsDirty(this BatchOctrees batchOctrees)
        {
            return dirtyBatches.Contains(batchOctrees);
        }
    }
}
