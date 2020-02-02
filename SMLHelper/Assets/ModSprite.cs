namespace SMLHelper.V2.Assets
{
    using System;
    using System.Collections.Generic;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif

    /// <summary>
    /// A class that handles a custom sprite and what item it is associated to.
    /// </summary>
    internal class ModSprite
    {
#if SUBNAUTICA
        internal static void Add(SpriteManager.Group group, string name, Atlas.Sprite sprite)
        {
            Dictionary<string, Sprite> dictionary = GetGroupDictionary(ref group);
            dictionary[name] = sprite;
        }
#endif
        internal static void Add(SpriteManager.Group group, string name, UnityEngine.Sprite sprite)
        {
            Dictionary<string, Sprite> dictionary = GetGroupDictionary(ref group);
#if SUBNAUTICA
            dictionary[name] = new Atlas.Sprite(sprite);
#elif BELOWZERO
            dictionary[name] = sprite;
#endif
        }

        private static Dictionary<string, Sprite> GetGroupDictionary(ref SpriteManager.Group group)
        {
            if (group == SpriteManager.Group.None)
                group = SpriteManager.Group.Item;
            // There are no calls for sprites in the None Group.
            // All sprite calls for almost anything we don't manually group is in the Item group.

            if (!ModSprites.TryGetValue(group, out Dictionary<string, Sprite> dictionary))
            {
                dictionary = new Dictionary<string, Sprite>(StringComparer.InvariantCultureIgnoreCase);
                ModSprites.Add(group, dictionary);
            }

            return dictionary;
        }

        internal static void Add(ModSprite sprite)
        {
            Add(sprite.Group, sprite.Id, sprite.Sprite);
        }

        internal static Dictionary<SpriteManager.Group, Dictionary<string, Sprite>> ModSprites
            = new Dictionary<SpriteManager.Group, Dictionary<string, Sprite>>();

        /// <summary>
        /// The tech type of a specific item associated with this sprite.
        /// Can be <see cref="TechType.None"/> if this sprite is for used on a group.
        /// </summary>
        public TechType TechType;

        /// <summary>
        /// The actual sprite used in-game when this sprite is references.
        /// </summary>
        public Sprite Sprite;

        /// <summary>
        /// The group that this sprite belongs to. 
        /// Can be <see cref="SpriteManager.Group.None"/> if this sprite is for used on an item.
        /// </summary>
        public SpriteManager.Group Group;

        /// <summary>
        /// The internal identifier of this sprite when it isn't associated to an item.
        /// </summary>
        public string Id;

        /// <summary>
        /// Creates a new ModSprite to be used with a specific TechType.
        /// Created with an Atlas Sprite.
        /// </summary>
        /// <param name="type">The techtype paired to this sprite.</param>
        /// <param name="sprite">The sprite to be added.</param>
        public ModSprite(TechType type, Sprite sprite)
        {
            TechType = type;
            Id = type.AsString();
            Sprite = sprite;
            Group = SpriteManager.Group.Item;
        }

        /// <summary>
        /// Creates a new ModSprite to be used with a specific group and internal ID.
        /// Created with an Atlas Sprite.
        /// </summary>
        /// <param name="group">The sprite group.</param>
        /// <param name="id">The sprite internal identifier.</param>
        /// <param name="sprite">The sprite to be added.</param>
        public ModSprite(SpriteManager.Group group, string id, Sprite sprite)
        {
            Group = group;
            Id = id;
            Sprite = sprite;
            TechType = TechType.None;
        }

        /// <summary>
        /// Creates a new ModSprite to be used with a specific group and internal ID.
        /// Created with an Atlas Sprite.
        /// </summary>
        /// <param name="group">The sprite group.</param>
        /// <param name="type">The techtype paired to this sprite.</param>
        /// <param name="sprite">The sprite to be added.</param>
        public ModSprite(SpriteManager.Group group, TechType type, Sprite sprite)
        {
            Group = group;
            Id = type.AsString();
            Sprite = sprite;
            TechType = type;
        }
    }
}
