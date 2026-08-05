using Alteruna.Multiplayer.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Alteruna.Multiplayer.Unity
{
	/// <summary>
	/// Change Hue to a unique color based on avatar index.
	/// </summary>
	/// <remarks>
	///	<img src="../images/Alteruna.UniqueAvatarColor.png" />
	/// </remarks>
	[HelpURL("https://docs.v2.alteruna.com/html/T_Alteruna_Multiplayer_Unity_UniqueAvatarColor.htm")]
	[AddComponentMenu("Alteruna/Avatar/Unique Avatar Color"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
	public class UniqueAvatarColor : CommunicationBridge
	{
		/// <summary>
		/// Saturation of generated color.
		/// </summary>
		[Range(0f, 1f)]
		public float Saturation = 0.6f;

		/// <summary>
		/// References to sprite renderers to be affected by hue changes.
		/// </summary>
		[FormerlySerializedAs("sprites")]
		public SpriteRenderer[] Sprites;

		/// <summary>
		/// References to mesh renderers to be affected by hue changes.
		/// </summary>
		[FormerlySerializedAs("meshes")]
		public Renderer[] Renderers;

		private int _lastId = -1;

		/// <summary>
		/// Manually update color of objects referenced inside the <c>UniqueAvatarColor</c>.
		/// </summary>
		public void UpdateHue()
		{
			if (_lastId >= 0)
			{
				Possess((ushort)_lastId);
			}
		}
		
		/// <summary>
		/// Manually set color of objects referenced inside the <c>UniqueAvatarColor</c>.
		/// </summary>
		/// <param name="index">User index</param>
		public void UpdateHue(ushort index)
		{
			Possess(index);
		}

		public override void Possessed(bool isMe, User user) => Possess(user.Index);

		private void Possess(ushort index)
		{
			if (index == _lastId)
			{
				return;
			}

			_lastId = index;

			foreach (SpriteRenderer spriteRenderer in Sprites)
			{
				spriteRenderer.color = HueFromId(spriteRenderer.color, _lastId, Saturation);
			}

			foreach (var rend in Renderers)
			{
				Material m = rend.material;
				m.color = HueFromId(m.color, _lastId, Saturation);
				rend.material = m;
			}
		}

		/// <summary>
		/// Set hue of a color based on ID.
		/// </summary>
		/// <param name="color">Base color.</param>
		/// <param name="id">Color ID.</param>
		/// <returns>Color with new hue.</returns>
		public static Color HueFromId(Color color, int id)
		{
			Color.RGBToHSV(color, out _, out float s, out float v);
			return Color.HSVToRGB(((id * 3 + 3) % 8.5f) / 9f, s, v);
		}

		/// <summary>
		/// Set hue of a color based on ID.
		/// </summary>
		/// <param name="color">Base color.</param>
		/// <param name="id">Color ID.</param>
		/// <param name="saturation">Saturation of returned color.</param>
		/// <returns>Color with new hue with given saturation and value.</returns>
		public static Color HueFromId(Color color, int id, float saturation)
		{
			Color.RGBToHSV(color, out _, out float _, out float v);
			return HueFromId(color, id, saturation, v);
		}

		/// <summary>
		/// Set hue of a color based on ID.
		/// </summary>
		/// <param name="color">Base color.</param>
		/// <param name="id">Color ID.</param>
		/// <param name="saturation">Saturation of returned color.</param>
		/// <param name="value">Value of returned color.</param>
		/// <returns>Color with new hue with given saturation and value.</returns>
		public static Color HueFromId(Color color, int id, float saturation, float value) =>
			Color.HSVToRGB(((id * 3 + 3) % 8.5f) / 9f, saturation, value);
	}
}