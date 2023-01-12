namespace Example_mod
{
    using System.Collections;
    using SMLHelper.Assets;
    using UnityEngine;

    public class MyFirstPrefab: ModPrefab
    {
        public MyFirstPrefab() : base("TitianiumClone", "TitianiumClonePrefab")
        {

        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var task = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(TechType.Titanium, task);
            gameObject.Set(task.Get());
        }
    }
}
