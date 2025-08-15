using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Alteruna
{
	public class MatchmakingEntry : MonoBehaviour
	{
		public TMP_Text NameText;
		public GameObject OwnerIcon;
		public GameObject ReadyIcon;

		[HideInInspector] public bool Owner;
		[HideInInspector] public bool Ready;
		[HideInInspector] public ushort Id;

		public void SetOwner(bool isOwner)
		{
			Owner = isOwner;
			OwnerIcon.SetActive(isOwner);
		}

		public void SetReady(bool isReady)
		{
			Ready = isReady;
			ReadyIcon.SetActive(isReady);
		}
	}
}