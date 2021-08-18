﻿// Visual Pinball Engine
// Copyright (C) 2021 freezy and VPE Team
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

// ReSharper disable AssignmentInConditionalExpression

using UnityEditor;
using VisualPinball.Engine.VPT.Plunger;

namespace VisualPinball.Unity.Editor
{
	[CustomEditor(typeof(PlungerColliderAuthoring)), CanEditMultipleObjects]
	public class PlungerColliderInspector : ItemColliderInspector<Plunger, PlungerData, PlungerAuthoring, PlungerColliderAuthoring>
	{
		private SerializedProperty _speedPullProperty;
		private SerializedProperty _speedFireProperty;
		private SerializedProperty _strokeProperty;
		private SerializedProperty _scatterVelocityProperty;
		private SerializedProperty _isMechPlungerProperty;
		private SerializedProperty _isAutoPlungerProperty;
		private SerializedProperty _mechStrengthProperty;
		private SerializedProperty _momentumXferProperty;
		private SerializedProperty _parkPositionProperty;

		protected override void OnEnable()
		{
			base.OnEnable();
			_speedPullProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.SpeedPull));
			_speedFireProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.SpeedFire));
			_strokeProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.Stroke));
			_scatterVelocityProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.ScatterVelocity));
			_isMechPlungerProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.IsMechPlunger));
			_isAutoPlungerProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.IsAutoPlunger));
			_mechStrengthProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.MechStrength));
			_momentumXferProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.MomentumXfer));
			_parkPositionProperty = serializedObject.FindProperty(nameof(PlungerColliderAuthoring.ParkPosition));
		}

		public override void OnInspectorGUI()
		{
			if (HasErrors()) {
				return;
			}

			serializedObject.Update();

			OnPreInspectorGUI();

			PropertyField(_speedPullProperty, "Pull Speed");
			PropertyField(_speedFireProperty, "Release Speed");
			PropertyField(_strokeProperty, "Stroke Length");
			PropertyField(_scatterVelocityProperty, "Scatter Velocity");

			PropertyField(_isMechPlungerProperty, "Mechanical Plunger");
			PropertyField(_isAutoPlungerProperty, "Auto Plunger");

			PropertyField(_mechStrengthProperty, "Mech Strength");
			PropertyField(_momentumXferProperty, "Momentum Xfer");
			PropertyField(_parkPositionProperty, "Park Position", onChanged: () => {
				ColliderAuthoring.GetComponentInParent<PlungerAuthoring>().UpdateParkPosition(ColliderAuthoring.ParkPosition);
			});

			base.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
		}
	}
}
