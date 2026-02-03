using FMODUnity;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	// Module for handling bullet delayed spawn
	public class BulletModuleSpawn : BulletModule
	{
		// Delayed spawn stats : duration and audio played when shot
		public float timeBeforeSpawn;
		public bool playAudio;
		public EventReference audioEvent;
		// memorizing position/orientation from ShotParams can be necessary if bullet is both homing and delayed
		private Vector3 deltaFromSpawn;

		public override void Enable() { base.Enable(); }
		public override void Disable() { base.Disable(); }

		// Called at Bullet.Update()
		public void Update()
		{
			timeBeforeSpawn -= Time.deltaTime;
			if (timeBeforeSpawn > 0) return;
			TriggerBulletBirth();
		}

		// Makes the bullet achieve its spawn.
		public void TriggerBulletBirth()
		{
			// Chances are targets have moved during the delay, so full restore is needed here
			if (moduleHoming.isEnabled)
			{
				moduleHoming.RefreshTarget();

				moduleHoming.MoveToTarget(moduleHoming.spawnOnTarget);
				self.position = self.position + self.up * deltaFromSpawn.y + self.right * deltaFromSpawn.x;

				moduleHoming.LookAtTarget(moduleHoming.homingSpawnRate);
				moduleMovement.Rotate(deltaFromSpawn.z);
			}

			isEnabled = false;
			if (playAudio && !audioEvent.IsNull) audioService.PlayOneShot(audioEvent, self.position);
			bullet.Prepare(true);
		}

		// Called at Bullet.ApplyBulletParams()
		public void ApplyBulletParams(BulletParams bp)
		{
			isEnabled = bp.delaySpawn;
			if (!isEnabled)
			{
				// if this module isn't enabled, before returning, if there's a SFX to be played it must be done now
				playAudio = solver.SolveDynamicBool(bp.playAudioAtSpawn, 29232405, ParameterOwner.Bullet);
				if (!playAudio) return;
				audioEvent = solver.SolveDynamicAudioEvent(bp.audioEvent, 12659374, ParameterOwner.Bullet);
				if (!audioEvent.IsNull) audioService.PlayOneShot(audioEvent, self.position);
				return;
			}

			timeBeforeSpawn = solver.SolveDynamicFloat(bp.timeBeforeSpawn, 30534841, ParameterOwner.Bullet);

			playAudio = solver.SolveDynamicBool(bp.playAudioAtSpawn, 30166684, ParameterOwner.Bullet);
			audioEvent = solver.SolveDynamicAudioEvent(bp.audioEvent, 1168027, ParameterOwner.Bullet);
		}

		// Called at Bullet.Die()
		public void Die()
		{
			isEnabled = false;
			playAudio = false;
			audioEvent = default;
			deltaFromSpawn = Vector3.zero;
		}

		// Called by patternModule if both homing and delayed
		public void MemorizeSpawnDelta(Vector3 delta)
		{
			deltaFromSpawn = delta;
		}
	}
}