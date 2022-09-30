﻿// Visual Pinball Engine
// Copyright (C) 2022 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

// ReSharper disable PossibleUnintendedReferenceComparison

using System.Collections.Generic;
using System.Linq;
using NetVips;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;

namespace VisualPinball.Unity.Editor
{
	/// <summary>
	/// What's rendered on the right panel.
	/// </summary>
	public class AssetDetails : VisualElement
	{
		public Asset Asset {
			set {
				if (_asset == value) {
					return;
				}
				// toggle empty label
				if (value != null && _asset == null) {
					SetVisibility(_emptyLabel, false);
					SetVisibility(_scrollView, true);
				}
				if (value == null && _asset != null) {
					SetVisibility(_emptyLabel, true);
					SetVisibility(_scrollView, false);
				}
				_asset = value;
				if (value != null) {
					var so = new SerializedObject(_asset);
					if (_asset.Library.IsLocked) {
						_bodyReadOnly.Bind(so);
						BindReadOnly(_asset);
					} else {
						_body.Bind(so);
					}
					Bind(_asset);
				}
			}
		}

		private Asset _asset;

		private readonly TemplateContainer _header;
		private readonly TemplateContainer _body;
		private readonly TemplateContainer _bodyReadOnly;
		private readonly TemplateContainer _footer;

		private readonly ScrollView _scrollView;
		private readonly Label _emptyLabel;

		private Toggle _replaceSelectedKeepName;

		public new class UxmlFactory : UxmlFactory<AssetDetails, UxmlTraits> { }

		public AssetDetails()
		{
			var header = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/org.visualpinball.engine.unity/VisualPinball.Unity/VisualPinball.Unity.Editor/AssetBrowser/AssetStructure/AssetDetails_Header.uxml");
			_header = header.CloneTree();
			var body = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/org.visualpinball.engine.unity/VisualPinball.Unity/VisualPinball.Unity.Editor/AssetBrowser/AssetStructure/AssetDetails_Body.uxml");
			_body = body.CloneTree();
			var bodyReadOnly = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/org.visualpinball.engine.unity/VisualPinball.Unity/VisualPinball.Unity.Editor/AssetBrowser/AssetStructure/AssetDetails_Body_ReadOnly.uxml");
			_bodyReadOnly = bodyReadOnly.CloneTree();
			var footer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/org.visualpinball.engine.unity/VisualPinball.Unity/VisualPinball.Unity.Editor/AssetBrowser/AssetStructure/AssetDetails_Footer.uxml");
			_footer = footer.CloneTree();
			var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/org.visualpinball.engine.unity/VisualPinball.Unity/VisualPinball.Unity.Editor/AssetBrowser/AssetStructure/AssetDetails.uss");
			styleSheets.Add(styleSheet);

			_scrollView = new ScrollView();
			_scrollView.Add(_header);
			_scrollView.Add(_bodyReadOnly);
			_scrollView.Add(_body);
			_scrollView.Add(_footer);

			_emptyLabel = new Label("No item selected.") {
				style = {
					alignSelf = Align.Center,
					marginTop = 24,
					marginBottom = 24,
					unityFontStyleAndWeight = FontStyle.Italic
				}
			};

			// bind buttons
			_bodyReadOnly.Q<Button>("add-selected").clicked += OnAddSelected;
			_bodyReadOnly.Q<Button>("replace-selected").clicked += OnReplaceSelected;
			_replaceSelectedKeepName = _bodyReadOnly.Q<Toggle>("replace-selected-keep-name");

			Add(_emptyLabel);
			Add(_scrollView);

			SetVisibility(_emptyLabel, true);
			SetVisibility(_scrollView, false);
		}

		public void Bind(Asset asset)
		{
			_header.Q<Label>("title").text = asset.Name;
			_header.Q<Image>("library-icon").image = Icons.AssetLibrary(IconSize.Small);
			_header.Q<Label>("library-name").text = asset.Library != null ? asset.Library.Name : "<no library>";

			_header.Q<Image>("category-icon").image = EditorGUIUtility.IconContent("d_Folder Icon").image;
			_header.Q<Label>("category-name").text = asset.Category?.Name ?? "<no category>";
			_header.Q<Image>("date-icon").image = Icons.Calendar(IconSize.Small);
			_header.Q<Label>("date-value").text = asset.AddedAt.ToLongDateString();

			_header.Q<PreviewEditorElement>("preview").Object = asset.Object;
			_body.Q<PresetDropdownElement>("thumb-camera-preset").SetValue(asset.ThumbCameraPreset);
			BindInfo(asset);

			SetVisibility(_bodyReadOnly, _asset.Library.IsLocked);
			SetVisibility(_body, !_asset.Library.IsLocked);
		}

		private void BindInfo(Asset asset)
		{
			// info
			if (asset.Object is GameObject go) {
				SetVisibility(_footer.Q<Foldout>("footer-info"), true);
				var (meshes, subMeshes, vertices, triangles, uvs, materials) = CountVertices(go);
				const string separator = ", ";
				_footer.Q<Label>("info-geo-stats").text =
					vertices + (vertices == 1 ? " vertex" : " vertices") + separator +
					triangles + " triangle" + (triangles == 1 ? "" : "s") + separator +
					uvs + " uv" + (uvs == 1 ? "" : "s") + separator +
					meshes + " mesh" + (meshes == 1 ? "" : "es") + separator +
					subMeshes + " sub mesh" + (subMeshes == 1 ? "" : "es") + separator +
					materials + " material" + (materials == 1 ? "" : "s");

			} else {
				SetVisibility(_footer.Q<Foldout>("footer-info"), false);
			}
		}

		private void BindReadOnly(Asset asset)
		{
			// material variations
			_bodyReadOnly.Q<AssetMaterialVariationsElement>("material-variations").MaterialVariations = asset.MaterialVariations;

			// description
			SetVisibility(_bodyReadOnly.Q<Label>("description"), !string.IsNullOrEmpty(_asset.Description));

			// tags
			var tags = _bodyReadOnly.Q<Foldout>("tags-foldout");
			if (asset.Tags is { Count: > 0 }) {
				var container = tags.Q<VisualElement>("tags-container");
				container.Clear();
				foreach (var tag in asset.Tags) {
					container.Add(new AssetTagElement(tag));
				}
				SetVisibility(tags, true);
			} else {
				SetVisibility(tags, false);
			}

			// attributes
			var attributes = _bodyReadOnly.Q<Foldout>("attributes-container");
			if (asset.Attributes is { Count: > 0 }) {
				attributes.Clear();
				foreach (var attribute in asset.Attributes) {
					attributes.Add(new AssetAttributeElement(attribute));
				}
				SetVisibility(attributes, true);
			} else {
				SetVisibility(attributes, false);
			}

			// links
			var links = _bodyReadOnly.Q<Foldout>("links-foldout");
			if (asset.Links is { Count: > 0 }) {
				var container = links.Q<VisualElement>("links-container");
				container.Clear();
				foreach (var link in asset.Links) {
					container.Add(new AssetLinkElement(link));
				}
				SetVisibility(links, true);
			} else {
				SetVisibility(links, false);
			}
		}

		private void OnAddSelected()
		{
			if (_asset.Scale == AssetScale.World) {
				InstantiateAsset(); // should automatically land at 0/0, since the playfield is centered
				return;
			}

			if (_asset.Scale == AssetScale.Table) {

				// find parent
				var (pf, parentTransform) = FindPlayfieldAndParent();
				if (pf == null || parentTransform == null) {
					return;
				}

				// instantiate
				var go = InstantiateAsset(parentTransform);

				// move to the middle of the playfield
				if (go.GetComponent(typeof(IMainRenderableComponent)) is IMainRenderableComponent comp) {
					comp.SetEditorPosition(new Vector3(pf.Width / 2, pf.Height / 2, 0));
					comp.UpdateTransforms();
				}
			}
		}

		private static (PlayfieldComponent, Transform) FindPlayfieldAndParent()
		{
			// first, check selection in hierarchy.
			if (Selection.activeGameObject != null) {
				var pf = Selection.activeGameObject.GetComponentInParent<PlayfieldComponent>();
				if (pf != null) {
					return Selection.activeGameObject.GetComponent<PlayfieldComponent>() != null
						? (pf, Selection.activeGameObject.transform)
						: (pf, Selection.activeGameObject.transform.parent.transform);
				}
			}

			// if nothing selected, put it under the playfield.
			if (TableSelector.Instance.SelectedTable != null) {
				var pf = TableSelector.Instance.SelectedTable.GetComponentInChildren<PlayfieldComponent>();
				if (pf != null) {
					return (pf, pf.transform);
				}
				Debug.LogError("Cannot find playfield. You'll need to have a playfield component so the asset can be scaled correctly.");
			} else {
				Debug.LogError("No table found. You'll need to have a table with a playfield so the asset can be scaled correctly.");
			}

			return (null, null);
		}

		private void OnReplaceSelected()
		{
			var selection = Selection.gameObjects;
			var newSelection = new List<GameObject>();
			var keepName = _replaceSelectedKeepName.value;

			for (var i = selection.Length - 1; i >= 0; --i) {
				var selected = selection[i];

				var go = InstantiateAsset();
				if (go == null) {
					break;
				}

				if (keepName) {
					go.name = selected.name;
				}

				Undo.RegisterCreatedObjectUndo(go, "replace object with asset");
				go.transform.parent = selected.transform.parent;

				if (go.GetComponent(typeof(IMainRenderableComponent)) is IMainRenderableComponent comp) {
					comp.CopyFromObject(selected);

				} else {
					go.transform.localPosition = selected.transform.localPosition;
					go.transform.localRotation = selected.transform.localRotation;
					go.transform.localScale = selected.transform.localScale;
				}
				go.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
				Undo.DestroyObjectImmediate(selected);

				newSelection.Add(go);
			}

			Selection.objects = newSelection.Select(go => (Object)go).ToArray();
		}

		private GameObject InstantiateAsset(Transform parentGo = null)
		{
			var prefab = _asset.Object;
			var prefabType = PrefabUtility.GetPrefabAssetType(prefab);
			GameObject go;
			if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant) {
				go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentGo);

			} else {
				go = Object.Instantiate(prefab, parentGo) as GameObject;
			}

			if (go != null) {
				go.name = prefab.name;
			} else {
				Debug.LogError("Error instantiating prefab.");
			}

			Undo.RegisterCreatedObjectUndo(go, "add asset to scene");

			return go;
		}

		private static (int, int, int, int, int, int) CountVertices(GameObject go)
		{
			var vertices = 0;
			var triangles = 0;
			var uvs = 0;
			var meshes = 0;
			var subMeshes = 0;
			var materials = 0;
			foreach (var mf in go.GetComponentsInChildren<MeshFilter>()) {
				var mesh = mf.sharedMesh;
				if (mesh != null) {
					meshes++;
					vertices += mesh.vertexCount;
					triangles += mesh.triangles.Length;
					uvs += mesh.uv.Length;
					subMeshes += mesh.subMeshCount;
				}

				var mr = mf.gameObject.GetComponent<MeshRenderer>();
				if (mr != null) {
					materials += mr.sharedMaterials.Length;
				}
			}
			return (meshes, subMeshes, vertices, triangles, uvs, materials);
		}

		private static void SetVisibility(VisualElement element, bool isVisible)
		{
			switch (isVisible) {
				case false when !element.ClassListContains("hidden"):
					element.AddToClassList("hidden");
					break;
				case true when element.ClassListContains("hidden"):
					element.RemoveFromClassList("hidden");
					break;
			}
		}
	}
}