namespace SMLHelper.V2.MonoBehaviours
{
    using SMLHelper.V2.Assets;
    using UnityEngine;
    using Logger = V2.Logger;

    /// <summary>
    /// This <see cref="MonoBehaviour"/> is <c>automatically</c> added to any <see cref="GameObject"/> created through <see cref="ModPrefab.GetGameObject"/>.
    /// </summary>
    public class Fixer : MonoBehaviour, IProtoEventListener
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        [SerializeField]
        public TechType techType;

        [SerializeField]
        public string ClassId;

        private float time;
        private bool initalized;

        private void Update()
        {
            if (!initalized)
            {
                time = Time.time + 1f;
                initalized = true;
            }

            if (Time.time > time && this.gameObject != Builder.prefab)
            {
                if (this.transform.position.y < -4500)
                {
                    Logger.Debug("Destroying object: " + this.gameObject);
                    Destroy(this.gameObject);
                }
                else
                {
                    Logger.Debug("Destroying Fixer for object: " + this.gameObject);
                    Destroy(this);
                }
            }
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            Constructable constructable = GetComponent<Constructable>();
            if (constructable != null)
            {
                constructable.techType = techType;
            }

            TechTag techTag = GetComponent<TechTag>();
            if (techTag != null)
            {
                techTag.type = techType;
            }
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
