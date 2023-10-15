// Visual Pinball Engine
// Copyright (C) 2023 freezy and VPE Team
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

namespace VisualPinball.Unity
{
	internal static class HitTargetAnimation
	{
		internal static void Update(ref HitTargetAnimationData data, in HitTargetStaticData staticData,
			uint timeMsec)
		{
			var oldTimeMsec = data.TimeMsec < timeMsec ? data.TimeMsec : timeMsec;
			data.TimeMsec = timeMsec;
			var diffTimeMsec = (float)(timeMsec - oldTimeMsec);

			if (data.HitEvent) {
				data.MoveAnimation = true;
				data.HitEvent = false;
			}

			if (data.MoveAnimation) {
				var step = staticData.Speed;
				if (!data.MoveDirection) {
					step = -step;
				}

				data.XRotation += step * diffTimeMsec;
				if (data.MoveDirection) {
					if (data.XRotation >= staticData.MaxAngle) {
						data.XRotation = staticData.MaxAngle;
						data.MoveDirection = false;
					}

				} else {
					if (data.XRotation <= 0.0f) {
						data.XRotation = 0.0f;
						data.MoveAnimation = false;
						data.MoveDirection = true;
					}
				}
			}
		}
	}
}
