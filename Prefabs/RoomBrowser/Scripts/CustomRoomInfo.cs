namespace Alteruna
{
	public enum GameMode : byte
	{
		FreeForAll,
		Teams
	}

	public class CustomRoomInfo
	{
		public GameMode GameMode { get; set; }
		public int SceneIndex { get; set; }
	}
}
