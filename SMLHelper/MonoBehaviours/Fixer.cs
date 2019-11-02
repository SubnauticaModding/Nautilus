namespace SMLHelper.V2.MonoBehaviours
{
    using UnityEngine;
    using System.Reflection;
    using Logger = V2.Logger;

    public class Fixer : MonoBehaviour, IProtoEventListener
    {
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

            if (Time.time > time && gameObject != Builder.prefab)
            {
                if (transform.position.y < -4500)
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

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            var constructable = GetComponent<Constructable>();
            if (constructable != null)
            {
                constructable.techType = techType;
            }

            var techTag = GetComponent<TechTag>();
            if (techTag != null)
            {
                techTag.type = techType;
            }
        }
    }
}
