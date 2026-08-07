#if !UNITY_2019
using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Unity;
using UnityEditor;
using UnityEngine;
using Avatar = Alteruna.Multiplayer.Unity.Avatar;

namespace Alteruna.UnityEditor
{
	public static class CreateObject
	{
		[MenuItem("GameObject/Alteruna/Multiplayer Manager", false, 0)]
		private static void CreateMultiplayerObject(MenuCommand menuCommand)
		{
			CreateMultiplayerObject();

			/*
			if (Selection.activeGameObject != null)
			{
				Transform t = go.transform;
				t.parent = Selection.activeGameObject.transform;
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
			}
			*/
		}

		private static MultiplayerManager CreateMultiplayerObject()
		{
			GameObject go = new GameObject("Multiplayer Manager");
			MultiplayerManager mpm = go.AddComponent<MultiplayerManager>();
			EditorUtility.SetDirty(go);
			return mpm;
		}
		
		[MenuItem("GameObject/Alteruna/Avatar", false)]
		private static void CreateAvatarObject(MenuCommand menuCommand)
		{
			var multiplayer = Object.FindObjectOfType<MultiplayerManager>(true);
			if (multiplayer == null)
			{
				multiplayer = CreateMultiplayerObject();
			}
			
			/*
			// Search for an existing red material in the project
			Material redMaterial = FindMaterial("AvatarMat");

			// If not found, create a new red material
			if (redMaterial == null)
			{
				redMaterial = new Material(Shader.Find("Standard"));
				redMaterial.color = new Color32(0, 170, 212, 255);
				AssetDatabase.CreateAsset(redMaterial, "Assets/AvatarMat.mat");
			}
			*/
			
			// Create avatar game object.
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			go.name = "Avatar";
			
			Transform t = go.transform;
			if (Selection.activeGameObject != null)
			{
				t.parent = Selection.activeGameObject.transform;
			}
			t.localPosition = new Vector3(0, 0.5f, 0);
			t.localRotation = Quaternion.identity;
			
			var mr = go.GetComponent<MeshRenderer>();
			//mr.material = redMaterial;
			
			var avatar = go.AddComponent<Avatar>();
			go.AddComponent<UniqueAvatarColor>().Renderers = new Renderer[] { mr };
			go.AddComponent<TransformSynchronizable>();
			
			// Create camera.
			GameObject camera = new GameObject("Camera");
			Transform cameraTransform = camera.transform;
			cameraTransform.parent = t;
			cameraTransform.localPosition = new Vector3(0, 0.5f, 0);
			cameraTransform.localRotation = Quaternion.identity;

			camera.AddComponent<Camera>().nearClipPlane = 0.4f;
			camera.AddComponent<AudioListener>();
			camera.AddComponent<TransformSynchronizable>();
			
			// Create googles.
			Transform googles = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			googles.parent = cameraTransform;
			googles.name = "Eyes";
			Object.DestroyImmediate(googles.GetComponent<BoxCollider>());
			googles.localPosition = new Vector3(0, 0, 0.4f);
			googles.localScale = new Vector3(0.4f, 0.3f, 0.3f);
			googles.localRotation = Quaternion.identity;

			// Set avatar prefab in multiplayer.
			if (multiplayer.AvatarPrefab == null)
			{
				multiplayer.AvatarPrefab = avatar;
				if (multiplayer.AvatarSpawning == AvatarBehavior.Disabled)
				{
					multiplayer.AvatarSpawning = AvatarBehavior.SpawnOnJoin;
				}
				go.SetActive(false);
				EditorUtility.SetDirty(multiplayer);
			}

			EditorUtility.SetDirty(go);
		}
		
		private static Material FindMaterial(string name)
		{
			string[] guids = AssetDatabase.FindAssets(name + " t:material");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
				if (material != null)
					return material;
			}
			return null;
		}
	}
}
#endif