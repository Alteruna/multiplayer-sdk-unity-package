namespace Alteruna.Multiplayer.Unity
{
	public partial class InterpolationTransformSynchronizable
	{
		/// <summary>
		/// Methods for interpolate, extrapolate, and other.
		/// </summary>
		public enum InterpolationMethodType
		{
			None,
			Lerp,
			LerpRelative,
			SmoothDamp,
			Spring,
			Extrapolate
		}
	}
}