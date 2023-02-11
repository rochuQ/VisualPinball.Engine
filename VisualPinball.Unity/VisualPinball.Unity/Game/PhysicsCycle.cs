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

using System;
using NativeTrees;
using Unity.Collections;

namespace VisualPinball.Unity
{
	public struct PhysicsCycle : IDisposable
	{
		private NativeList<ContactBufferElement> _contacts;
		private NativeList<PlaneCollider> _overlappingColliders;

		public PhysicsCycle(Allocator a)
		{
			_contacts = new NativeList<ContactBufferElement>(a);
			_overlappingColliders = new NativeList<PlaneCollider>(a);
		}

		internal void Simulate(float dTime, ref PhysicsState state, ref NativeOctree<PlaneCollider> octree, ref NativeList<BallData> balls)
		{
			while (dTime > 0) {
				
				var hitTime = dTime;       // begin time search from now ...  until delta ends
				
				// todo apply flipper time
				
				// clear contacts
				_contacts.Clear();

				// todo dynamic broad phase

				for (var i = 0; i < balls.Length; i++) {
					var ball = balls[i];
					
					if (ball.IsFrozen) {
						continue;
					}
					
					// static broad phase
					PhysicsStaticBroadPhase.FindOverlaps(in octree, in ball, ref _overlappingColliders);
					
					// static narrow phase
					PhysicsStaticNarrowPhase.FindNextCollision(hitTime, ref ball, _overlappingColliders, ref _contacts);

					// write ball back
					balls[i] = ball;
				}
				
				
				// todo dynamic narrow phase

				// todo apply static time

				// todo displacement
				
				// todo dynamic collision
				
				// todo static collision
				
				// todo handle contacts

				// clear contacts
				_contacts.Clear();

				// todo ball spin hack
				
				dTime -= hitTime;  
			}
		}

		public void Dispose()
		{
			_contacts.Dispose();
			_overlappingColliders.Dispose();
		}
	}
}