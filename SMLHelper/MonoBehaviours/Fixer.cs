namespace SMLHelper.V2.MonoBehaviours
{
    using SMLHelper.V2.Assets;
    using UnityEngine;
    using Logger = V2.Logger;

    /// <summary>
    /// This <see cref="MonoBehaviour"/> is <c>automatically</c> added to any <see cref="GameObject"/> created through <see cref="ModPrefab.GetGameObject"/>.
    /// </summary>
    internal class Fixer : MonoBehaviour
    {
        private float time;
        private bool isPrefab => transform.position.y < -4500f;

        public void Awake()
        {
            time = Time.time + 1f;

            // preventing LargeWorldEntity registration for prefabs (registered in LargeWorldEntity.Start)
            if (gameObject.GetComponentInChildren<LargeWorldEntity>() is LargeWorldEntity lwe)
                lwe.enabled = !isPrefab;
        }

        public void Update()
        {
            if (Time.time < time || gameObject == Builder.prefab)
                return;

            if (isPrefab)
            {
                Logger.Debug("Destroying object: " + gameObject);
                Destroy(gameObject);
            }
            else
            {
                Logger.Debug("Destroying Fixer for object: " + gameObject);
                Destroy(this);
            }
        }
    }
}
