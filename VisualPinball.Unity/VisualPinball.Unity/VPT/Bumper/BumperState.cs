﻿// Visual Pinball Engine
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
	internal struct BumperState
	{
		internal readonly int ItemId;
		internal readonly int SkirtItemId;
		internal int RingItemId;
		internal BumperStaticData Static;
		internal BumperRingAnimationData RingAnimation;
		internal BumperSkirtAnimationData SkirtAnimation;

		public BumperState(int itemId, int skirtItemId, int ringItemId, BumperStaticData @static,
			BumperRingAnimationData ringAnimation, BumperSkirtAnimationData skirtAnimation)
		{
			ItemId = itemId;
			SkirtItemId = skirtItemId;
			RingItemId = ringItemId;
			Static = @static;
			RingAnimation = ringAnimation;
			SkirtAnimation = skirtAnimation;
		}
	}
}