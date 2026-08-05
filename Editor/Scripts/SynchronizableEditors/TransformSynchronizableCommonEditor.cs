using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;

namespace Alteruna.UnityEditor
{
	[CustomEditor(typeof(TransformSynchronizableCommon), true), CanEditMultipleObjects]
	public class TransformSynchronizableCommonEditor : SynchronizableEditor
	{
		public TransformSynchronizableCommonEditor() : base(true, true) { }
	}
}