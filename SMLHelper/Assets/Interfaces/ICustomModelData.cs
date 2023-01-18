namespace SMLHelper.Assets.Interfaces
{
    public interface ICustomModelData
    {
        /// <summary>
        /// The <see cref="CustomModelData"/> SMLHelper will use on the GameObject before it is sent to the game.
        /// </summary>
        public CustomModelData[] ModelDatas { get; }
    }
}